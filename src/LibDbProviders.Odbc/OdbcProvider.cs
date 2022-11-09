using System;
using System.Data;
using System.Data.Odbc;
using System.Threading;
using System.Threading.Tasks;

using Bau.Libraries.LibDbProviders.Base;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.ODBC
{
    /// <summary>
    ///		Proveedor para ODBC
    /// </summary>
    public class OdbcProvider : DbProviderBase
	{ 
		public OdbcProvider(IConnectionString connectionString) : base(connectionString) 
		{
			SqlHelper = new Parser.OdbcSelectParser();
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
		protected override IDbCommand GetCommand(string text, TimeSpan? timeout = null)
		{ 
			return new OdbcCommand(text, Connection as OdbcConnection);
		}

		/// <summary>
		///		Obtiene un parámetro SQLServer a partir de un parámetro genérico
		/// </summary>
		protected override IDataParameter ConvertParameter(ParameterDb parameter)
		{ 
			if (parameter.Direction == ParameterDb.ParameterDbDirection.ReturnValue)
				return new OdbcParameter(parameter.Name, OdbcType.Int);
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
			throw new NotSupportedException($"Tipo del parámetro {parameter.Name} desconocido");
		}

		/// <summary>
		///		Obtiene el esquema
		/// </summary>
		public override async Task<Base.Schema.SchemaDbModel> GetSchemaAsync(TimeSpan timeout, CancellationToken cancellationToken)
		{
			return await new OdbcSchemaReader().GetSchemaAsync(this, timeout, cancellationToken);
		}

		/// <summary>
		///		Obtiene un datatable con el plan de ejcución de una sentencia
		/// </summary>
		public async override Task<DataTable> GetExecutionPlanAsync(string sql, ParametersDbCollection parameters, CommandType commandType, 
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
}