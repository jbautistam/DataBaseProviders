using System.Data;
using System.Data.Common;
using DuckDB.NET.Data;

using Bau.Libraries.LibDbProviders.Base;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.DuckDb;

/// <summary>
///		Proveedor para ODBC
/// </summary>
public class DuckDbProvider : DbProviderBase
{ 
	public DuckDbProvider(IConnectionString connectionString) : base(connectionString) 
	{
		SqlHelper = new Parser.DuckDbSelectParser();
	}

	/// <summary>
	///		Crea la conexión
	/// </summary>
	protected override IDbConnection GetInstance() => new DuckDBConnection(ConnectionString.ConnectionString);

	/// <summary>
	///		Obtiene un comando
	/// </summary>
	protected override DbCommand GetCommand(string text, TimeSpan? timeout = null)
	{
		if (Connection is DuckDBConnection connection)
			return new DuckDbCommand(text, connection);
		else
			throw new ArgumentException("Connection database type unknown");
	}

	/// <summary>
	///		Obtiene un parámetro SQLServer a partir de un parámetro genérico
	/// </summary>
	protected override IDataParameter ConvertParameter(ParameterDb parameter)
	{ 
		if (parameter.Direction == ParameterDb.ParameterDbDirection.ReturnValue)
			return new DuckDBParameter(parameter.Name, DbType.Int64);
		if (parameter.IsText)
			return new DuckDBParameter(parameter.Name, DbType.String);
		if (parameter.Value is bool)
			return new DuckDBParameter(parameter.Name, DbType.Boolean);
		if (parameter.Value is int)
			return new DuckDBParameter(parameter.Name, DbType.Int64);
		if (parameter.Value is double)
			return new DuckDBParameter(parameter.Name, DbType.Double);
		if (parameter.Value is string)
			return new DuckDBParameter(parameter.Name, DbType.String, parameter.Length);
		if (parameter.Value is byte [])
			return new DuckDBParameter(parameter.Name, DbType.Binary);
		if (parameter.Value is DateTime)
			return new DuckDBParameter(parameter.Name, DbType.DateTime);
		if (parameter.Value is Enum)
			return new DuckDBParameter(parameter.Name, DbType.Int32);
		throw new NotSupportedException($"Parameter type unknown {parameter.Name}");
	}

	/// <summary>
	///		Obtiene el esquema
	/// </summary>
	public override async Task<Base.Schema.SchemaDbModel> GetSchemaAsync(bool includeSystemTables, TimeSpan timeout, CancellationToken cancellationToken)
	{
		return await new Parser.DuckDbSchemaReader().GetSchemaAsync(this, includeSystemTables, timeout, cancellationToken);
	}

	/// <summary>
	///		Obtiene un datatable con el plan de ejcución de una sentencia
	/// </summary>
	public async override Task<DataTable> GetExecutionPlanAsync(string sql, ParametersDbCollection? parameters, CommandType commandType, 
																TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
	{
		await Task.Delay(1);
		return new DataTable();
	}

	/// <summary>
	///		Implementación del sistema de tratamiento de cadenas SQL
	/// </summary>
	public override Base.SqlTools.ISqlHelper SqlHelper { get; }
}