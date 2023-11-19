using Bau.Libraries.LibDataStructures.Collections;
using Bau.Libraries.DbAggregator.Models;

namespace Bau.Libraries.DbAggregator;

/// <summary>
///		Manager para la agregación de proveedores de base de datos
/// </summary>
public class DbAggregatorManager
{
	/// <summary>
	///		Añade una conexión
	/// </summary>
	public void AddConnection(ConnectionModel connection)
	{
		ProviderModel? provider = GetDataProvider(connection);

			if (provider is not null)
				Providers.Add(connection.Key, provider);
	}

	/// <summary>
	///		Añade un proveedor
	/// </summary>
	public void AddProvider(ProviderModel provider)
	{
		Providers.Add(provider.Key, provider);
	}

	/// <summary>
	///		Obtiene el proveedor de datos
	/// </summary>
	public ProviderModel? GetProvider(string? key)
	{
		if (!string.IsNullOrWhiteSpace(key) && Providers.ContainsKey(key))
			return Providers[key];
		else
			return null;
	}

	/// <summary>
	///		Obtiene un proveedor a partir de una conexión
	/// </summary>
	private ProviderModel? GetDataProvider(ConnectionModel connection)
	{
		// Obtiene el proveedor
		if (connection is not null)
		{
			if (IsConnection(ConnectionModel.DataBaseType.SqLite))
				return GetSqLiteDataProvider(connection.Key, connection.Parameters);
			else if (IsConnection(ConnectionModel.DataBaseType.SqlServer))
				return GetSqlServerDataProvider(connection.Key, connection.Parameters);
			else if (IsConnection(ConnectionModel.DataBaseType.Odbc))
				return GetOdbcDataProvider(connection.Key, connection.Parameters);
			else if (IsConnection(ConnectionModel.DataBaseType.PostgreSql))
				return GetPostgreSqlDataProvider(connection.Key, connection.Parameters);
			else if (IsConnection(ConnectionModel.DataBaseType.MySql))
				return GetMySqlDataProvider(connection.Key, connection.Parameters);
			else if (IsConnection(ConnectionModel.DataBaseType.Spark))
				return GetSparkDataProvider(connection.Key, connection.Parameters);
		}
		// Si ha llegado hasta aquí es porque no ha encontrado ningún proveedor válido
		return null;

		// Comprueba el tipo de conexión
		bool IsConnection(ConnectionModel.DataBaseType type)
		{
			return connection.Type.Equals(type.ToString(), StringComparison.CurrentCultureIgnoreCase);
		}
	}

	/// <summary>
	///		Obtiene el proveedor agregado de SqLite
	/// </summary>
	private ProviderModel GetSqLiteDataProvider(string key, NormalizedDictionary<string> parameters)
	{
		LibDbProviders.SqLite.SqLiteConnectionString connectionString = new LibDbProviders.SqLite.SqLiteConnectionString();

			// Añade los parámetros
			connectionString.AssignParameters(parameters.ToDictionary());
			// Devuelve el proveedor
			return new ProviderModel(key, ConnectionModel.DataBaseType.SqLite.ToString(), new LibDbProviders.SqLite.SqLiteProvider(connectionString));
	}

	/// <summary>
	///		Obtiene el proveedor de datos de SqlServer
	/// </summary>
	private ProviderModel GetSqlServerDataProvider(string key, NormalizedDictionary<string> parameters)
	{
		LibDbProviders.SqlServer.SqlServerConnectionString connectionString = new LibDbProviders.SqlServer.SqlServerConnectionString();

			// Asigna los parámetros
			connectionString.AssignParameters(parameters.ToDictionary());
			// Devuelve el proveedor
			return new ProviderModel(key, ConnectionModel.DataBaseType.SqlServer.ToString(), new LibDbProviders.SqlServer.SqlServerProvider(connectionString));
	}

	/// <summary>
	///		Obtiene un proveedor de datos para ODBC
	/// </summary>
	private ProviderModel GetOdbcDataProvider(string key, NormalizedDictionary<string> parameters)
	{
		LibDbProviders.ODBC.OdbcConnectionString connectionString = new LibDbProviders.ODBC.OdbcConnectionString(string.Empty);

			// Asigna los parámetros
			connectionString.AssignParameters(parameters.ToDictionary());
			// Devuelve el proveedor
			return new ProviderModel(key, ConnectionModel.DataBaseType.Odbc.ToString(), new LibDbProviders.ODBC.OdbcProvider(connectionString));
	}

	/// <summary>
	///		Obtiene el proveedor de datos de MySql
	/// </summary>
	private ProviderModel GetMySqlDataProvider(string key, NormalizedDictionary<string> parameters)
	{
		LibDbProviders.MySql.MySqlConnectionString connectionString = new LibDbProviders.MySql.MySqlConnectionString();

			// Asigna los parámetros
			connectionString.AssignParameters(parameters.ToDictionary());
			// Devuelve el proveedor
			return new ProviderModel(key, ConnectionModel.DataBaseType.MySql.ToString(), new LibDbProviders.MySql.MySqlProvider(connectionString));
	}

	/// <summary>
	///		Obtiene el proveedor de datos de PostgreSql
	/// </summary>
	private ProviderModel GetPostgreSqlDataProvider(string key, NormalizedDictionary<string> parameters)
	{
		LibDbProviders.PostgreSql.PostgreSqlConnectionString connectionString = new LibDbProviders.PostgreSql.PostgreSqlConnectionString();

			// Asigna los parámetros
			connectionString.AssignParameters(parameters.ToDictionary());
			// Devuelve el proveedor
			return new ProviderModel(key, ConnectionModel.DataBaseType.PostgreSql.ToString(), new LibDbProviders.PostgreSql.PostgreSqlProvider(connectionString));
	}

	/// <summary>
	///		Obtiene el proveedor de datos de Spark
	/// </summary>
	private ProviderModel GetSparkDataProvider(string key, NormalizedDictionary<string> parameters)
	{
		return new ProviderModel(key, ConnectionModel.DataBaseType.Spark.ToString(), 
								 new LibDbProviders.Spark.SparkProvider(new LibDbProviders.Spark.SparkConnectionString(parameters.ToDictionary())));
	}

	/// <summary>
	///		Conexiones
	/// </summary>
	public NormalizedDictionary<ProviderModel> Providers { get; } = new NormalizedDictionary<ProviderModel>();
}