// Variables privadas
using Bau.Libraries.LibDbProviders.Base.Schema;
using Bau.Libraries.LibDbProviders.SqlServer;

string connectionString = "Data Source=.;Initial Catalog=MetadataTest;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True";

// Prueba de tiempo de carga del esquema
System.Diagnostics.Stopwatch stopwatch = new();
SchemaDbModel? schema = null;

	// Arranca el temporizador
	stopwatch.Start();
	// Carga el esquema
	using (SqlServerProvider connection = new(new SqlServerConnectionString(connectionString)))
	{
		// Abre la conexión
		connection.Open();
		// Carga el esquema
		schema = await connection.GetSchemaAsync(new SchemaOptions
														{
															IncludeTables = true,
															IncludeViews = true,
															IncludeRoutines = false,
															IncludeTriggers = false,
															IncludeDescriptions = false,
															IncludeSystemData = true
														}, 
													TimeSpan.FromMinutes(2), CancellationToken.None);
	}
	// Detiene el temporizador
	stopwatch.Stop();
	// Muestra el tiempo pasado
	Console.WriteLine($"Elapsed: {stopwatch.Elapsed.ToString()}");
	// Muestra las tablas
	if (schema is not null)
	{
		Console.WriteLine("--- Tables ---");
		foreach (TableDbModel table in schema.Tables)
		{
			Console.WriteLine(table.FullName);
			Print(table.Fields);
		}
		Console.WriteLine("--- Views ---");
		foreach (ViewDbModel view in schema.Views)
		{
			Console.WriteLine(view.FullName);
			Print(view.Fields);
		}
	}

	// Imprime una lista de campos
	void Print(List<FieldDbModel> fields)
	{
		foreach (FieldDbModel field in fields)
		{
			Console.WriteLine($"""
									{field.FullName} {field.DbType}({field.Length}) {field.Type.ToString()} {(field.IsRequired ? "NOT NULL" : "NULL")} 
										{(field.IsKey ? "PK" : "")} {(field.IsForeignKey ? "FK" : "")}
								""".Trim()
								);
			
		}
	}