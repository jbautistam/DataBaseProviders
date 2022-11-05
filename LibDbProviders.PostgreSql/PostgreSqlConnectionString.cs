using System;

using Bau.Libraries.LibDbProviders.Base;

namespace Bau.Libraries.LibDbProviders.PostgreSql
{
	/// <summary>
	///		Cadena de conexión de PostgreSql
	/// </summary>
	public class PostgreSqlConnectionString : DbConnectionStringBase
	{ 
		// Variables privadas
		private string connectionString;

		public PostgreSqlConnectionString() : base(string.Empty, 30) { }

		public PostgreSqlConnectionString(string connectionString) : base(connectionString, 30) {}

		public PostgreSqlConnectionString(System.Collections.Generic.Dictionary<string, string> parameters, int timeout = 15) : base(parameters, timeout) {}

		public PostgreSqlConnectionString(string server, string dataBase, int port, bool integratedSecurity, 
										  string user, string password = null, int timeout = 15) : base(string.Empty, timeout)
		{
			Server = server;
			DataBase = dataBase;
			Port = port;
			UseIntegratedSecurity = integratedSecurity;
			User = user;
			Password = password;
		}

		/// <summary>
		///		Asigna el valor de un parámetro
		/// </summary>
		protected override void AssignParameter(string key, string value)
		{
			if (IsEqual(key, nameof(Server)))
				Server = value;
			else if (IsEqual(key, nameof(DataBase)))
				DataBase = value;
			else if (IsEqual(key, nameof(Port)))
				Port = GetInt(value, Port);
			else if (IsEqual(key, nameof(UseIntegratedSecurity)))
				UseIntegratedSecurity = GetBool(value);
			else if (IsEqual(key, nameof(User)))
				User = value;
			else if (IsEqual(key, nameof(Password)))
				Password = value;
			else if (IsEqual(key, nameof(ConnectionString)))
				ConnectionString = value;
		}

		/// <summary>
		///		Servidor
		/// </summary>
		public string Server { get; set; }

		/// <summary>
		///		Base de datos
		/// </summary>
		public string DataBase { get; set; }

		/// <summary>
		///		Puerto
		/// </summary>
		public int Port { get; set; } = 5432;

		/// <summary>
		///		Indica si se debe utilizar seguridad integrada
		/// </summary>
		public bool UseIntegratedSecurity { get; set; }

		/// <summary>
		///		Usuario
		/// </summary>
		public string User { get; set; }

		/// <summary>
		///		Contraseña
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		///		Cadena de conexión
		/// </summary>
		/// <remarks>
		///		Ejemplo: Server=127.0.0.1;Port=5432;Database=myDataBase;User Id=myUsername;Password=myPassword;
		///		Ejemplo: Server=127.0.0.1;Port=5432;Database=myDataBase;Integrated Security=true;
		/// </remarks>
		public override string ConnectionString 
		{
			get 
			{
				if (!string.IsNullOrEmpty(connectionString))
					return connectionString;
				else 
				{ 
					string connection = $"Server={Server};Database={DataBase};";

						// Añade el puerto
						if (Port > 0)
							connection += $"port={Port};";
						// Añade las opciones de seguridad
						if (UseIntegratedSecurity)
							connection += "Integrated Security=true;";
						else
						{
							connection += $"User Id={User};";
							if (!string.IsNullOrEmpty(Password))
								connection += $"Password={Password};";
						}
						// Devuelve la cadena de conexión
						return connection;
				}
			}
			set { connectionString = value; }
		}
	}
}