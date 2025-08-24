namespace Bau.Libraries.LibDbProviders.Spark;

/// <summary>
///		Cadena de conexión de Spark
/// </summary>
public class SparkConnectionString : Base.DbConnectionStringBase
{ 
	public SparkConnectionString(string connectionString) : base(connectionString) {}

	public SparkConnectionString(Dictionary<string, string> parameters) : base(parameters) {}

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