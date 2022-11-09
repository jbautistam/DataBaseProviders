using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

using Bau.Libraries.LibDbProviders.Base;
using Bau.Libraries.LibDbProviders.Base.Schema;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.SqlServer
{
    /// <summary>
    ///		Proveedor para SQL Server
    /// </summary>
    public class SqlServerProvider : DbProviderBase
	{
		public SqlServerProvider(IConnectionString connectionString) : base(connectionString) 
		{ 
			SqlHelper = new	SqlServerSelectParser(this);
		}

		/// <summary>
		///		Crea la conexión
		/// </summary>
		protected override IDbConnection GetInstance()
		{
			return new SqlConnection(ConnectionString.ConnectionString);
		}

		/// <summary>
		///		Obtiene un comando
		/// </summary>
		protected override IDbCommand GetCommand(string text, TimeSpan? timeout = null)
		{
			SqlCommand command = new SqlCommand(text, Connection as SqlConnection, Transaction as SqlTransaction);

				// Asigna el tiempo de espera al comando
				if (timeout != null)
					command.CommandTimeout = (int) (timeout ?? TimeSpan.FromMinutes(1)).TotalSeconds;
				// Devuelve el comando
				return command;
		}

		/// <summary>
		///		Convierte un parámetro
		/// </summary>
		protected override IDataParameter ConvertParameter(ParameterDb parameter)
		{
			if (parameter.Direction == ParameterDb.ParameterDbDirection.ReturnValue)
				return new SqlParameter(parameter.Name, SqlDbType.Int);
			if (parameter.Value == null || parameter.Value is DBNull)
				return new SqlParameter(parameter.Name, null);
			if (parameter.IsText)
				return new SqlParameter(parameter.Name, SqlDbType.Text);
			if (parameter.Value is bool?)
				return new SqlParameter(parameter.Name, SqlDbType.Bit);
			if (parameter.Value is int?)
				return new SqlParameter(parameter.Name, SqlDbType.Int);
			if (parameter.Value is long?)
				return new SqlParameter(parameter.Name, DbType.Int64);
			if (parameter.Value is double?)
				return new SqlParameter(parameter.Name, SqlDbType.Float);
			if (parameter.Value is decimal?)
				return new SqlParameter(parameter.Name, SqlDbType.Decimal);
			if (parameter.Value is string)
				return new SqlParameter(parameter.Name, SqlDbType.VarChar, parameter.Length);
			if (parameter.Value is byte[])
				return new SqlParameter(parameter.Name, SqlDbType.Image);
			if (parameter.Value is DateTime?)
				return new SqlParameter(parameter.Name, SqlDbType.DateTime);
			if (parameter.Value is Guid?)
				return new SqlParameter(parameter.Name, SqlDbType.UniqueIdentifier);
			if (parameter.Value is Enum)
				return new SqlParameter(parameter.Name, SqlDbType.Int);
			throw new NotSupportedException($"Unknown parameter type: {parameter.Name} ({parameter.Value.GetType().ToString()})");
		}

		/// <summary>
		///		Copia masiva de un <see cref="IDataReader"/> sobre una tabla
		/// </summary>
		public override long BulkCopy(IDataReader reader, string table, System.Collections.Generic.Dictionary<string, string> mappings, 
									  int recordsPerBlock = 50_000, TimeSpan? timeout = null)
		{
			long records = 0;

				// Copia los datos
				using (SqlBulkCopy bulkCopy = new SqlBulkCopy(Connection as SqlConnection))
				{
					// Asigna el manejador de eventos que obtiene el número de registros
					bulkCopy.NotifyAfter = 1;
					bulkCopy.SqlRowsCopied += (sender, args) => records = args.RowsCopied;
					// Asigna las propiedades
					bulkCopy.BulkCopyTimeout = (int) (timeout ?? TimeSpan.FromMinutes(5)).TotalSeconds;
					bulkCopy.BatchSize = recordsPerBlock;
					bulkCopy.DestinationTableName = table;
					bulkCopy.EnableStreaming = true;
					// Crea los mapeos predeterminados
					if (mappings == null || mappings.Count == 0)
						mappings = CreateMappings(reader);
					// Asigna los mapeos
					if (mappings != null && mappings.Count > 0)
						foreach (System.Collections.Generic.KeyValuePair<string, string> mapping in mappings)
							bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(mapping.Key, mapping.Value));
					// Escribe los datos en el servidor
					bulkCopy.WriteToServer(reader);
				}
				// Devuelve el número de registros copiados
				return records;
		}

		/// <summary>
		///		Crea los mapeos predeterminados
		/// </summary>
		private System.Collections.Generic.Dictionary<string, string> CreateMappings(IDataReader reader)
		{
			System.Collections.Generic.Dictionary<string, string> mappings = new System.Collections.Generic.Dictionary<string, string>();

				// Obtiene las columnas de mapeo
				for (int field = 0; field < reader.FieldCount; field++)
					mappings.Add(reader.GetName(field), reader.GetName(field));
				// Devuelve la colección de mapeos
				return mappings;
		}

		/// <summary>
		///		Obtiene el esquema
		/// </summary>
		public async override Task<SchemaDbModel> GetSchemaAsync(TimeSpan timeout, CancellationToken cancellationToken)
		{
			return await new Parser.SqlServerSchemaReader().GetSchemaAsync(this, timeout, cancellationToken);
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