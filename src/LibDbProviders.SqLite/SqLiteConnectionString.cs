using Bau.Libraries.LibDbProviders.Base;

namespace Bau.Libraries.LibDbProviders.SqLite;

/// <summary>
///		Cadena de conexión de SqLite
/// </summary>
public class SqLiteConnectionString : DbConnectionStringBase
{ 
	// Enumerados públicos
	/// <summary>
	///		Modo de apertura de la base de datos
	/// </summary>
	public enum OpenMode
	{
		/// <summary>Indefinido</summary>
		Unknown,
		/// <summary>Abre la base de datos para lectura y escritura y la crea si no existe</summary>
		ReadWriteCreate,
		/// <summary>Abre la base de datos para lectura / escritura</summary>
		ReadWrite,
		/// <summary>Abre la base de datos en modo de sólo lectura</summary>
		ReadOnly,
		/// <summary>Abre una base de datos en memoria</summary>
		Memory
	}

	public SqLiteConnectionString() : base(string.Empty) { }

	public SqLiteConnectionString(string connectionString) : base(connectionString) {}

	public SqLiteConnectionString(Dictionary<string, string> parameters) : base(parameters) {}

	public SqLiteConnectionString(string fileName, string password, OpenMode mode = OpenMode.ReadWriteCreate) : base(string.Empty)
	{
		FileName = fileName;
		Password = password;
		Mode = mode;
	}

	/// <summary>
	///		Asigna el valor de un parámetro
	/// </summary>
	protected override void AssignParameter(string key, string value)
	{
		if (IsEqual(key, nameof(Mode)))
			Mode = GetEnum(value, OpenMode.ReadWriteCreate);
		else if (IsEqual(key, nameof(FileName)))
			FileName = value;
		else if (IsEqual(key, nameof(Password)))
			Password = value;
	}

	/// <summary>
	///		Genera la cadena de conexión
	/// </summary>
	protected override string GenerateConnectionString()
	{
		string connection = $"Data Source={FileName};";

			// Añade la contraseña
			if (!string.IsNullOrEmpty(Password))
				connection += $"Password={Password};";
			// Añade el modo
			if (Mode != OpenMode.Unknown)
				connection += $"Mode={Mode.ToString()};";
			// Devuelve la cadena de conexión
			return connection;
	}

	/// <summary>
	///		Nombre de archivo
	/// </summary>
	public string? FileName { get; private set; }

	/// <summary>
	///		Contraseña
	/// </summary>
	public string? Password { get; private set; }

	/// <summary>
	///		Modo de conexión
	/// </summary>
	public OpenMode Mode { get; private set; } = OpenMode.ReadWrite;
}