namespace Bau.Libraries.LibDbProviders.Base.Schema;

/// <summary>
///		Opciones de lectura del esquema
/// </summary>
public class SchemaOptions
{
	/// <summary>
	///		Indica si se deben incluir las tablas del esquema
	/// </summary>
	public bool IncludeTables { get; set; } = true;

	/// <summary>
	///		Indica si se deben incluir las vistas del esquema
	/// </summary>
	public bool IncludeViews { get; set; } = true;

	/// <summary>
	///		Indica si se deben incluir las rutinas del esquema
	/// </summary>
	public bool IncludeRoutines { get; set; } = true;

	/// <summary>
	///		Indica si se deben incluir los desencadenadores en el esquema
	/// </summary>
	public bool IncludeTriggers { get; set; } = true;

	/// <summary>
	///		Indica si se deben incluir las descripciones del esquema
	/// </summary>
	public bool IncludeDescriptions { get; set; } = true;

	/// <summary>
	///		Indica si se deben incluir datos de sistema
	/// </summary>
	public bool IncludeSystemData { get; set; } 
}
