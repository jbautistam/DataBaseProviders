using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;

using Bau.Libraries.LibDbProviders.Base;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.Spark
{
    /// <summary>
    ///		Proveedor para Spark
    /// </summary>
    public class SparkProvider : DbProviderBase
	{ 
		public SparkProvider(IConnectionString connectionString) : base(connectionString) 
		{
			SqlHelper = new Parser.SparkSelectParser();
		}

		/// <summary>
		///		Crea la conexión
		/// </summary>
		protected override IDbConnection GetInstance()
		{ 
			return new OdbcConnection(ConnectionString.ConnectionString);
		}

		/// <summary>
		///		Obtiene un comando
		/// </summary>
		protected override DbCommand GetCommand(string text, TimeSpan? timeout = null)
		{ 
			return new OdbcCommand(text, Connection as OdbcConnection);
		}

		/// <summary>
		///		Obtiene un parámetro SQLServer a partir de un parámetro genérico
		/// </summary>
		protected override IDataParameter ConvertParameter(ParameterDb parameter)
		{ 
			// Crea un parámetro de retorno entero
			if (parameter.Direction == ParameterDb.ParameterDbDirection.ReturnValue)
				return new OdbcParameter(parameter.Name, OdbcType.Int);
			// Asigna el tipo de parámetro
			if (parameter.IsText)
				return new OdbcParameter(parameter.Name, OdbcType.VarChar);
			if (parameter.Value is bool)
				return new OdbcParameter(parameter.Name, OdbcType.Bit);
			if (parameter.Value is int)
				return new OdbcParameter(parameter.Name, OdbcType.Int);
			if (parameter.Value is double)
				return new OdbcParameter(parameter.Name, OdbcType.Double);
			if (parameter.Value is string)
				return new OdbcParameter(parameter.Name, OdbcType.VarChar, parameter.Length);
			if (parameter.Value is byte [])
				return new OdbcParameter(parameter.Name, OdbcType.Binary);
			if (parameter.Value is DateTime)
				return new OdbcParameter(parameter.Name, OdbcType.Date);
			if (parameter.Value is Enum)
				return new OdbcParameter(parameter.Name, OdbcType.Int);
			// Devuelve un parámetro genérico
			return new OdbcParameter(parameter.Name, parameter.Value);
		}

		/// <summary>
		///		Obtiene el esquema
		/// </summary>
		public async override Task<Base.Schema.SchemaDbModel> GetSchemaAsync(TimeSpan timeout, CancellationToken cancellationToken)
		{
			return await new Parser.SparkSchemaReader().GetSchemaAsync(this, timeout, cancellationToken);
		}

		/// <summary>
		///		Obtiene un datatable con el plan de ejcución de una sentencia
		/// </summary>
		public async override Task<DataTable> GetExecutionPlanAsync(string sql, ParametersDbCollection? parameters, CommandType commandType, 
																	TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
		{
			return await GetDataTableAsync($"EXPLAIN EXTENDED {sql}", parameters, commandType, timeout, cancellationToken);
		}

		/// <summary>
		///		Implementación del sistema de tratamiento de cadenas SQL
		/// </summary>
		public override Base.SqlTools.ISqlHelper SqlHelper { get; }
	}
}