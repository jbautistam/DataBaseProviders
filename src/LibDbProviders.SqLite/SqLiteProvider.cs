using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

using Bau.Libraries.LibDbProviders.Base;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.SqLite
{
    /// <summary>
    ///		Proveedor para SqLite
    /// </summary>
    public class SqLiteProvider : DbProviderBase
	{
		public SqLiteProvider(IConnectionString connectionString) : base(connectionString) 
		{ 
			SqlHelper = new Parser.SqLiteSelectParser();
		}

		/// <summary>
		///		Crea la conexión
		/// </summary>
		protected override IDbConnection GetInstance()
		{
			return new SQLiteConnection(ConnectionString.ConnectionString);
		}

		/// <summary>
		///		Obtiene un comando
		/// </summary>
		protected override DbCommand GetCommand(string text, TimeSpan? timeout = null)
		{
			SQLiteCommand command = new SQLiteCommand(text, Connection as SQLiteConnection, Transaction as SQLiteTransaction);

				// Asigna el tiempo de espera si es necarios
				if (timeout != null)
					command.CommandTimeout  = (int) (timeout ?? TimeSpan.FromMinutes(1)).TotalSeconds;
				// Devuelve el comando
				return command;
		}

		/// <summary>
		///		Convierte un parámetro
		/// </summary>
		protected override IDataParameter ConvertParameter(ParameterDb parameter)
		{
			// Convierte el parámetro
			if (parameter.Direction == ParameterDb.ParameterDbDirection.ReturnValue)
				return new SQLiteParameter(parameter.Name, DbType.Int32);
			else if (parameter.Value is null || parameter.Value is DBNull)
				return new SQLiteParameter(parameter.Name, null);
			else if (parameter.IsText)
				return new SQLiteParameter(parameter.Name, DbType.String);
			else if (parameter.Value is bool?)
				return new SQLiteParameter(parameter.Name, DbType.Int64);
			else if (parameter.Value is int?)
				return new SQLiteParameter(parameter.Name, DbType.Int64);
			else if (parameter.Value is long?)
				return new SQLiteParameter(parameter.Name, DbType.Int64);
			else if (parameter.Value is double?)
				return new SQLiteParameter(parameter.Name, DbType.Double);
			else if (parameter.Value is float?)
				return new SQLiteParameter(parameter.Name, DbType.Double);
			else if (parameter.Value is decimal?)
				return new SQLiteParameter(parameter.Name, DbType.Double);
			else if (parameter.Value is string)
				return new SQLiteParameter(parameter.Name, DbType.String, parameter.Length);
			else if (parameter.Value is byte[])
				return new SQLiteParameter(parameter.Name, DbType.Binary);
			else if (parameter.Value is DateTime?)
				return new SQLiteParameter(parameter.Name, DbType.DateTime);
			else if (parameter.Value is Enum)
				return new SQLiteParameter(parameter.Name, DbType.Int64);
			// Si ha llegado hasta aquí, lanza una excepción
			throw new NotSupportedException($"Parameter type unknown {parameter.Name}");
		}

		/// <summary>
		///		Obtiene el esquema
		/// </summary>
		public async override Task<Base.Schema.SchemaDbModel> GetSchemaAsync(bool includeSystemTables, TimeSpan timeout, CancellationToken cancellationToken)
		{
			return await new Parser.SqLiteSchemaReader().GetSchemaAsync(this, includeSystemTables, timeout, cancellationToken);
		}

		/// <summary>
		///		Obtiene un datatable con el plan de ejcución de una sentencia
		/// </summary>
		public async override Task<DataTable> GetExecutionPlanAsync(string sql, ParametersDbCollection? parameters, CommandType commandType, 
																	TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
		{
			return await GetDataTableAsync($"EXPLAIN QUERY PLAN {sql}", parameters, commandType, timeout, cancellationToken);
		}

		/// <summary>
		///		Implementación del sistema de tratamiento de cadenas SQL
		/// </summary>
		public override Base.SqlTools.ISqlHelper SqlHelper { get; }
	}
}