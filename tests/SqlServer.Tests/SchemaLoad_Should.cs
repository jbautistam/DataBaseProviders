using Bau.Libraries.LibDbProviders.Base.Schema;
using Bau.Libraries.LibDbProviders.SqlServer;

namespace SqlServer.Tests;

/// <summary>
///		Pruebas de esquema
/// </summary>
public class SchemaLoad_Should
{
	// Variables privadas
	private string ConnectionString = "Data Source=.;Initial Catalog=Roivolution_NunezDeArenas_Reporting;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True";

	/// <summary>
	///		Prueba de tiempo de carga del esquema
	/// </summary>
	[Fact]
	public async Task load_schema_async()
	{
		System.Diagnostics.Stopwatch stopwatch = new();
		SchemaDbModel? schema = null;

			// Arranca el temporizador
			stopwatch.Start();
			// Carga el esquema
			using (SqlServerProvider connection = new(new SqlServerConnectionString(ConnectionString)))
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
			System.Diagnostics.Debug.WriteLine($"Elapsed: {stopwatch.Elapsed.ToString()}");
			// Muestra las tablas
			if (schema is not null)
			{
				foreach (TableDbModel table in schema.Tables)
					System.Diagnostics.Debug.WriteLine(table.FullName);
				foreach (ViewDbModel view in schema.Views)
					System.Diagnostics.Debug.WriteLine(view.FullName);
			}
	}
}