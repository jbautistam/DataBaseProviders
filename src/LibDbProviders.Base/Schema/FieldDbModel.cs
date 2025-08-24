namespace Bau.Libraries.LibDbProviders.Base.Schema;

/// <summary>
///		Datos de un campo de base de datos
/// </summary>
public class FieldDbModel : BaseSchemaDbModel
{
	// Enumerados públicos
	/// <summary>
	///		Tipo de campo
	/// </summary>
	public enum Fieldtype
	{
		/// <summary>Desconocido. No se debería utilizar</summary>
		Unknown,
		/// <summary>Cadena</summary>
		String,
		/// <summary>Fecha</summary>
		Date,
		/// <summary>Número entero</summary>
		Integer,
		/// <summary>Número decimal</summary>
		Decimal,
		/// <summary>Valor lógico</summary>
		Boolean,
		/// <summary>Datos binarios</summary>
		Binary
	}

	/// <summary>
	///		Nombre de la tabla a la que se asocia el campo (interesante sobre todo cuando se trata de una vista)
	/// </summary>
	public string? Table { get; set; }

	/// <summary>
	///		Tipo del campo
	/// </summary>
	public Fieldtype Type { get; set; }

	/// <summary>
	///		Tipo original del campo en la base de datos
	/// </summary>
	public string? DbType { get; set; }

	/// <summary>
	///		Indica si el campo es clave
	/// </summary>
	public bool IsKey { get; set; }

	/// <summary>
	///		Indica si el campo es clave foránea
	/// </summary>
	public bool IsForeignKey { get; set; }

	/// <summary>
	///		Longitud del campo
	/// </summary>
	public int Length { get; set; }

	/// <summary>
	///		Indica si es un campo obligatorio
	/// </summary>
	public bool IsRequired { get; set; }

	/// <summary>
	///		Formato del campo
	/// </summary>
	public string? Format { get; set; }

	/// <summary>
	///		Posición del campo
	/// </summary>
	public int OrdinalPosition { get; set; }

	/// <summary>
	///		Valor predeterminado
	/// </summary>
	public string? Default { get; set; }

	public int NumericPrecision { get; set; }

	public int NumericPrecisionRadix { get; set; }

	public int NumericScale { get; set; }

	public int DateTimePrecision { get; set; }

	public string? CharacterSetName { get; set; }

	public string? CollationCatalog { get; set; }

	public string? CollationSchema { get; set; }

	public string? CollationName { get; set; }

	/// <summary>
	///		Indica si es un campo de identidad
	/// </summary>
	public bool IsIdentity { get; set; }
}
