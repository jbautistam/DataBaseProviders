namespace Bau.Libraries.LibDbProviders.Base.Schema;

/// <summary>
///		Clase con los datos de un esquema de base de datos
/// </summary>
public class SchemaDbModel
{
	/// <summary>
	///		Añade una tabla / vista a la colección del esquema
	/// </summary>
	public BaseTableDbModel? Add(bool isTable, string? schema, string? tableName)
	{
		BaseTableDbModel? table = null;

			// Normaliza los datos
			schema = schema ?? string.Empty;
			// Añade el elemento
			if (!string.IsNullOrWhiteSpace(tableName))
				if (isTable)
					table = Add(Tables, schema, tableName);
				else
					table = Add(Views, schema, tableName);
			// Devuelve la tabla / vista
			return table;
	}

	/// <summary>
	///		Añade una tabla / vista a la colección del esquema
	/// </summary>
	public BaseTableDbModel? Add(bool isTable, string? schema, string? tableName, string? fieldName, 
								FieldDbModel.Fieldtype fieldType, string? fieldDbType, int fieldLength, bool isPrimaryKey, bool isRequired)
	{
		BaseTableDbModel? table = null;

			// Normaliza los datos
			schema = schema ?? string.Empty;
			// Añade el elemento
			if (!string.IsNullOrWhiteSpace(tableName) && !string.IsNullOrWhiteSpace(fieldName))
				if (isTable)
					table = Add(Tables, schema, tableName, fieldName, fieldType, fieldDbType, fieldLength, isPrimaryKey, isRequired);
				else
					table = Add(Views, schema, tableName, fieldName, fieldType, fieldDbType, fieldLength, isPrimaryKey, isRequired);
			// Devuelve la tabla / vista
			return table;
	}

	/// <summary>
	///		Añade la tabla a la colección
	/// </summary>
	private TableDbModel Add(List<TableDbModel> tables, string? schema, string? tableName) => Search(tables, schema, tableName);

	/// <summary>
	///		Añade la tabla a la colección y un campo a la tabla
	/// </summary>
	private TableDbModel Add(List<TableDbModel> tables, string? schema, string? tableName, string? fieldName, FieldDbModel.Fieldtype fieldType, 
							 string? fieldDbType, int fieldLength, bool isPrimaryKey, bool isRequired)
	{
		TableDbModel table = Add(tables, schema, tableName);

			// Añade un campo a la tabla
			table.AddField(fieldName, fieldType, fieldDbType, fieldLength, isPrimaryKey, isRequired);
			// Devuelve la tabla
			return table;
	}

	/// <summary>
	///		Añade una vista a la colección
	/// </summary>
	private ViewDbModel Add(List<ViewDbModel> views, string? schema, string? name) => Search(views, schema, name);

	/// <summary>
	///		Añade la vista a la colección y un campo a la vista
	/// </summary>
	private ViewDbModel Add(List<ViewDbModel> views, string? schema, string? tableName, string? fieldName, FieldDbModel.Fieldtype fieldType, 
							string? fieldDbType, int fieldLength, bool isPrimaryKey, bool isRequired)
	{
		ViewDbModel view = Search(views, schema, tableName);

			// Añade un campo a la tabla
			view.AddField(fieldName, fieldType, fieldDbType, fieldLength, isPrimaryKey, isRequired);
			// Devuelve la lista
			return view;
	}

	/// <summary>
	///		Busca una tabla, si no existía, la añade
	/// </summary>
	private TableDbModel Search(List<TableDbModel> tables, string? schema, string? name)
	{
		TableDbModel? table = tables.FirstOrDefault(item => (item.Schema ?? "Unknown").Equals(schema, StringComparison.CurrentCultureIgnoreCase) &&
														    (item.Name ?? "Unknown").Equals(name, StringComparison.CurrentCultureIgnoreCase));

			// Crea la tabla si no existía
			if (table is null)
			{
				// Crea la tabla
				table = new TableDbModel
								{
									Schema = schema,
									Name = name
								};
				// La añade a la colección
				tables.Add(table);
			}
			// Devuelve la tabla
			return table;
	}

	/// <summary>
	///		Busca una vista, si no existía, la añade
	/// </summary>
	private ViewDbModel Search(List<ViewDbModel> views, string? schema, string? name)
	{
		ViewDbModel? view = views.FirstOrDefault(item => (item.Schema ?? "Unknown").Equals(schema, StringComparison.CurrentCultureIgnoreCase) &&
														 (item.Name ?? "Unknown").Equals(name, StringComparison.CurrentCultureIgnoreCase));

			// Crea la tabla si no existía
			if (view is null)
			{
				// Crea la vista
				view = new ViewDbModel
								{
									Schema = schema,
									Name = name
								};
				// La añade a la colección
				views.Add(view);
			}
			// Devuelve la vista
			return view;
	}

	/// <summary>
	///		Tablas de la base de datos
	/// </summary>
	public List<TableDbModel> Tables { get; } = new();

	/// <summary>
	///		Vistas de la base de datos
	/// </summary>
	public List<ViewDbModel> Views { get; } = new();

	/// <summary>
	///		Rutinas de la base de datos
	/// </summary>
	public List<RoutineDbModel> Routines { get; } = new();

	/// <summary>
	///		Desencadenadores de la base de datos
	/// </summary>
	public List<TriggerDbModel> Triggers { get; } = new();
}
