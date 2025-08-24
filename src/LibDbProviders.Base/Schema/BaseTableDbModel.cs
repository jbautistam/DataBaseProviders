namespace Bau.Libraries.LibDbProviders.Base.Schema;

/// <summary>
///		Clase con los datos de una tabla de base de datos
/// </summary>
public abstract class BaseTableDbModel : BaseSchemaDbModel
{
	/// <summary>
	///		Añade un campo a la tabla
	/// </summary>
	public void AddField(string? name, FieldDbModel.Fieldtype type, string? dbType, int length, bool isPrimaryKey, bool isRequired)
	{
		FieldDbModel? field = Fields.FirstOrDefault(item => (item.Name ?? "Unknown").Equals(name, StringComparison.CurrentCultureIgnoreCase));

			// Si no existía el campo, se crea
			if (field is null)
			{
				field = new FieldDbModel 
								{ 
									Name = name 
								};
				Fields.Add(field);
			}
			// Asigna las propiedades
			field.Type = type;
			field.DbType = dbType;
			field.Length = length;
			field.IsKey = isPrimaryKey;
			field.IsRequired = isRequired;
	}

	/// <summary>
	///		Indica si es una tabla de sistema
	/// </summary>
	public bool IsSystem { get; set; }

	/// <summary>
	///		Campos de la tabla
	/// </summary>
	public List<FieldDbModel> Fields { get; } = [];
}
