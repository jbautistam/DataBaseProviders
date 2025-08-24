namespace Bau.Libraries.LibDbProviders.SqLite.Builders;

/// <summary>
///		Generador de base de datos
/// </summary>
public class SqLiteBuilder(string fileName)
{
	/// <summary>
	///		Crea un archivo de base de datos con las tablas especificadas
	/// </summary>
	public void Create()
	{
		if (TableBuilders.Count > 0)
			using (SqLiteProvider dbProvider = new(new SqLiteConnectionString(FileName, string.Empty, SqLiteConnectionString.OpenMode.ReadWriteCreate)))
			{
				// Abre la conexión
				dbProvider.Open();
				// Crea las tablas
				foreach (SqLiteTableBuilder builder in TableBuilders)
				{
					string sql = builder.GetSqlCreate();

						if (!string.IsNullOrEmpty(sql))
							dbProvider.Execute(sql, null, System.Data.CommandType.Text);
				}
			}
	}

	/// <summary>
	///		Genera una tabla
	/// </summary>
	public SqLiteTableBuilder WithTable(string table)
	{
		// Añade el generador
		TableBuilders.Add(new SqLiteTableBuilder(this, table));
		// Devuelve el último generador
		return TableBuilders[TableBuilders.Count - 1];
	}

	/// <summary>
	///		Nombre de archivo
	/// </summary>
	public string FileName { get; } = fileName;

	/// <summary>
	///		Generadores de tablas
	/// </summary>
	private List<SqLiteTableBuilder> TableBuilders { get; } = [];
}
