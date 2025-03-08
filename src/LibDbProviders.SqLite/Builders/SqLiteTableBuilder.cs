namespace Bau.Libraries.LibDbProviders.SqLite.Builders;

/// <summary>
///		Generador de una tabla para SqLite
/// </summary>
public class SqLiteTableBuilder
{
	public SqLiteTableBuilder(SqLiteBuilder parent, string name)
	{
		Parent = parent;
		Name = name;
	}

	/// <summary>
	///		Crea una tabla
	/// </summary>
	public SqLiteTableBuilder WithTable(string name)
	{
		return Parent.WithTable(name);
	}

	/// <summary>
	///		Genera un campo
	/// </summary>
	public SqLiteFieldBuilder WithField(string name, SqLiteFieldBuilder.FieldType type, int size = 0, bool allowNulls = true)
	{
		// Crea un nuevo generador
		FieldBuilders.Add(new SqLiteFieldBuilder(this, name, type, size, allowNulls));
		// Devuelve el generador creado
		return FieldBuilders[FieldBuilders.Count - 1];
	}

	/// <summary>
	///		Obtiene la sql de generación de la tabla
	/// </summary>
	internal string GetSqlCreate()
	{
		string sql = "";

			// Obtiene la SQL de la tabla
			if (FieldBuilders.Count > 0)
			{
				// Añade la definición de campos
				foreach (SqLiteFieldBuilder builder in FieldBuilders)
				{
					// Añade la coma si es necesario
					if (!string.IsNullOrEmpty(sql))
						sql += ", ";
					// Añade el campo
					sql += builder.GetSqlCreate();
				}
				// Añade la cadena de creación de tablas
				if (!string.IsNullOrEmpty(sql))
				{
					// Asigna la cadena de generación
					sql = $"CREATE TABLE IF NOT Exists [{Name}] ({sql})";
					if (!UseRowId)
						sql += " WITHOUT ROWID"; 
				}
			}
			// Devuelve la cadena de generación
			return sql;
	}

	/// <summary>
	///		Indica si se debe utilizar RowId en la tabla
	/// </summary>
	public SqLiteTableBuilder WithRowId(bool withRowId)
	{
		// Asigna las propiedades
		UseRowId = withRowId;
		// Devuelve el generador
		return this;
	}
	
	/// <summary>
	///		Vuelve al generador anterior
	/// </summary>
	public SqLiteBuilder Back()
	{
		return Parent;
	}

	/// <summary>
	///		Nombre de la tabla
	/// </summary>
	public string Name { get; }

	/// <summary>
	///		Indica si se debe utilizar un campo RowId en la tabla
	/// </summary>
	public bool UseRowId { get; private set; } = true;

	/// <summary>
	///		Generador padre
	/// </summary>
	private SqLiteBuilder Parent { get; }

	/// <summary>
	///		Generadores de campos
	/// </summary>
	private List<SqLiteFieldBuilder> FieldBuilders { get; } = [];
}
