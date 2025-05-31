using System.Data;

using Bau.Libraries.LibDbProviders.Base;
using Bau.Libraries.LibDbProviders.Base.Models;
using Bau.Libraries.LibDbProviders.Base.Schema;

namespace Bau.Libraries.DbAggregator.Models;

/// <summary>
///		Proveedor de bases de datos
/// </summary>
public class ProviderModel
{
	public ProviderModel(string key, string type, IDbProvider provider)
	{
		Key = key;
		Type = type;
		Provider = provider;
	}

	/// <summary>
	///		Clona el proveedor
	/// </summary>
	public ProviderModel Clone() => new ProviderModel(Key, Type, Provider);

	/// <summary>
	///		Obtiene un valor de un comando
	/// </summary>
	protected string? GetCommandValue(Dictionary<string, string> commandParameters, string key)
	{
		// Busca el comando
		foreach (KeyValuePair<string, string> command in commandParameters)
			if (command.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase))
				return command.Value;
		// Si ha llegado hasta aquí es porque no ha encontrado nada
		return null;
	}

	/// <summary>
	///		Carga una tabla de datos a partir de una cadena SQL
	/// </summary>
	public IEnumerable<DataTable> LoadData(CommandModel command, int rowsPerPage = 20_000)
	{
		int pageIndex = 0;
		long rowsReaded = 0, records;
		ParametersDbCollection parametersDB = GetParametersDb(command.Parameters);

			// Abre la conexión
			Provider.Open();
			// Obtiene el número de registros
			records = Provider.GetRecordsCount(command.Sql, parametersDB) ?? 0;
			// Recoge las páginas de datos
			while (rowsReaded < records)
			{
				DataTable table = Provider.GetDataTable(command.Sql, parametersDB, CommandType.Text, pageIndex++, rowsPerPage);

					// Incrementa el número de filas leídas
					rowsReaded += table.Rows.Count;
					// Devuelve la tabla
					yield return table;
					// Si no hay nada en la tabla, se termina para evitar bucles infinitos
					if (table.Rows.Count == 0)
						rowsReaded = records + 1;
			}
			// Cierra la conexión
			Provider.Close();
	}

	/// <summary>
	///		Carga una tabla de datos a partir de una cadena SQL
	/// </summary>
	public DataTable LoadData(CommandModel command, int pageIndex, int pageSize, out long records)
	{
		ParametersDbCollection parametersDB = GetParametersDb(command.Parameters);
		DataTable table;

			// Abre la conexión
			Provider.Open();
			// Obtiene el número de registros
			records = Provider.GetRecordsCount(command.Sql, parametersDB) ?? 0;
			// Recoge las páginas de datos
			table = Provider.GetDataTable(command.Sql, parametersDB, CommandType.Text, pageIndex, pageSize);
			// Cierra la conexión
			Provider.Close();
			// Devuelve la tabla
			return table;
	}

	/// <summary>
	///		Carga un dataReader de una base de datos
	/// </summary>
	public IDataReader OpenReader(CommandModel command, TimeSpan timeout)
	{
		ParametersDbCollection parametersDB = GetParametersDb(command.Parameters);

			// Abre la conexión
			Provider.Open();
			// Recoge las páginas de datos
			return Provider.ExecuteReader(command.Sql, parametersDB, CommandType.Text, timeout);
	}

	/// <summary>
	///		Ejecuta un comando
	/// </summary>
	public int Execute(CommandModel command)
	{
		int result;

			// Abre la conexión
			Provider.Open();
			// Ejecuta el comando
			result = Provider.Execute(command.Sql, GetParametersDb(command.Parameters), CommandType.Text, command.Timeout);
			// Cierra la conexión
			Provider.Close();
			// Devuelve el número de registros
			return result;
	}

	/// <summary>
	///		Ejecuta una serie de comandos
	/// </summary>
	public void Execute(List<CommandModel> commands)
	{
		// Abre la conexión
		Provider.Open();
		// Ejecuta los comandos
		foreach (CommandModel command in commands)
			Provider.Execute(command.Sql, GetParametersDb(command.Parameters), CommandType.Text);
		// Cierra la conexión
		Provider.Close();
	}

	/// <summary>
	///		Ejecuta un comando de lectura de un escalar
	/// </summary>
	public object? ExecuteScalar(CommandModel command)
	{
		object? result;

			// Abre la conexión
			Provider.Open();
			// Ejecuta el comando
			result = Provider.ExecuteScalar(command.Sql, GetParametersDb(command.Parameters), CommandType.Text);
			// Cierra la conexión
			Provider.Close();
			// Devuelve el número de registros
			return result;
	}

	/// <summary>
	///		Obtiene los parámetros de base de datos
	/// </summary>
	private ParametersDbCollection GetParametersDb(Dictionary<string, object?> parameters)
	{
		ParametersDbCollection parametersDB = [];

			// Asigna los parámetros
			if (parameters is not null)
				foreach (KeyValuePair<string, object?> parameter in parameters)
					parametersDB.Add(parameter.Key, parameter.Value);
			// Devuelve la colección
			return parametersDB;
	}

	/// <summary>
	///		Ejecuta una copia masiva de un <see cref="IDataReader"/> sobre una tabla
	/// </summary>
	public long BulkCopy(IDataReader dataReader, string table, Dictionary<string, string> mappings, int recordsPerBlock, TimeSpan timeout)
	{
		long records;

			// Abre la conexión
			Provider.Open();
			// Ejecuta la copia masiva
			records = Provider.BulkCopy(dataReader, NormalizeName(table), mappings, recordsPerBlock, timeout);
			// Cierra la conexión
			Provider.Close();
			// Devuelve el número de registros copiados
			return records;
	}

	/// <summary>
	///		Normaliza un nombre de tabla o registro
	/// </summary>
	private string NormalizeName(string name)
	{
		if (!name.StartsWith("["))
			return $"[{name}]";
		else
			return name;
	}

	/// <summary>
	///		Carga el esquema de base de datos
	/// </summary>
	public async Task<SchemaDbModel> LoadSchemaAsync(bool includeSystemTables, TimeSpan timeout, CancellationToken cancellationToken)
	{
		return await Provider.GetSchemaAsync(includeSystemTables, timeout, cancellationToken);
	}

	/// <summary>
	///		Código del proveedor
	/// </summary>
	public string Key { get; }

	/// <summary>
	///		Tipo del proveedor de datos
	/// </summary>
	public string Type { get; }

	/// <summary>
	///		Proveedor de base de datos
	/// </summary>
	private IDbProvider Provider { get; }
}
