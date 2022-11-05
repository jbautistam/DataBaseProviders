using System;
using System.Data;
using Microsoft.Data.Sqlite;

using Bau.Libraries.LibDbProviders.Base;
using Bau.Libraries.LibDbProviders.Base.Parameters;

namespace Bau.Libraries.LibDbProviders.SqLite
{
	/// <summary>
	///		Proveedor para SqLite
	/// </summary>
	public class SqLiteProvider : DbProviderBase
	{
		public SqLiteProvider(IConnectionString connectionString) : base(connectionString) 
		{ 
			SqlParser = new Parser.SqLiteSelectParser();
		}

		/// <summary>
		///		Crea la conexión
		/// </summary>
		protected override IDbConnection GetInstance()
		{
			// Asegura que exista el directorio si se tiene que crear la base de datos
			EnsurePath(ConnectionString as SqLiteConnectionString);
			// Obtiene la conexión
			return new SqliteConnection(ConnectionString.ConnectionString);
		}

		/// <summary>
		///		Si se tiene que crear la base de datos, se asegura que exista el directorio
		/// </summary>
		private void EnsurePath(SqLiteConnectionString connectionString)
		{
			if (connectionString != null &&
					connectionString.Mode == SqLiteConnectionString.OpenMode.ReadWriteCreate &&
					!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(connectionString.FileName)))
				try
				{
					System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(connectionString.FileName));
				}
				catch (Exception exception)
				{
					System.Diagnostics.Trace.TraceError($"Error when create path at {nameof(SqLiteProvider)}. {exception.Message}");
				}
		}

		/// <summary>
		///		Obtiene un comando
		/// </summary>
		protected override IDbCommand GetCommand(string sql, TimeSpan? timeOut = null)
		{
			SqliteCommand command = new SqliteCommand(sql, Connection as SqliteConnection, Transaction as SqliteTransaction);

				// Asigna el tiempo de espera al comando
				if (timeOut != null)
					command.CommandTimeout = (int) (timeOut ?? TimeSpan.FromMinutes(1)).TotalSeconds;
				// Devuelve el comando
				return command;
		}

		/// <summary>
		///		Convierte un parámetro
		/// </summary>
		protected override IDataParameter ConvertParameter(ParameterDb parameter)
		{
			// Convierte el parámetro
			if (parameter.Direction == ParameterDirection.ReturnValue)
				return new SqliteParameter(parameter.Name, SqliteType.Integer);
			if (parameter.Value == null || parameter.Value is DBNull)
				return new SqliteParameter(parameter.Name, null);
			if (parameter.IsText)
				return new SqliteParameter(parameter.Name, SqliteType.Integer);
			if (parameter.Value is bool?)
				return new SqliteParameter(parameter.Name, SqliteType.Integer);
			if (parameter.Value is int?)
				return new SqliteParameter(parameter.Name, SqliteType.Integer);
			if (parameter.Value is long?)
				return new SqliteParameter(parameter.Name, SqliteType.Integer);
			if (parameter.Value is double?)
				return new SqliteParameter(parameter.Name, SqliteType.Real);
			if (parameter.Value is string)
				return new SqliteParameter(parameter.Name, SqliteType.Text, parameter.Length);
			if (parameter.Value is byte[])
				return new SqliteParameter(parameter.Name, SqliteType.Blob);
			if (parameter.Value is DateTime?)
				return new SqliteParameter(parameter.Name, DbType.DateTime);
			if (parameter.Value is Enum)
				return new SqliteParameter(parameter.Name, SqliteType.Integer);
			// Si ha llegado hasta aquí, lanza una excepción
			throw new NotSupportedException($"Tipo del parámetro {parameter.Name} desconocido");
		}

		/// <summary>
		///		Obtiene el esquema
		/// </summary>
		public async override System.Threading.Tasks.Task<Base.Schema.SchemaDbModel> GetSchemaAsync(TimeSpan timeout, System.Threading.CancellationToken cancellationToken)
		{
			return await new Parser.SqLiteSchemaReader().GetSchemaAsync(this, timeout, cancellationToken);
		}
	}
}