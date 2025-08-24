using Bau.Libraries.LibDbProviders.Base.SqlTools;

namespace Bau.Libraries.LibDbProviders.SqlServer.Parser;

/// <summary>
///		Parser para consultas en SQL de SQL Server
/// </summary>
internal class SqlServerSelectParser : BaseSqlHelper
{
	internal SqlServerSelectParser(SqlServerProvider connection)
	{
		Connection = connection;
	}

	/// <summary>
	///		Obtiene una cadena SQL con paginación en el servidor
	/// </summary>
	public override string GetSqlPagination(string sql, int pageNumber, int pageSize)
	{
		if (CheckIsOldVersion())
			return GetSqlPaginationOldVersions(sql, pageNumber, pageSize);
		else
			return GetSqlPaginationNewVersions(sql, pageNumber, pageSize);
	}

	/// <summary>
	///		Comprueba si la base de datos se corresponde a una versión anterior a 2012 (las ordenaciones son diferentes)
	/// </summary>
	private bool CheckIsOldVersion()
	{
		bool isOldVersion = true;
		string version = string.Empty;

			// Abre la conexión
			Connection.Open();
			// Obtiene la versión
			try
			{
				version = (string?) Connection.ExecuteScalar("SELECT SERVERPROPERTY('ProductVersion') AS ProductVersion", null, 
															 System.Data.CommandType.Text) ?? string.Empty;
			}
			catch (Exception exception)
			{
				System.Diagnostics.Trace.TraceWarning($"Can't read the SqlServer Version. {exception.Message}");
			}
			// Comprueba si es una versión antigua
			if (!string.IsNullOrWhiteSpace(version))
			{
				string[] parts = version.Split('.');

					// La versión nueva para la paginación en SQL Server comienza en la 2012 (11.xx)
					if (parts.Length > 0 && int.TryParse(parts[0], out int versionNumber) && versionNumber >= 11)
						isOldVersion = false;
			}
			// Devuelve el valor que indica si es una versión antigua
			return isOldVersion;
	}

	/// <summary>
	///		Obtiene la SQL de paginación para las nuevas versiones de SQL Server (a partir de la 2012)
	/// </summary>
	private string GetSqlPaginationNewVersions(string sql, int pageNumber, int pageSize)
	{
		return $"{sql} OFFSET {pageNumber * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";
	}

	/// <summary>
	///		Obtiene la cadena SQL de paginación para las versiones antiguas de SQL Server (hasta la 2012)
	/// </summary>
	private string GetSqlPaginationOldVersions(string sql, int pageNumber, int pageSize)
	{
		string sqlPagination;

			// Normaliza la cadena SQL
			sql = Normalize(sql);
			// Obtiene la parte Row_Number y lo incluye como campo de la SQL original.
			sqlPagination = GetRowNumberSection(sql) + ", " + GetSqlWithoutSelectAndOrderBy(sql);
			// Rodea la consulta con SELECT * FROM (strSql) y añade un alias.
			sqlPagination = $"SELECT * FROM ({sqlPagination}) AS ResultsQuery";
			// Añade la sección de paginación del tipo: WHERE RowNumber BETWEEN 11 AND 20
			return $" {sqlPagination} WHERE RowNumber BETWEEN {GetLowerLimit(pageNumber, pageSize)} AND {GetUpperLimit(pageNumber, pageSize)}";
	}

	/// <summary>
	///		Devuelve una sentencia SQL para que esté preparada para ser tratada para la paginación: sin SELECT ni ORDER BY
	/// </summary>
	private string GetSqlWithoutSelectAndOrderBy(string sql)
	{
		string initialSql = GetSqlWithoutOrderBy(sql);

			// Si la SQL tiene un distinct
			if (HasDistinctClause(sql))
				initialSql = $" * FROM ({initialSql}) AS tmp1";
			else
				initialSql = initialSql.Remove(0, "SELECT".Length);
			// Devuelve la cadena SQL
			return initialSql.Trim();
	}

	/// <summary>
	///		A partir de la cadena SQL, devuelve la sección correspondiente al ROW_NUMBER:
	/// (SELECT ROW_NUMBER() OVER (ORDER BY [strOrderBy])
	/// </summary>
	private string GetRowNumberSection(string sql)
	{
		// Quita el ORDER By de la cadena SQL
		Cut(sql, "ORDER BY", out string orderBySection);
		// Si no hay cláusula ORDER BY, incluimos en el ORDER BY necesario en la sección ROW_NUMBER el primer campo de la SELECT
		if (string.IsNullOrWhiteSpace(orderBySection))
			orderBySection = GetSqlFirstField(sql);
		// Cuando la consulta consta de un DISTINCT, se quitan las referencias a tablas del ORDER BY      
		if (HasDistinctClause(sql))
			orderBySection = RemoveTableReferencesToOrderBySection(orderBySection);
		// Devuelve la cadena SQL
		return $"SELECT ROW_NUMBER() OVER (ORDER BY {orderBySection}) AS RowNumber";
	}

	/// <summary>
	///		Devuelve el primer campo de una consulta
	/// </summary>
	private string GetSqlFirstField(string sql)
	{
		string firstField;

			// Obtiene la primera parte de la consulta, por ejemplo: SELECT tabla1.campo1
			firstField = Cut(sql, ",", out _);
			// Si tiene un DISTINCT se ha de quitar también
			if (HasDistinctClause(firstField))
				if (firstField.StartsWith("SELECT DISTINCT", StringComparison.CurrentCultureIgnoreCase))
					firstField = firstField.Remove(0, "SELECT DISTINCT".Length);
				else
					firstField = firstField.Remove(0, "SELECT".Length);
			else
				firstField = firstField.Remove(0, "SELECT".Length);
			// Si tiene un alias, obtiene el alias
			return GetAlias(firstField);
	}

	/// <summary>
	///		Elimina las referencias a las tablas de los elementos del ORDER BY
	/// </summary>
	private string RemoveTableReferencesToOrderBySection(string orderBySection)
	{
		string[] fields;

			// Obtiene los elementos de la sección ORDER BY
			orderBySection = orderBySection.Trim();
			fields = orderBySection.Split(',');
			// Si el array no contiene nada es porque solo incluye uno, con lo que se devuelve sólo ese elemento
			if (fields?.Length > 0)
			{
				string orderByWithoutTables = string.Empty;

					foreach (string field in fields)
					{
						string fieldWithoutTable = RemoveTableReferencesToElement(field.Trim());

							if (!string.IsNullOrWhiteSpace(fieldWithoutTable))
							{
								if (!string.IsNullOrWhiteSpace(orderByWithoutTables))
									orderByWithoutTables += ", ";
								orderByWithoutTables += fieldWithoutTable;
							}
					}
					return orderByWithoutTables;
			}
			else
				return orderBySection;
	}

	/// <summary>
	///		Devuelve el nombre de campo sin referencias a tablas
	/// </summary>
	private string RemoveTableReferencesToElement(string field)
	{
		string cutResult = Cut(field, ".", out string fieldWithoutTable);

			// Si no existen referencias a tablas, o el campo está precedido por dbo, 
			// se devuelve el mismo elemento enviado como parámetro.
			if (string.IsNullOrWhiteSpace(fieldWithoutTable) || cutResult.Equals("dbo", StringComparison.InvariantCultureIgnoreCase))
				return field;
			// Devuelve la cadena inicial
			return fieldWithoutTable;
	}

	/// <summary>
	///		Obtiene el límite inferior para la paginación
	/// </summary>
	protected int GetUpperLimit(int pageNumber, int pageSize) => (pageNumber + 1) * pageSize;

	/// <summary>
	///		Obtiene el límite superior para la paginación
	/// </summary>
	protected int GetLowerLimit(int pageNumber, int pageSize) => pageSize * pageNumber + 1;

	/// <summary>
	///		Indica si en la cadena SQL se incluye un Distinct
	/// </summary>
	protected bool HasDistinctClause(string sql) => sql.StartsWith("SELECT DISTINCT", StringComparison.CurrentCultureIgnoreCase);

	/// <summary>
	///		Obtiene el alias de un campo
	/// </summary>
	protected string GetAlias(string field)
	{
		if (string.IsNullOrWhiteSpace(field))
			return field;
		else
		{
			int indexStartAs = field.IndexOf(" AS ", StringComparison.InvariantCultureIgnoreCase);

				if (indexStartAs < 0)
					return field;
				else
				{
					int indexEndAs = indexStartAs + " AS ".Length;

						if (indexEndAs < field.Length)
							return field.Substring(indexEndAs);
						else
							return field;
				}
		}
	}

	/// <summary>
	///		Datos de conexión
	/// </summary>
	private SqlServerProvider Connection { get; }

	/// <summary>
	///		Separador inicial para el nombre de campos tablas
	/// </summary>
	public override string SeparatorStart { get; } = "[";

	/// <summary>
	///		Separador inicial para el nombre de campos tablas
	/// </summary>
	public override string SeparatorEnd { get; } = "]";

	/// <summary>
	///		Prefijos de los parámetros
	/// </summary>
	public override string ParameterPrefix { get; } = "@";
}