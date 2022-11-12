using System;

using Bau.Libraries.LibDbProviders.Base;

namespace Bau.Libraries.LibDbProviders.MySql
{
	/// <summary>
	///		Cadena de conexión de MySql
	/// </summary>
	public class MySqlConnectionString : DbConnectionStringBase
	{ 
		public MySqlConnectionString(string connectionString) : base(connectionString) {}

		public MySqlConnectionString(Dictionary<string, string> parameters) : base(parameters) {}

		public MySqlConnectionString(string server, string dataBase, int port, string user, string? password = null) : base(string.Empty)
		{
			Server = server;
			DataBase = dataBase;
			Port = port;
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
			else if (IsEqual(key, nameof(User)))
				User = value;
			else if (IsEqual(key, nameof(Password)))
				Password = value;
			else if (IsEqual(key, nameof(ConnectionString)))
				ConnectionString = value;
		}

		/// <summary>
		///		Genera la cadena de conexión
		/// </summary>
		protected override string GenerateConnectionString()
		{
			string connection = $"server={Server};database={DataBase};port={Port};user={User};";

				// Añade la contraseña
				if (!string.IsNullOrEmpty(Password))
					connection += $"password={Password};";
				// Devuelve la cadena de conexión
				return connection;
		}

		/// <summary>
		///		Servidor
		/// </summary>
		public string? Server { get; set; }

		/// <summary>
		///		Base de datos
		/// </summary>
		public string? DataBase { get; set; }

		/// <summary>
		///		Puerto
		/// </summary>
		public int Port { get; set; } = 3306;

		/// <summary>
		///		Usuario
		/// </summary>
		public string? User { get; set; }

		/// <summary>
		///		Contraseña
		/// </summary>
		public string? Password { get; set; }
	}
}