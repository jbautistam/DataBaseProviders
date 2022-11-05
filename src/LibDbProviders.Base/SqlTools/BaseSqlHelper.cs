using System;

using Bau.Libraries.LibDbProviders.Base.Parameters;

namespace Bau.Libraries.LibDbProviders.Base.SqlTools
{
	/// <summary>
	///		Clase base para las rutinas de ayuda en el tratamiento de SQL
	/// </summary>
	public abstract class BaseSqlHelper : ISqlHelper
	{
		/// <summary>
		///		Obtiene una cadena SQL que cuenta los elementos resultantes de una consulta
		/// </summary>
		public virtual string GetSqlCount(string sql)
		{
			return $"SELECT COUNT(*) FROM ({GetSqlWithoutOrderBy(Normalize(sql))}) AS tmpQuery";
		}

		/// <summary>
		///		Elimina la sección ORDER BY de la consulta
		/// </summary>
		protected string GetSqlWithoutOrderBy(string sql)
		{
			return Cut(sql, "ORDER BY", out _).Trim();
		}

		/// <summary>
		///		Elimina caracteres no deseados de una cadena SQL (saltos de línea, espacios dobles, tabuladores)
		/// </summary>
		protected string Normalize(string sql)
		{ 
			// Normaliza una cadena SQL
			sql = sql.Replace("\t", " ");
			sql = sql.Replace("\r\n", " ");
			sql = sql.Replace("\r", " ");
			sql = sql.Replace("\n", " ");
			// Quita los espacios dobles
			while (sql.IndexOf("  ", StringComparison.InvariantCulture) >= 0)
				sql = sql.Replace("  ", " ");
			// Devuelve la cadena
			return sql;
		}

		/// <summary>
		///		Obtiene la cadena SQL necesaria para paginar
		/// </summary>
		public abstract string GetSqlPagination(string sql, int pageNumber, int pageSize);

		/// <summary>
		///		Obtiene una cadena SQL con los valores de los parámetros insertados en la propia cadena
		/// </summary>
		public string ConvertSqlNoParameters(string sql, ParametersDbCollection parametersDb)
		{
			string sqlOutput = string.Empty;
			System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(sql, "\\" + ParameterPrefix + "\\w*",
																									System.Text.RegularExpressions.RegexOptions.IgnoreCase,
																									TimeSpan.FromSeconds(1));
			int lastIndex = 0;

				// Mientras haya una coincidencia
				while (match.Success)
				{
					// Añade lo anterior del SQL a la cadena de salida y cambia el índice de último elemento encontrado
					sqlOutput += sql.Substring(lastIndex, match.Index - lastIndex);
					lastIndex = match.Index + match.Length;
					// Añade el valor del parámetro a la cadena de salida
					sqlOutput += ConvertToSqlValue(GetParameterValue(sql.Substring(match.Index, match.Length), parametersDb, 0));
					// Pasa a la siguiente coincidencia
					match = match.NextMatch();
				}
				// Añade el resto de la cadena inicial
				if (lastIndex < sql.Length)
					sqlOutput += sql.Substring(lastIndex);
				// Devuelve la colección de parámetros para la base de datos
				return sqlOutput;
		}

		/// <summary>
		///	Normaliza la SQL cambiando los parámetros por índices
		///	Obtiene los parámetros de base de datos buscando en la cadena SQL las cadenas: @xxxx (@ es el prefijo del argumento de consulta)
		/// Añade un parámetro a la colección cada vez que se encuentre un nombre de variable
		/// Se hace así porque OleDb y ODBC no admiten parámetros por nombre si no que sustituye los nombres
		/// de parámetros por posición (utilizando el marcador ?)
		/// </summary>
		public (string sql, ParametersDbCollection parametersDb) NormalizeSql(string sql, ParametersDbCollection parametersDb)
		{
			ParametersDbCollection ouputParametersDb = new ParametersDbCollection();
			string sqlOutput = string.Empty;
			System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(sql, "\\" + ParameterPrefix + "\\w*",
																									System.Text.RegularExpressions.RegexOptions.IgnoreCase,
																									TimeSpan.FromSeconds(1));
			int lastIndex = 0;

				// Mientras haya una coincidencia
				while (match.Success)
				{
					// Añade lo anterior del SQL a la cadena de salida y cambia el índice de último elemento encontrado
					sqlOutput += sql.Substring(lastIndex, match.Index - lastIndex);
					lastIndex = match.Index + match.Length;
					// Añade el marcador de parámetro
					sqlOutput += "? ";
					// Añade el parámetro a la colección de parámetros necesarios
					ouputParametersDb.Add(GetParameterValue(sql.Substring(match.Index, match.Length), parametersDb, ouputParametersDb.Count));
					// Pasa a la siguiente coincidencia
					match = match.NextMatch();
				}
				// Añade el resto de la cadena inicial
				if (lastIndex < sql.Length)
					sqlOutput += sql.Substring(lastIndex);
				// Devuelve la colección de parámetros para la base de datos
				return (sqlOutput, ouputParametersDb);
		}

		/// <summary>
		///		Convierte un parámetro al índice de ODBC
		/// </summary>
		protected ParameterDb GetParameterValue(string key, ParametersDbCollection parametersDb, int index)
		{
			// Busca el parámetro en la colección
			foreach (ParameterDb parameter in parametersDb)
				if (key.Equals(parameter.Name, StringComparison.CurrentCultureIgnoreCase) ||
						key.Equals(ParameterPrefix + parameter.Name, StringComparison.CurrentCultureIgnoreCase))
					return new ParameterDb(ParameterPrefix + parameter.Name + index.ToString(), parameter.Value, parameter.Direction, parameter.Length);
			// Si ha llegado hasta aquí, devuelve un parámetro nulo
			return new ParameterDb(ParameterPrefix + key + index.ToString(), null, System.Data.ParameterDirection.Input);
		}

		/// <summary>
		///		Convierte un objeto en una cadena para el contenido de una variable de SqlCmd
		/// </summary>
		protected string ConvertToSqlValue(ParameterDb parameterDb)
		{
			if (parameterDb.Value == null || parameterDb.Value == DBNull.Value)
				return "NULL";
			else
				switch (parameterDb.Value)
				{
					case int valueInteger:
						return ConvertIntToSql(valueInteger);
					case short valueInteger:
						return ConvertIntToSql(valueInteger);
					case long valueInteger:
						return ConvertIntToSql(valueInteger);
					case double valueDecimal:
						return ConvertDecimalToSql(valueDecimal);
					case float valueDecimal:
						return ConvertDecimalToSql(valueDecimal);
					case decimal valueDecimal:
						return ConvertDecimalToSql((double) valueDecimal);
					case string valueString:
						return ConvertStringToSql(valueString);
					case DateTime valueDate:
						return ConvertDateToSql(valueDate);
					case bool valueBool:
						return ConvertBooleanToSql(valueBool);
					default:
						return ConvertStringToSql(parameterDb.Value.ToString());
				}
		}

		/// <summary>
		///		Convierte un valor lógico a SQL
		/// </summary>
		private string ConvertBooleanToSql(bool value)
		{
			if (value)
				return "1";
			else
				return "0";
		}

		/// <summary>
		///		Convierte una fecha a SQL
		/// </summary>
		private string ConvertDateToSql(DateTime valueDate)
		{
			return $"'{valueDate:yyyy-MM-dd}'";
		}

		/// <summary>
		///		Convierte un valor decimal a Sql
		/// </summary>
		private string ConvertDecimalToSql(double value)
		{
			return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}

		/// <summary>
		///		Convierte un entero en una cadena
		/// </summary>
		private string ConvertIntToSql(long value)
		{
			return value.ToString();
		}

		/// <summary>
		///		Convierte una cadena a SQL
		/// </summary>
		private string ConvertStringToSql(string value)
		{
			return "'" + value.Replace("'", "''") + "'";
		}

		/// <summary>
		///		Corta una cadena hasta un separador. Devuelve la parte inicial de la cadena antes del separador
		///	y deja en la cadena inicial, a partir del separador
		/// </summary>
		protected string Cut(string source, string separator, out string target)
		{
			string cut = "";

				// Inicializa los valores de salida
				target = "";
				// Si hay algo que cortar ...
				if (!string.IsNullOrWhiteSpace(source))
				{
					int index = source.IndexOf(separator, StringComparison.CurrentCultureIgnoreCase);

						// Corta la cadena
						if (index < 0)
							cut = source;
						else
							cut = source.Substring(0, index);
						// Borra la cadena cortada
						if ((cut + separator).Length - 1 < source.Length)
							target = source.Substring((cut + separator).Length);
						else
							target = "";
				}
				// Devuelve la primera parte de la cadena
				return cut;
		}

		/// <summary>
		///		Formatea un nombre de campo / tabla, es decir, SeparatorStart + field + SeparatorEnd
		/// </summary>
		public string FormatName(string field)
		{
			return $"{SeparatorStart}{field}{SeparatorEnd}";
		}

		/// <summary>
		///		Formatea un nombre de campo y tabla, es decir, SeparatorStart + table + SeparatorEnd + . + SeparatorStart + field + SeparatorEnd
		/// </summary>
		public string FormatName(string table, string field)
		{
			if (string.IsNullOrWhiteSpace(table))
				return FormatName(field);
			else
				return $"{SeparatorStart}{table}{SeparatorEnd}.{SeparatorStart}{field}{SeparatorEnd}";
		}

		/// <summary>
		///		Separador inicial para el nombre de campos tablas
		/// </summary>
		public abstract string SeparatorStart { get; }

		/// <summary>
		///		Separador inicial para el nombre de campos tablas
		/// </summary>
		public abstract string SeparatorEnd { get; }

		/// <summary>
		///		Prefijos de los parámetros
		/// </summary>
		public abstract string ParameterPrefix { get; }
	}
}
