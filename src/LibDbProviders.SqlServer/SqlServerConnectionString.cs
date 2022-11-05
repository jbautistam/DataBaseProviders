using System;

using Bau.Libraries.LibDbProviders.Base;

namespace Bau.Libraries.LibDbProviders.SqlServer
{
	/// <summary>
	///		Cadena de conexión de SQL Server
	/// </summary>
	public class SqlServerConnectionString : DbConnectionStringBase
	{ 
		// Enumerados públicos
		/// <summary>
		///		Tipo de conexión
		/// </summary>
		public enum ConnectionType
		{
			/// <summary>A un servidor</summary>
			Normal,
			/// <summary>Un archivo de base de datos</summary>
			File
		}
		// Variables privadas
		private string _connectionString;

		public SqlServerConnectionString() : base(string.Empty, 30) { }

		public SqlServerConnectionString(string connectionString, int timeout = 30) : base(connectionString, timeout) { }

		public SqlServerConnectionString(string server, string user, string password, string dataBase, bool integratedSecurity, int timeout = 15) 
						: this(server, 0, user, password, dataBase, integratedSecurity, timeout) {}

		public SqlServerConnectionString(string server, int port, string user, string password, string dataBase, bool integratedSecurity, int timeout = 15) : base(string.Empty, timeout)
		{
			Type = ConnectionType.Normal;
			Server = server;
			Port = port;
			User = user;
			Password = password;
			DataBase = dataBase;
			UseIntegratedSecurity = integratedSecurity;
		}

		public SqlServerConnectionString(string server, string dataBaseFile, int timeout = 15) : base(string.Empty, timeout)
		{
			Type = ConnectionType.File;
			Server = server;
			DataBaseFile = dataBaseFile;
		}

		public SqlServerConnectionString(System.Collections.Generic.Dictionary<string, string> parameters, int timeout = 15) : base(parameters, timeout) {}

		/// <summary>
		///		Asigna el valor de un parámetro
		/// </summary>
		protected override void AssignParameter(string key, string value)
		{
			if (IsEqual(key, nameof(Type)))
				Type = GetEnum(value, ConnectionType.Normal);
			else if (IsEqual(key, nameof(Server)))
				Server = value;
			else if (IsEqual(key, nameof(Port)))
				Port = GetInt(value, Port);
			else if (IsEqual(key, nameof(UseIntegratedSecurity)))
				UseIntegratedSecurity = GetBool(value);
			else if (IsEqual(key, nameof(MultipleActiveResultSets)))
				MultipleActiveResultSets = GetBool(value);
			else if (IsEqual(key, nameof(User)))
				User = value;
			else if (IsEqual(key, nameof(Password)))
				Password = value;
			else if (IsEqual(key, nameof(DataBase)))
				DataBase = value;
			else if (IsEqual(key, nameof(DataBaseFile)))
				DataBaseFile = value;
			else if (IsEqual(key, nameof(ConnectionString)))
				ConnectionString = value;
		}

		/// <summary>
		///		Tipo de conexión
		/// </summary>
		public ConnectionType Type { get; set; }

		/// <summary>
		///		Servidor
		/// </summary>
		public string Server { get; set; }

		/// <summary>
		///		Puerto
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		///		Indica si se debe utilizar seguridad integrada
		/// </summary>
		public bool UseIntegratedSecurity { get; set; }

		/// <summary>
		///		Indica si se deben utilizar varios conjuntos de resultados
		/// </summary>
		public bool MultipleActiveResultSets { get; set; }

		/// <summary>
		///		Usuario
		/// </summary>
		public string User { get; set; }

		/// <summary>
		///		Contraseña
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		///		Base de datos
		/// </summary>
		public string DataBase { get; set; }

		/// <summary>
		///		Archivo de base de datos
		/// </summary>
		public string DataBaseFile { get; set; }

		/// <summary>
		///		Cadena de conexión
		/// </summary>
		public override string ConnectionString 
		{
			get 
			{
				string GetServerAndPort(string server, int port)
				{
					if (port < 1 || port == 1433)
						return server;
					else
						return $"{server},{port}";
				}

				if (!string.IsNullOrEmpty(_connectionString))
					return _connectionString;
				else
					switch (Type)
					{
						case ConnectionType.File:
							return $"Data Source={Server};AttachDbFilename=\"{DataBaseFile}\";Connect timeout={Timeout};User Instance=True;Integrated Security={UseIntegratedSecurity};";
						case ConnectionType.Normal:
							string connectionString = $"Data Source={GetServerAndPort(Server, Port)};Initial Catalog={DataBase};";

								// Añade datos de usuario
								if (UseIntegratedSecurity)
									connectionString += "Integrated Security=True;";
								else
									connectionString += $"Persist Security Info=True;User ID={User};Password={Password};";
								// Añade el valor que indica si se deben utilizar varios conjuntos de resultados
								if (MultipleActiveResultSets)
									connectionString += "MultipleActiveResultSets=True;";
								// Devuelve la cadena de conexión
								return connectionString;
						default:
							throw new Base.DBExceptions.DbException("Unknown connection type");
					}
			}
			set { _connectionString = value; }
		}
	}
}