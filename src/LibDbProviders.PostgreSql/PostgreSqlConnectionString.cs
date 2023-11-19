using Bau.Libraries.LibDbProviders.Base;

namespace Bau.Libraries.LibDbProviders.PostgreSql;

/// <summary>
///		Cadena de conexión de PostgreSql
/// </summary>
public class PostgreSqlConnectionString : DbConnectionStringBase
{ 
	public PostgreSqlConnectionString() : base(string.Empty) { }

	public PostgreSqlConnectionString(string connectionString) : base(connectionString) {}

	public PostgreSqlConnectionString(Dictionary<string, string> parameters) : base(parameters) {}

	public PostgreSqlConnectionString(string server, string dataBase, int port, bool integratedSecurity, 
									  string user, string? password = null) : base(string.Empty)
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
		else if (IsEqual(key, nameof(AdditionalProperties)))
			AdditionalProperties = value;
		else if (IsEqual(key, nameof(ConnectionString)))
			ConnectionString = value;
	}

	/// <summary>
	///		Genera la cadena de conexión
	/// </summary>
	protected override string GenerateConnectionString()
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
			// Añade las propiedades adicionales
			if (!string.IsNullOrWhiteSpace(AdditionalProperties))
				connection += AdditionalProperties;
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
	public int Port { get; set; } = 5432;

	/// <summary>
	///		Indica si se debe utilizar seguridad integrada
	/// </summary>
	public bool UseIntegratedSecurity { get; set; }

	/// <summary>
	///		Usuario
	/// </summary>
	public string? User { get; set; }

	/// <summary>
	///		Contraseña
	/// </summary>
	public string? Password { get; set; }

	/// <summary>
	///		Propiedades adicionales
	/// </summary>
	public string? AdditionalProperties { get; set; }
}