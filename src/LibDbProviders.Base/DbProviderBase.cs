using System;
using System.Data;
using System.Data.Common;

using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.Base
{
    /// <summary>
    ///		Clase base para los proveedores de base de datos
    /// </summary>
    public abstract class DbProviderBase : IDbProvider
	{   
		protected DbProviderBase(IConnectionString connectionString)
		{ 
			// Inicializa la conexión
			ConnectionString = connectionString;
			// Inicializa los objetos protegidos
			Connection = null;
			Transaction = null;
		}

		/// <summary>
		///		Abre la conexión a la base de datos
		/// </summary>
		public void Open()
		{
			if (Connection is null || Connection.State != ConnectionState.Open)
			{
				// Crea la conexión
				Connection = GetInstance();
				// Abre la conexión
				Connection.Open();
			}
		}

		/// <summary>
		///		Abre la conexión a la base de datos de forma asíncrona
		/// </summary>
		public async Task OpenAsync(CancellationToken cancellationToken)
		{
			if (Connection is null || Connection.State != ConnectionState.Open)
			{
				// Crea la conexión
				Connection = GetInstance();
				// Abre la conexión
				if (Connection is DbConnection connection)
					await connection.OpenAsync(cancellationToken);
			}
		}

		/// <summary>
		///		Crea la conexión
		/// </summary>
		protected abstract IDbConnection GetInstance();

		/// <summary>
		///		Ejecuta una sentencia o procedimiento sobre la base de datos y devuelve un escalar de forma asíncrona
		/// </summary>
		private DbCommand PrepareCommand(QueryModel query)
		{
			DbCommand command = GetSqlCommand(query);

				// Indica el tipo de comando
				command.CommandType = ConvertCommandType(query.Type);
				command.CommandTimeout = GetTimeout(query.Timeout);
				// Añade los parámetros al comando
				AddParameters(command, query.Parameters);
				// Devuelve el comando
				return command;

				// Obtiene el comando teniendo en cuenta la paginación
				DbCommand GetSqlCommand(QueryModel query)
				{
					if (query.Pagination.MustPaginate && query.Type == QueryModel.QueryType.Text && SqlHelper != null)
						return GetCommand(SqlHelper.GetSqlPagination(query.Sql, query.Pagination.Page - 1, query.Pagination.PageSize), 
										  query.Timeout);
					else
						return GetCommand(query.Sql, query.Timeout);
				}
		}

		/// <summary>
		///		Obtiene un comando
		/// </summary>
		protected abstract DbCommand GetCommand(string sql, TimeSpan? timeout);

		/// <summary>
		///		Cierra la conexión a la base de datos
		/// </summary>
		public virtual void Close()
		{
			if (Connection?.State == ConnectionState.Open && Transaction == null)
				Connection.Close();
		}

		/// <summary>
		///		Ejecuta una sentencia o un procedimiento sobre la base de datos
		/// </summary>
		public int Execute(QueryModel query)
		{
			int rows;

				// Ejecuta la consulta
				using (DbCommand command = PrepareCommand(query))
				{ 
					// Ejecuta la consulta
					rows = command.ExecuteNonQuery();
					// Pasa los valores de salida de los parámetros del comando a la colección de parámetros de entrada
					if (query.Parameters is not null)
					{
						query.Parameters.Clear();
						query.Parameters.AddRange(ReadOutputParameters(command.Parameters));
					}
				}
				// Devuelve el número de registros afectados
				return rows;
		}

		/// <summary>
		///		Ejecuta una sentencia o un procedimiento sobre la base de datos
		/// </summary>
		public int Execute(string sql, ParametersDbCollection parameters, CommandType commandType, TimeSpan? timeout = null)
		{
			return Execute(ConvertQuery(sql, parameters, commandType, 0, 0, timeout));
		}

		/// <summary>
		///		Ejecuta una sentencia o un procedimiento sobre la base de datos de forma asíncrona
		/// </summary>
		public async Task<int> ExecuteAsync(QueryModel query, CancellationToken? cancellationToken = null)
		{
			int rows;

				// Ejecuta la consulta
				using (DbCommand command = PrepareCommand(query))
				{ 
					// Ejecuta la consulta
					rows = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
					// Pasa los valores de salida de los parámetros del comando a la colección de parámetros de entrada
					if (query.Parameters is not null)
					{
						query.Parameters.Clear();
						query.Parameters.AddRange(ReadOutputParameters(command.Parameters));
					}
				}
				// Devuelve el número de registros afectados
				return rows;
		}

		/// <summary>
		///		Ejecuta una sentencia o un procedimiento sobre la base de datos de forma asíncrona
		/// </summary>
		public async Task<int> ExecuteAsync(string sql, ParametersDbCollection parameters, CommandType commandType, TimeSpan? timeout = null,
											CancellationToken? cancellationToken = null)
		{
			return await ExecuteAsync(ConvertQuery(sql, parameters, commandType, 0, 0, timeout));
		}

		/// <summary>
		///		Obtiene un DataReader
		/// </summary>
		public IDataReader ExecuteReader(string sql, ParametersDbCollection parametersDB, CommandType commandType, TimeSpan? timeout = null)
		{
			return ExecuteReader(ConvertQuery(sql, parametersDB, commandType, 0, 0, timeout));
		}

		/// <summary>
		///		Obtiene un IDataReader a partir de un nombre de una sentencia o procedimiento y sus parámetros paginando
		///	en el servidor
		/// </summary>
		/// <remarks>
		///		Sólo está implementado totalmente para los comandos de texto, no para los procedimientos almacenados
		/// </remarks>
		public IDataReader ExecuteReader(QueryModel query)
		{
			return PrepareCommand(query).ExecuteReader();
		}

		/// <summary>
		///		Obtiene un DataReader de forma asíncrona
		/// </summary>
		public async Task<DbDataReader> ExecuteReaderAsync(string sql, ParametersDbCollection parametersDB, CommandType commandType, 
														   TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
		{
			return await ExecuteReaderAsync(ConvertQuery(sql, parametersDB, commandType, 0, 0, timeout), cancellationToken);
		}

		/// <summary>
		///		Obtiene un DataReader de forma asíncrona
		/// </summary>
		public async Task<DbDataReader> ExecuteReaderAsync(QueryModel query, CancellationToken? cancellationToken = null)
		{
			return await PrepareCommand(query).ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
		}

		/// <summary>
		///		Obtiene un IDataReader a partir de un nombre de una sentencia o procedimiento y sus parámetros paginando
		///	en el servidor
		/// </summary>
		/// <remarks>
		///		Sólo está implementado totalmente para los comandos de texto, no para los procedimientos almacenados
		/// </remarks>
		public IDataReader ExecuteReader(string sql, ParametersDbCollection parameters, CommandType commandType, int pageNumber, int pageSize, TimeSpan? timeout = null)
		{
			return ExecuteReader(ConvertQuery(sql, parameters, commandType, pageNumber, pageSize, timeout));
		}

		/// <summary>
		///		Obtiene un IDataReader a partir de un nombre de una sentencia o procedimiento y sus parámetros paginando
		///	en el servidor de forma asíncrona
		/// </summary>
		/// <remarks>
		///		Sólo está implementado totalmente para los comandos de texto, no para los procedimientos almacenados
		/// </remarks>
		public async Task<DbDataReader> ExecuteReaderAsync(string sql, ParametersDbCollection parameters, CommandType commandType, int pageNumber, int pageSize, 
														   TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
		{
			return await ExecuteReaderAsync(ConvertQuery(sql, parameters, commandType, pageNumber, pageSize, timeout), cancellationToken);
		}

		/// <summary>
		///		Ejecuta una sentencia o procedimiento sobre la base de datos y devuelve un escalar
		/// </summary>
		public object? ExecuteScalar(QueryModel query)
		{
			return PrepareCommand(query).ExecuteScalar();
		}

		/// <summary>
		///		Ejecuta una sentencia o procedimiento sobre la base de datos y devuelve un escalar
		/// </summary>
		public object? ExecuteScalar(string sql, ParametersDbCollection parameters, CommandType commandType, TimeSpan? timeout = null)
		{
			return ExecuteScalar(ConvertQuery(sql, parameters, commandType, 0, 0, timeout));
		}

		/// <summary>
		///		Ejecuta una sentencia o procedimiento sobre la base de datos y devuelve un escalar de forma asíncrona
		/// </summary>
		public async Task<object?> ExecuteScalarAsync(QueryModel query, CancellationToken? cancellationToken = null)
		{
			return await PrepareCommand(query).ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
		}

		/// <summary>
		///		Ejecuta una sentencia o procedimiento sobre la base de datos y devuelve un escalar de forma asíncrona
		/// </summary>
		public async Task<object?> ExecuteScalarAsync(string sql, ParametersDbCollection parameters, CommandType commandType, 
													 TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
		{
			return await ExecuteScalarAsync(ConvertQuery(sql, parameters, commandType, 0, 0, timeout), cancellationToken);
		}

		/// <summary>
		///		Obtiene un dataTable a partir de un nombre de una sentencia o procedimiento y sus parámetros
		/// </summary>
		public DataTable GetDataTable(QueryModel query)
		{
			DataTable table = new DataTable();

				// Carga los datos de la tabla
				table.Load(ExecuteReader(query), LoadOption.OverwriteChanges);
				// Devuelve la tabla
				return table;
		}

		/// <summary>
		///		Obtiene un dataTable a partir de un nombre de una sentencia o procedimiento y sus parámetros
		/// </summary>
		public DataTable GetDataTable(string sql, ParametersDbCollection parameters, CommandType commandType, TimeSpan? timeout = null)
		{
			return GetDataTable(ConvertQuery(sql, parameters, commandType, 0, 0, timeout));
		}

		/// <summary>
		///		Obtiene un dataTable a partir de un nombre de una sentencia o procedimiento y sus parámetros de forma asíncrona
		/// </summary>
		public async Task<DataTable> GetDataTableAsync(QueryModel query, CancellationToken? cancellationToken = null)
		{
			DataTable table = new DataTable();

				// Carga los datos de la tabla
				table.Load(await ExecuteReaderAsync(query, cancellationToken), LoadOption.OverwriteChanges);
				// Devuelve la tabla
				return table;
		}

		/// <summary>
		///		Obtiene un dataTable a partir de un nombre de una sentencia o procedimiento y sus parámetros de forma asíncrona
		/// </summary>
		public async Task<DataTable> GetDataTableAsync(string sql, ParametersDbCollection parameters, CommandType commandType, 
													   TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
		{
			return await GetDataTableAsync(ConvertQuery(sql, parameters, commandType, 0, 0, timeout), cancellationToken);
		}

		/// <summary>
		///		Obtiene un dataTable a partir de un nombre de una sentencia o procedimiento y sus parámetros
		/// </summary>
		public DataTable GetDataTable(string sql, ParametersDbCollection parameters, CommandType commandType, int pageNumber, int pageSize, TimeSpan? timeout = null)
		{
			return GetDataTable(ConvertQuery(sql, parameters, commandType, pageNumber, pageSize, timeout));
		}

		/// <summary>
		///		Obtiene un dataTable a partir de un nombre de una sentencia o procedimiento y sus parámetros
		/// </summary>
		public async Task<DataTable> GetDataTableAsync(string sql, ParametersDbCollection parameters, CommandType commandType, int pageNumber, int pageSize, 
													   TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
		{
			return await GetDataTableAsync(ConvertQuery(sql, parameters, commandType, pageNumber, pageSize, timeout), cancellationToken);
		}

		/// <summary>
		///		Obtiene un datatable con el plan de ejcución de una sentencia
		/// </summary>
		public async Task<DataTable> GetExecutionPlanAsync(QueryModel query, CancellationToken? cancellationToken = null)
		{
			return await GetExecutionPlanAsync(query.Sql, query.Parameters, ConvertCommandType(query.Type), query.Timeout, cancellationToken);
		}

		/// <summary>
		///		Obtiene un datatable con el plan de ejcución de una sentencia
		/// </summary>
		public abstract Task<DataTable> GetExecutionPlanAsync(string sql, ParametersDbCollection parameters, CommandType commandType, 
															  TimeSpan? timeout = null, CancellationToken? cancellationToken = null);

		/// <summary>
		///		Obtiene el número de registros de una consulta
		/// </summary>
		public long? GetRecordsCount(QueryModel query)
		{
			if (SqlHelper is null || query.Type != QueryModel.QueryType.Text)
				return null;
			else
			{
				object? result = ExecuteScalar(ConvertQuery(SqlHelper.GetSqlCount(query.Sql), query.Parameters, CommandType.Text,
															0, 0, query.Timeout));

					// Normaliza el resultado
					if (result is null)
						return null;
					else if (result is long)
						return (long?) result;
					else
						return (int?) result;
			}
		}

		/// <summary>
		///		Obtiene el número de registros de una consulta
		/// </summary>
		public long? GetRecordsCount(string sql, ParametersDbCollection parametersDB, TimeSpan? timeout = null)
		{
			return GetRecordsCount(ConvertQuery(sql, parametersDB, CommandType.Text, 0, 0, timeout));
		}

		/// <summary>
		///		Obtiene el número de registros de una consulta
		/// </summary>
		public async Task<long?> GetRecordsCountAsync(QueryModel query, CancellationToken? cancellationToken = null)
		{
			if (SqlHelper == null)
				return null;
			else
			{
				object? result = await ExecuteScalarAsync(ConvertQuery(SqlHelper.GetSqlCount(query.Sql), query.Parameters, CommandType.Text,
																	   0, 0, query.Timeout), 
														  cancellationToken);

					// Normaliza el resultado
					if (result == null)
						return null;
					else if (result is long)
						return (long?) result;
					else
						return (int?) result;
			}
		}

		/// <summary>
		///		Obtiene el número de registros de una consulta
		/// </summary>
		public async Task<long?> GetRecordsCountAsync(string sql, ParametersDbCollection parametersDB, 
													  TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
		{
			return await GetRecordsCountAsync(ConvertQuery(sql, parametersDB, CommandType.Text, 0, 0, timeout), cancellationToken);
		}

		/// <summary>
		///		Copia masiva de datos en una tabla
		/// </summary>
		public virtual long BulkCopy(IDataReader reader, string table, Dictionary<string, string> mappings, 
									 int recordsPerBlock = 30_000, TimeSpan? timeout = null)
		{
			return new SqlTools.SqlBulkCopy().Process(this, reader, table, mappings, recordsPerBlock, timeout);
		}

		/// <summary>
		///		Copia masiva de datos en una tabla de forma asíncrona
		/// </summary>
		public async Task<long> BulkCopyAsync(IDataReader reader, string table, Dictionary<string, string> mappings, int recordsPerBlock = 30000, 
											  TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
		{
			return await new SqlTools.SqlBulkCopy().ProcessAsync(this, reader, table, mappings, recordsPerBlock, 
																 timeout ?? TimeSpan.FromMinutes(60), cancellationToken ?? CancellationToken.None);
		}

		/// <summary>
		///		Añade a un comando los parámetros de una clase <see cref="ParametersDbCollection"/>
		/// </summary>
		protected void AddParameters(DbCommand command, ParametersDbCollection parameters)
		{ 
			// Limpia los parámetros antiguos
			command.Parameters.Clear();
			// Añade los parámetros nuevos
			if (parameters != null)
				foreach (ParameterDb parameter in parameters)
					command.Parameters.Add(GetSQLParameter(parameter));
		}

		/// <summary>
		///		Obtiene un parámetro a partir de un parámetro genérico
		/// </summary>
		private IDataParameter GetSQLParameter(ParameterDb parameter)
		{
			IDataParameter parameterDB = ConvertParameter(parameter);

				// Asigna el valor
				if ((parameterDB.DbType == DbType.AnsiString || parameterDB.DbType == DbType.AnsiStringFixedLength ||
						 parameterDB.DbType == DbType.String || parameterDB.DbType == DbType.StringFixedLength) &&
						 string.IsNullOrEmpty(parameter.Value as string))
					parameterDB.Value = DBNull.Value;
				else
					parameterDB.Value = parameter.GetDBValue();
				// Asigna la dirección
				switch (parameter.Direction)
				{
					case ParameterDb.ParameterDbDirection.InputOutput:
							parameterDB.Direction = ParameterDirection.InputOutput;
						break;
					case ParameterDb.ParameterDbDirection.Output:
							parameterDB.Direction = ParameterDirection.Output;
						break;
					case ParameterDb.ParameterDbDirection.ReturnValue:
							parameterDB.Direction = ParameterDirection.ReturnValue;
						break;
					default:
							parameterDB.Direction = ParameterDirection.Input;
						break;
				}
				// Devuelve el parámetro
				return parameterDB;
		}

		/// <summary>
		///		Método abstracto para convertir un parámetro
		/// </summary>
		protected abstract IDataParameter ConvertParameter(ParameterDb parameter);

		/// <summary>
		///		Lee los parámetros de salida
		/// </summary>
		private ParametersDbCollection ReadOutputParameters(IDataParameterCollection outputParameters)
		{
			ParametersDbCollection parameters = new ParametersDbCollection();

				// Recupera los parámetros
				foreach (IDataParameter outputParameter in outputParameters)
					parameters.Add(new ParameterDb(outputParameter.ParameterName, outputParameter.Value, ParameterDb.ParameterDbDirection.Output));
				// Devuelve la colección de parámetros
				return parameters;
		}

		/// <summary>
		///		Inicia una transacción
		/// </summary>
		public void BeginTransaction()
		{ 
			// Abre la conexión si no estaba abierta
			Open();
			// Inicia la transacción
			Transaction = Connection?.BeginTransaction();
		}

		/// <summary>
		///		Confirma una transacción
		/// </summary>
		public void Commit()
		{
			Transaction?.Commit();
			Transaction = null;
		}

		/// <summary>
		///		Deshace una transacción
		/// </summary>
		public void RollBack()
		{
			Transaction?.Rollback();
			Transaction = null;
		}

		/// <summary>
		///		Obtiene el timeout de la conexión
		/// </summary>
		private int GetTimeout(TimeSpan? timeout)
		{
			return (int) (timeout ?? TimeSpan.FromMinutes(1)).TotalSeconds;
		}

		/// <summary>
		///		Convierte una serie de propiedades en una consulta
		/// </summary>
		private QueryModel ConvertQuery(string sql, ParametersDbCollection parameters, CommandType commandType, int page, int pageSize, TimeSpan? timeout)
		{
			Builders.QueryBuilder builder = new();

				// Añade la SQL
				builder.WithSql(sql, ConvertCommandType(commandType))
					   .WithParameters(parameters);
				// Añade la paginación
				if (page > 0 && pageSize > 0)
					builder.WithPagination(page, pageSize);
				// Añade el timeout
				if (timeout is not null)
					builder.WithTimeout(timeout.Value);
				// Devuelve la consulta generada
				return builder.Build();
		}

		/// <summary>
		///		Obtiene el esquema de base de datos de forma asíncrona
		/// </summary>
		public abstract Task<Schema.SchemaDbModel> GetSchemaAsync(TimeSpan timeout, CancellationToken cancellationToken);
		
		/// <summary>
		///		Convierte un tipo de comando
		/// </summary>
		private CommandType ConvertCommandType(QueryModel.QueryType type)
		{
			switch (type)
			{
				case QueryModel.QueryType.StoredProcedure:
					return CommandType.StoredProcedure;
				case QueryModel.QueryType.Table:
					return CommandType.TableDirect;
				default:
					return CommandType.Text;
			}
		}
		
		/// <summary>
		///		Convierte un tipo de comando
		/// </summary>
		private QueryModel.QueryType ConvertCommandType(CommandType type)
		{
			switch (type)
			{
				case CommandType.StoredProcedure:
					return QueryModel.QueryType.StoredProcedure;
				case CommandType.TableDirect:
					return QueryModel.QueryType.Table;
				default:
					return QueryModel.QueryType.Text;
			}
		}
		/// <summary>
		///		Desconecta la conexión
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///		Desconecta la conexión
		/// </summary>
		private void Dispose(bool disposing)
		{
			if (disposing && Connection != null)
			{
				Close();
				Connection.Dispose();
			}
		}

		/// <summary>
		///		Cadena de conexión
		/// </summary>
		public IConnectionString ConnectionString { get; set; }

		/// <summary>
		///		Parser para consultas SQL
		/// </summary>
		public abstract SqlTools.ISqlHelper SqlHelper { get; }

		/// <summary>
		///		Conexión
		/// </summary>
		protected IDbConnection? Connection { get; set; }

		/// <summary>
		///		Transacción
		/// </summary>
		public IDbTransaction? Transaction { get; protected set; }
	}
}