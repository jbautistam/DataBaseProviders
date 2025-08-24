namespace Bau.Libraries.LibDbProviders.ODBC;

/// <summary>
///		Cadena de conexión de OleDB
/// </summary>
public class OdbcConnectionString : Base.DbConnectionStringBase
{ 
	public OdbcConnectionString(string connectionString) : base(connectionString) {}

	public OdbcConnectionString(Dictionary<string, string> parameters) : base(parameters) {}

	/// <summary>
	///		Asigna el valor de un parámetro
	/// </summary>
	protected override void AssignParameter(string key, string value)
	{
		if (IsEqual(key, nameof(ConnectionString)))
			ConnectionString = value;
	}

	/// <summary>
	///		Genera la cadena de conexión
	/// </summary>
	protected override string GenerateConnectionString() => ConnectionString;
}