using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.Base
{
    /// <summary>
    ///		Interface para los proveedores de datos
    /// </summary>
    public interface IDbProvider : IDisposable
	{
		/// <summary>
		///		Abre la conexión
		/// </summary>
		void Open();

		/// <summary>
		///		Abre la conexión de forma asíncrona
		/// </summary>
		Task OpenAsync(CancellationToken cancellationToken);

		/// <summary>
		///		Cierra la conexión
		/// </summary>
		void Close();

		/// <summary>
		///		Ejecuta una sentencia o un procedimiento sobre la base de datos
		/// </summary>
		/// <returns>
		/// Devuelve el número de registros procesados o -1 si se ha ejecutado un Rollback o la cadena SQL es una
		///	instrucción DML
		///	</returns>
		int Execute(string sql, ParametersDbCollection parameters, CommandType commandType, TimeSpan? timeout = null);

		/// <summary>
		///		Ejecuta una sentencia o un procedimiento sobre la base de datos de forma asíncrona
		/// </summary>
		Task<int> ExecuteAsync(string sql, ParametersDbCollection parameters, CommandType commandType, TimeSpan? timeout = null,
							   CancellationToken? cancellationToken = null);

		/// <summary>
		///		Obtiene un DataReader
		/// </summary>
		IDataReader ExecuteReader(string sql, ParametersDbCollection parametersDB, CommandType commandType, TimeSpan? timeout = null);

		/// <summary>
		///		Obtiene un DataReader de forma asíncrona
		/// </summary>
		Task<DbDataReader> ExecuteReaderAsync(string sql, ParametersDbCollection parametersDB, CommandType commandType, TimeSpan? timeout = null,
											  CancellationToken? cancellationToken = null);

		/// <summary>
		///		Obtiene un IDataReader a partir de un nombre de una sentencia o procedimiento y sus parámetros paginando
		///	en el servidor
		/// </summary>
		IDataReader ExecuteReader(string sql, ParametersDbCollection parameters, CommandType commandType, int pageNumber, int pageSize, TimeSpan? timeout = null);

		/// <summary>
		///		Obtiene un IDataReader a partir de un nombre de una sentencia o procedimiento y sus parámetros paginando
		///	en el servidor de forma asíncrona
		/// </summary>
		Task<DbDataReader> ExecuteReaderAsync(string sql, ParametersDbCollection parameters, CommandType commandType, int pageNumber, int pageSize, 
											  TimeSpan? timeout = null, CancellationToken? cancellationToken = null);

		/// <summary>
		///		Ejecuta una sentencia o procedimiento sobre la base de datos y devuelve un escalar
		/// </summary>
		object ExecuteScalar(string sql, ParametersDbCollection parameters, CommandType commandType, TimeSpan? timeout = null);

		/// <summary>
		///		Ejecuta una sentencia o procedimiento sobre la base de datos y devuelve un escalar de forma asíncrona
		/// </summary>
		Task<object> ExecuteScalarAsync(string sql, ParametersDbCollection parameters, CommandType commandType, 
										TimeSpan? timeout = null, CancellationToken? cancellationToken = null);

		/// <summary>
		///		Obtiene un dataTable a partir de un nombre de una sentencia o procedimiento y sus parámetros
		/// </summary>
		DataTable GetDataTable(string sql, ParametersDbCollection parameters, CommandType commandType, TimeSpan? timeout = null);

		/// <summary>
		///		Obtiene un dataTable a partir de un nombre de una sentencia o procedimiento y sus parámetros de forma asíncrona
		/// </summary>
		Task<DataTable> GetDataTableAsync(string sql, ParametersDbCollection parameters, CommandType commandType, 
										  TimeSpan? timeout = null, CancellationToken? cancellationToken = null);

		/// <summary>
		///		Obtiene un dataTable a partir de un nombre de una sentencia o procedimiento y sus parámetros
		/// </summary>
		DataTable GetDataTable(string sql, ParametersDbCollection parameters, CommandType commandType, int pageNumber, int pageSize, TimeSpan? timeout = null);

		/// <summary>
		///		Obtiene un dataTable a partir de un nombre de una sentencia o procedimiento y sus parámetros de forma asíncrona
		/// </summary>
		Task<DataTable> GetDataTableAsync(string sql, ParametersDbCollection parameters, CommandType commandType, int pageNumber, int pageSize, 
										  TimeSpan? timeout = null, CancellationToken? cancellationToken = null);

		/// <summary>
		///		Obtiene un datatable con el plan de ejcución de una sentencia
		/// </summary>
		Task<DataTable> GetExecutionPlanAsync(string sql, ParametersDbCollection parameters, CommandType commandType, 
											  TimeSpan? timeout = null, CancellationToken? cancellationToken = null);

		/// <summary>
		///		Obtiene el número de registros resultantes de una consulta SQL
		/// </summary>
		long? GetRecordsCount(string sql, ParametersDbCollection parametersDB, TimeSpan? timeout = null);

		/// <summary>
		///		Obtiene el número de registros resultantes de una consulta SQL de forma asíncrona
		/// </summary>
		Task<long?> GetRecordsCountAsync(string sql, ParametersDbCollection parametersDB, TimeSpan? timeout = null, CancellationToken? cancellationToken = null);

		/// <summary>
		///		Copia masiva a una tabla
		/// </summary>
		long BulkCopy(IDataReader reader, string table, System.Collections.Generic.Dictionary<string, string> mappings, 
					  int recordsPerBlock = 30_000, TimeSpan? timeout = null);

		/// <summary>
		///		Copia masiva a una tabla de forma asíncrona
		/// </summary>
		Task<long> BulkCopyAsync(IDataReader reader, string table, System.Collections.Generic.Dictionary<string, string> mappings, 
								 int recordsPerBlock = 30_000, TimeSpan? timeout = null, CancellationToken? cancellationToken = null);

		/// <summary>
		///		Inicia una transacción
		/// </summary>
		void BeginTransaction();

		/// <summary>
		///		Confirma una transacción
		/// </summary>
		void Commit();

		/// <summary>
		///		Deshace una transacción
		/// </summary>
		void RollBack();

		/// <summary>
		///		Obtiene el esquema de la base de datos de forma asíncrona
		/// </summary>
		Task<Schema.SchemaDbModel> GetSchemaAsync(TimeSpan timeout, CancellationToken cancellationToken);

		/// <summary>
		///		Parámetros de conexión
		/// </summary>
		IConnectionString ConnectionString { get; set; }

		/// <summary>
		///		Transacción actual
		/// </summary>
		IDbTransaction Transaction { get; }

		/// <summary>
		///		Clase para tratamiento adicional de cadenas SQL
		/// </summary>
		SqlTools.ISqlHelper SqlHelper { get; }
	}
}