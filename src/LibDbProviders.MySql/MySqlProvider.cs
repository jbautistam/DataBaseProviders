using System.Data;
using System.Data.Common;

using MySql.Data.MySqlClient;
using Bau.Libraries.LibDbProviders.Base;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.MySql;

/// <summary>
///		Proveedor para MySql
/// </summary>
public class MySqlProvider : DbProviderBase
{
	public MySqlProvider(IConnectionString connectionString) : base(connectionString) 
	{ 
		SqlHelper = new Parser.MySqlSelectParser();
	}

	/// <summary>
	///		Crea la conexión
	/// </summary>
	protected override IDbConnection GetInstance() => new MySqlConnection(ConnectionString.ConnectionString);

	/// <summary>
	///		Obtiene un comando
	/// </summary>
	protected override DbCommand GetCommand(string text, TimeSpan? timeout = null)
	{
		return new MySqlCommand(text, Connection as MySqlConnection, Transaction as MySqlTransaction);
	}

	/// <summary>
	///		Convierte un parámetro
	/// </summary>
	protected override IDataParameter ConvertParameter(ParameterDb parameter)
	{
		// Convierte el parámetro
		if (parameter.Direction == ParameterDb.ParameterDbDirection.ReturnValue)
			return new MySqlParameter(parameter.Name, MySqlDbType.Int32);
		if (parameter.Value == null)
			return new MySqlParameter(parameter.Name, null);
		if (parameter.IsText)
			return new MySqlParameter(parameter.Name, MySqlDbType.String);
		if (parameter.Value is bool?)
			return new MySqlParameter(parameter.Name, MySqlDbType.Bit);
		if (parameter.Value is int?)
			return new MySqlParameter(parameter.Name, MySqlDbType.Int64);
		if (parameter.Value is double?)
			return new MySqlParameter(parameter.Name, MySqlDbType.Double);
		if (parameter.Value is string)
			return new MySqlParameter(parameter.Name, MySqlDbType.VarString, parameter.Length);
		if (parameter.Value is byte[])
			return new MySqlParameter(parameter.Name, MySqlDbType.Byte);
		if (parameter.Value is DateTime?)
			return new MySqlParameter(parameter.Name, MySqlDbType.DateTime);
		if (parameter.Value is Enum)
			return new MySqlParameter(parameter.Name, MySqlDbType.Int16);
		// Si ha llegado hasta aquí, lanza una excepción
		throw new NotSupportedException($"Parameter type unknown {parameter.Name}");
	}

	/// <summary>
	///		Obtiene el esquema
	/// </summary>
	public async override Task<Base.Schema.SchemaDbModel> GetSchemaAsync(Base.Schema.SchemaOptions options, TimeSpan timeout, CancellationToken cancellationToken)
	{
		return await new Parser.MySqlSchemaReader().GetSchemaAsync(this, options, timeout, cancellationToken);
	}

	/// <summary>
	///		Obtiene un datatable con el plan de ejcución de una sentencia
	/// </summary>
	public async override Task<DataTable> GetExecutionPlanAsync(string sql, ParametersDbCollection? parameters, CommandType commandType, 
																TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
	{
		return await GetDataTableAsync($"EXPLAIN ANALYZE {sql}", parameters, commandType, timeout, cancellationToken);
	}

	/// <summary>
	///		Implementación del sistema de tratamiento de cadenas SQL
	/// </summary>
	public override Base.SqlTools.ISqlHelper SqlHelper { get; }
}