namespace Bau.Libraries.LibDbProviders.SqLite.Builders;

/// <summary>
///		Generador de un campo 
/// </summary>
public class SqLiteFieldBuilder
{
	/// <summary>
	///		Tipo de campo
	/// </summary>
	public enum FieldType
	{
		/// <summary>Entero</summary>
		Integer,
		/// <summary>Texto</summary>
		Text,
		/// <summary>Valor real</summary>
		Real,
		/// <summary>Fecha / hora</summary>
		DateTime
	}

	public SqLiteFieldBuilder(SqLiteTableBuilder parent, string name, FieldType type, int size = 0, bool allowNulls = true)
	{
		Parent = parent;
		Name = name;
		Type = type;
		Size = size;
		AllowNulls = allowNulls;
	}

	/// <summary>
	///		Crea un campo
	/// </summary>
	public SqLiteFieldBuilder WithField(string name, FieldType type, int size = 0, bool allowNulls = true)
	{
		return Parent.WithField(name, type, size, allowNulls);
	}

	/// <summary>
	///		Indica si el campo admite nulos
	/// </summary>
	public SqLiteFieldBuilder WithNulls(bool allowNulls = true)
	{
		// Indica si admite nulos
		AllowNulls = allowNulls;
		// Devuelve el generador
		return this;
	}

	/// <summary>
	///		Indica si el campo es una clave principal
	/// </summary>
	public SqLiteFieldBuilder WithPrimaryKey(bool primaryKey = true)
	{
		// Indica si el campo es clave primaria
		PrimaryKey = primaryKey;
		// Devuelve el generador
		return this;
	}

	/// <summary>
	///		Indica si el campo es autoincrementado
	/// </summary>
	public SqLiteFieldBuilder WithAutoIncrement(bool autoIncrement = true)
	{
		// Indica si el campo es autoincrementado
		AutoIncrement = autoIncrement;
		// Devuelve el generador
		return this;
	}

	/// <summary>
	///		Obtiene la SQL de creación de un campo
	/// </summary>
	internal string GetSqlCreate()
	{
		string field = $"[{Name}] {GetSqlFieldtype(Size)}";

			// Añade los parámetros de clave y autoincremento
			if (PrimaryKey)
			{
				field += " PRIMARY KEY";
				if (AutoIncrement)
					field += " AUTOINCREMENT";
			}
			else // ... sólo puede tener nulos si no es una clave primaria
			{
				if (!AllowNulls)
					field += " NOT NULL";
				else
					field += " NULL";
			}
			// Devuelve la cadena de definición del campo
			return field;
	}

	/// <summary>
	///		Obtiene el tipo de campo
	/// </summary>
	private string GetSqlFieldtype(int size)
	{
		switch (Type)
		{
			case FieldType.Integer:
				return "INTEGER";
			case FieldType.Real:
				return "FLOAT";
			case FieldType.DateTime:
				return "DATETIME";
			default:
				if (size != 0)
					return $"TEXT({size})";
				else
					return "TEXT";
		}
	}

	/// <summary>
	///		Vuelve al generador anterior
	/// </summary>
	public SqLiteTableBuilder Back()
	{
		return Parent;
	}

	/// <summary>
	///		Nombre del campo
	/// </summary>
	public string Name { get; }

	/// <summary>
	///		Tipo del campo
	/// </summary>
	public FieldType Type { get; }

	/// <summary>
	///		Tamaño del campo
	/// </summary>
	public int Size { get; }

	/// <summary>
	///		Indica si permite nulos
	/// </summary>
	public bool AllowNulls { get; private set; } = true;

	/// <summary>
	///		Indica si es clave principal
	/// </summary>
	public bool PrimaryKey { get; private set; }

	/// <summary>
	///		Indica si se autoincrementa el valor
	/// </summary>
	public bool AutoIncrement { get; private set; }

	/// <summary>
	///		Generador padre
	/// </summary>
	private SqLiteTableBuilder Parent { get; }
}
