namespace Bau.Libraries.LibDbProviders.DuckDb;

/// <summary>
///		Cadena de conexión a DuckDb
/// </summary>
public class DuckDbConnectionString : Base.DbConnectionStringBase
{ 
	public DuckDbConnectionString(string connectionString) : base(connectionString) {}

	public DuckDbConnectionString(Dictionary<string, string> parameters) : base(parameters) {}

	public DuckDbConnectionString(bool inMemory, string? fileName)
	{
		InMemory = inMemory;
		FileName = fileName;
	}

	/// <summary>
	///		Asigna el valor de un parámetro
	/// </summary>
	protected override void AssignParameter(string key, string value)
	{
		if (IsEqual(key, nameof(ConnectionString)))
			ConnectionString = value;
		else if (IsEqual(key, nameof(FileName)))
			FileName = value;
		else if (IsEqual(key, nameof(InMemory)))
		{
			if (!string.IsNullOrWhiteSpace(value) && (value.Equals("true", StringComparison.CurrentCultureIgnoreCase) || value.Equals("1")))
				InMemory = true;
			else
				InMemory = false;
		}
	}

	/// <summary>
	///		Genera la cadena de conexión
	/// </summary>
	protected override string GenerateConnectionString() 
	{
		if (InMemory || string.IsNullOrWhiteSpace(FileName))
			return "DataSource=:memory:";
		else if (!string.IsNullOrWhiteSpace(FileName))
			return $"Data Source={FileName}";
		else
			throw new ArgumentException("Undefined connection string parameters");
	}

	/// <summary>
	///		Indica si la conexión se crea en memoria 
	/// </summary>
	public bool InMemory { get; private set; }

	/// <summary>
	///		Nombre de archivo
	/// </summary>
	public string? FileName { get; private set; }
}