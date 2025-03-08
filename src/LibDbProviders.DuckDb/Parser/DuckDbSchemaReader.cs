using System.Data;
using System.Data.Common;

using Bau.Libraries.LibDbProviders.Base.Extensors;
using Bau.Libraries.LibDbProviders.Base.Schema;

namespace Bau.Libraries.LibDbProviders.DuckDb.Parser;

/// <summary>
///		Clase para lectura del esquema en una conexión DuckDb
/// </summary>
internal class DuckDbSchemaReader
{
	// Esquemas de sistema
	private List<string> SystemSchemas = new() { "information_schema" };

	/// <summary>
	///		Obtiene el esquema
	/// </summary>
	internal async Task<SchemaDbModel> GetSchemaAsync(DuckDbProvider provider, bool includeSystemTables, TimeSpan timeout, CancellationToken cancellationToken)
	{
		SchemaDbModel schema = new();

			// Carga el esquema
			using (DuckDbProvider connection = new(provider.ConnectionString))
			{
				// Abre la conexión
				await connection.OpenAsync(cancellationToken);
				// Carga las tablas y vistas
				await ReadTablesAsync(connection, schema, timeout, includeSystemTables, cancellationToken);
				// Carga las columnas
				await ReadFieldsAsync(connection, schema, timeout, cancellationToken);
			}
			// Devuelve el esquema
			return schema;
	}

	/// <summary>
	///		Carga las tablas y vistas en el esquema
	/// </summary>
	private async Task ReadTablesAsync(DuckDbProvider connection, SchemaDbModel schema, TimeSpan timeout, bool includeSystemTables, CancellationToken cancellationToken)
	{
		string sql = """
						SELECT table_schema, table_name, table_type
							FROM information_schema.tables
					""";

			// Lee las tablas / vistas
			using (DbDataReader rdoData = await connection.ExecuteReaderAsync(sql, null, CommandType.Text, timeout, cancellationToken))
			{
				while (!cancellationToken.IsCancellationRequested && await rdoData.ReadAsync(cancellationToken))
				{
					string? schemaName = rdoData.IisNull<string>("table_schema");
					bool isSystemTable = IsSystemSchema(schemaName);

						if (includeSystemTables || !isSystemTable)
						{
							BaseTableDbModel? table = schema.Add(!(rdoData.IisNull<string>("table_type") ?? "Unknown").Equals("VIEW", StringComparison.CurrentCultureIgnoreCase),
																 rdoData.IisNull<string>("table_schema"),
																 rdoData.IisNull<string>("table_name"));

								// Indica si es una tabla de sistema
								if (table is not null)
									table.IsSystem = isSystemTable;
						}
				}
			}
	}

	/// <summary>
	///		Comprueba si un nombre de esquema pertenece a los esquemas de sistema
	/// </summary>
	private bool IsSystemSchema(string? schemaName)
	{
		return !string.IsNullOrWhiteSpace(schemaName) && 
			   SystemSchemas.Any(schema => schema.Equals(schemaName, StringComparison.CurrentCultureIgnoreCase));
	}

	/// <summary>
	///		Carga los campos de las tablas / vistas del esquema
	/// </summary>
	private async Task ReadFieldsAsync(DuckDbProvider connection, SchemaDbModel schema, TimeSpan timeout, CancellationToken cancellationToken)
	{
		// Carga las columnas de las tablas
		foreach (TableDbModel table in schema.Tables)
			if (!cancellationToken.IsCancellationRequested)
				await ReadFieldsAsync(connection, table, timeout, cancellationToken);
		// Carga las columnas de las vistas
		foreach (ViewDbModel view in schema.Views)
			if (!cancellationToken.IsCancellationRequested)
				await ReadFieldsAsync(connection, view, timeout, cancellationToken);
	}

	/// <summary>
	///		Carga los campos de una tabla / vista
	/// </summary>
	private async Task ReadFieldsAsync(DuckDbProvider connection, BaseTableDbModel table, TimeSpan timeout, CancellationToken cancellationToken)
	{
		string sql = $"""
						SELECT column_name, ordinal_position, column_default, is_nullable, data_type,
							   character_maximum_length, character_octet_length, numeric_precision, numeric_scale,
							   datetime_precision
							FROM information_schema.columns
							WHERE table_schema = '{table.Schema}'
								AND table_name = '{table.Name}'
					""";

			// Lee las columnas
			using (DbDataReader rdoData = await connection.ExecuteReaderAsync(sql, null, CommandType.Text, timeout, cancellationToken))
			{
				while (!cancellationToken.IsCancellationRequested && await rdoData.ReadAsync(cancellationToken))
					table.AddField(rdoData.IisNull<string>("column_name"), GetFieldType(rdoData.IisNull<string>("data_type")),
								   rdoData.IisNull<string>("data_type"), rdoData.IisNull<int>("character_maximum_length", 0),
								   false, (rdoData.IisNull<string>("is_nullable") ?? string.Empty).Equals("YES", StringComparison.CurrentCultureIgnoreCase));
			}
	}

	/// <summary>
	///		Obtiene el tipo de campo
	/// </summary>
	private FieldDbModel.Fieldtype GetFieldType(string? columnType)
	{
		return Normalize(columnType ?? string.Empty) switch
					{
						null => FieldDbModel.Fieldtype.Unknown,
						"INT2" or "_INT2" or "INT4" or "_INT4" or "INT8" or "_INT8" or "INTEGER" or "BIGINT" => FieldDbModel.Fieldtype.Integer,
						"FLOAT4" or "_FLOAT4" or "FLOAT8" or "_FLOAT8" or "DECIMAL" => FieldDbModel.Fieldtype.Decimal,
						"CHAR" or "_CHAR" or "TEXT" or "_TEXT" or "VARCHAR" or "UUID" => FieldDbModel.Fieldtype.String,
						"DATE" or "TIMESTAMP" => FieldDbModel.Fieldtype.Date,
						"BOOL" or "BOOLEAN" => FieldDbModel.Fieldtype.Boolean,
						_ => FieldDbModel.Fieldtype.Unknown
					};

		// Normaliza la cadena
		string Normalize(string type)
		{
			int index;

				// Pasa el tipo a mayúsculas
				type = type.ToUpper();
				// Quita los paréntesis
				index = type.IndexOf('(');
				if (index > 0)
					type = type.Substring(0, index);
				// Devuelve el tipo normalizado
				return type.Trim();
		}
	}
}
