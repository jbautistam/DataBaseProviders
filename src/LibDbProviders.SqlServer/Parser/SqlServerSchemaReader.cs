using System.Data;
using System.Data.Common;

using Bau.Libraries.LibDbProviders.Base.Extensors;
using Bau.Libraries.LibDbProviders.Base.Schema;

namespace Bau.Libraries.LibDbProviders.SqlServer.Parser;

/// <summary>
///		Clase de lectura de los datos de esquema para SqlServer
/// </summary>
internal class SqlServerSchemaReader
{
	/// <summary>
	///		Clase para la carga de un esquema de una base de datos SQL Server
	/// </summary>
	internal async Task<SchemaDbModel> GetSchemaAsync(SqlServerProvider provider, SchemaOptions options, TimeSpan timeout, CancellationToken cancellationToken)
	{
		SchemaDbModel schema = new();

			// Carga los datos del esquema
			using (SqlServerProvider connection = new(provider.ConnectionString))
			{
				// Abre la conexión
				await connection.OpenAsync(cancellationToken);
				// Carga los datos del esquema
				if (options.IncludeTables)
					await LoadTablesAndColumnsAsync(connection, schema, options, timeout, cancellationToken);
				if (options.IncludeTriggers)
					await LoadTriggersAsync(connection, schema, timeout, cancellationToken);
				//if (options.IncludeViews)
				//	await LoadViewsAsync(connection, schema, timeout, cancellationToken);
				if (options.IncludeRoutines)
					await LoadRoutinesAsync(connection, schema, timeout, cancellationToken);
			}
			// Devuelve el esquema
			return schema;
	}

	/// <summary>
	///		Carga las tablas, vistas y columnas de un esquema
	/// </summary>
	private async Task LoadTablesAndColumnsAsync(SqlServerProvider connection, SchemaDbModel schema, SchemaOptions options, TimeSpan timeout, 
												 CancellationToken cancellationToken)
	{
		string sql = """
						SELECT [Tables].Table_Catalog, [Tables].Table_Schema, [Tables].Table_Name, [Tables].Table_Type,
								[Columns].Column_Name, [Columns].Ordinal_Position, [Columns].Column_Default, [Columns].Is_Nullable, [Columns].Data_Type,
								[Columns].Character_Maximum_Length,
								CAST([Columns].Numeric_Precision AS int) AS Numeric_Precision,
								CAST([Columns].Numeric_Precision_Radix AS int) AS Numeric_Precision_Radix,
								CAST([Columns].Numeric_Scale AS int) AS Numeric_Scale,
								CAST([Columns].DateTime_Precision AS int) AS DateTime_Precision,
								[Columns].Character_Set_Name, [Columns].Collation_Catalog, [Columns].Collation_Schema, [Columns].Collation_Name
							FROM [Information_Schema].[Tables] INNER JOIN [Information_Schema].[Columns]
							ON [Tables].Table_Catalog = [Columns].Table_Catalog
								AND [Tables].Table_Schema = [Columns].Table_Schema
								AND [Tables].Table_Name = [Columns].Table_Name
						""";

			// Carga las tablas / vistas
			using (DbDataReader reader = await connection.ExecuteReaderAsync(sql, null, CommandType.Text, timeout, cancellationToken))
			{ 
				// Recorre la colección de registros
				while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
				{
					bool isView = (reader.IisNull<string>("Table_Type") ?? string.Empty).Equals("VIEW", StringComparison.CurrentCultureIgnoreCase);
					BaseTableDbModel? tableView = schema.Add(!isView, reader.IisNull<string>("TABLE_SCHEMA"), reader.IisNull<string>("TABLE_NAME"));

						if (tableView is not null)
						{
							// Asigna los datos del registro a la tabla / vista
							tableView.Catalog = reader.IisNull<string>("TABLE_CATALOG");
							// Añade el campo
							tableView.Fields.Add(new FieldDbModel()
													{
														Catalog = reader.IisNull<string>("Table_Catalog"),
														Name = reader.IisNull<string>("Column_Name"),
														OrdinalPosition = reader.IisNull<int>("Ordinal_Position", 0),
														Default = reader.IisNull<string>("Column_Default"),
														IsRequired = (reader.IisNull<string>("Is_Nullable") ?? "Unknown").Equals("no", StringComparison.CurrentCultureIgnoreCase),
														Type = GetFieldType(reader.IisNull<string>("Data_Type")),
														DbType = reader.IisNull<string>("Data_Type"),
														Length = reader.IisNull<int>("Character_Maximum_Length", 0),
														NumericPrecision = reader.IisNull<int>("Numeric_Precision", 0),
														NumericPrecisionRadix = reader.IisNull<int>("Numeric_Precision_Radix", 0),
														NumericScale = reader.IisNull<int>("Numeric_Scale", 0),
														DateTimePrecision = reader.IisNull<int>("DateTime_Precision", 0),
														CharacterSetName = reader.IisNull<string>("Character_Set_Name"),
														CollationCatalog = reader.IisNull<string>("Collation_Catalog"),
														CollationSchema = reader.IisNull<string>("Collation_Schema"),
														CollationName = reader.IisNull<string>("Collation_Name")
														// Estos campos se tienen que sacar de otras vistas
														//Description = reader.IisNull<string>("Description")
													}
											);
						}
				}
			}
			// Carga los datos adicionales de las vistas
			await LoadViewsDefinitionAsync(connection, schema.Views, timeout, cancellationToken);
			// Carga las restricciones de las tablas
			await LoadConstraintsAsync(connection, schema.Tables, timeout, cancellationToken);
	}

	/// <summary>
	///		Carga los datos adicionales de la definición de vistas
	/// </summary>
	private async Task LoadViewsDefinitionAsync(SqlServerProvider connection, List<ViewDbModel> views, TimeSpan timeout, CancellationToken cancellationToken)
	{
		string sql = """
					SELECT Table_Catalog, Table_Schema, Table_Name, View_Definition, Check_Option, Is_Updatable
						FROM Information_Schema.Views
					""";

			// Carga las vistas
			using (DbDataReader reader = await connection.ExecuteReaderAsync(sql, null, CommandType.Text, timeout, cancellationToken))
			{ 
				while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
				{					
					ViewDbModel? view = Search(views, reader.IisNull<string>("Table_Catalog"), reader.IisNull<string>("Table_Schema"),
											   reader.IisNull<string>("Table_Name"));

						// Asigna los datos adicionales al objeto
						if (view is not null)
						{
							view.Definition = reader.IisNull<string>("View_Definition");
							view.CheckOption = reader.IisNull<string>("Check_Option");
							view.IsUpdatable = !(reader.IisNull<string>("Is_Updatable") ?? "Unknown").Equals("NO", StringComparison.CurrentCultureIgnoreCase);
						}
				}
			}

		// Busca una vista en una colección
		ViewDbModel? Search(List<ViewDbModel> views, string? catalogue, string? schema, string? name)
		{
			return views.FirstOrDefault(item => $"{item.Catalog}##{item.Schema}##{item.Name}".Equals($"{catalogue}##{schema}##{name}", StringComparison.CurrentCultureIgnoreCase));
		}
	}

	/// <summary>
	///		Carga los triggers de un esquema
	/// </summary>
	private async Task LoadTriggersAsync(SqlServerProvider connection, SchemaDbModel schema, TimeSpan timeout, CancellationToken cancellationToken)
	{
		string sql = @"SELECT tmpTables.name AS DS_Table, tmpTrigger.name AS DS_Trigger_Name,
								   USER_NAME(tmpTrigger.uid) AS DS_User_Name, tmpTrigger.category AS NU_Category,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'IsExecuted') = 1) THEN 1 ELSE 0 END)) AS IsExecuted,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsAnsiNullsOn') = 1) THEN 1 ELSE 0 END)) AS ExecIsAnsiNullsOn,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsQuotedIdentOn') = 1) THEN 1 ELSE 0 END)) AS ExecIsQuotedIdentOn,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'IsAnsiNullsOn') = 1) THEN 1 ELSE 0 END)) AS IsAnsiNullsOn,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'IsQuotedIdentOn') = 1) THEN 1 ELSE 0 END)) AS IsQuotedIdentOn,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsAfterTrigger') = 1) THEN 1 ELSE 0 END)) AS ExecIsAfterTrigger,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsDeleteTrigger') = 1) THEN 1 ELSE 0 END)) AS ExecIsDeleteTrigger,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsFirstDeleteTrigger') = 1) THEN 1 ELSE 0 END)) AS ExecIsFirstDeleteTrigger,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsFirstInsertTrigger') = 1) THEN 1 ELSE 0 END)) AS ExecIsFirstInsertTrigger,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsFirstUpdateTrigger') = 1) THEN 1 ELSE 0 END)) AS ExecIsFirstUpdateTrigger,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsInsertTrigger') = 1) THEN 1 ELSE 0 END)) AS ExecIsInsertTrigger,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsInsteadOfTrigger') = 1) THEN 1 ELSE 0 END)) AS ExecIsInsteadOfTrigger,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsLastDeleteTrigger') = 1) THEN 1 ELSE 0 END)) AS ExecIsLastDeleteTrigger,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsLastInsertTrigger') = 1) THEN 1 ELSE 0 END)) AS ExecIsLastInsertTrigger,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsLastUpdateTrigger') = 1) THEN 1 ELSE 0 END)) AS ExecIsLastUpdateTrigger,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsTriggerDisabled') = 1) THEN 1 ELSE 0 END)) AS ExecIsTriggerDisabled,
								   CONVERT(bit, (CASE WHEN (OBJECTPROPERTY(tmpTrigger.id, N'ExecIsUpdateTrigger') = 1) THEN 1 ELSE 0 END)) AS ExecIsUpdateTrigger,
								   tmpTrigger.crdate AS FE_Create, tmpTrigger.refdate AS FE_Reference
							  FROM sys.sysobjects AS tmpTrigger INNER JOIN sys.sysobjects AS tmpTables
								ON tmpTrigger.parent_obj = tmpTables.id
							  WHERE OBJECTPROPERTY(tmpTrigger.id, N'IsTrigger') = 1
								AND OBJECTPROPERTY(tmpTrigger.id, N'IsMSShipped') = 0
							  ORDER BY tmpTables.Name, tmpTrigger.Name";

			// Carga los desencadenadores
			using (DbDataReader reader = await connection.ExecuteReaderAsync(sql, null, CommandType.Text, timeout, cancellationToken))
			{ 
				// Recorre la colección de registros
				while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
				{
					TriggerDbModel trigger = new();

						// Asigna los datos del registro al objeto
						trigger.Catalog = null; // clsBaseDB.iisNull(rdoTables, "TABLE_CATALOG") as string;
						trigger.Schema = null; // clsBaseDB.iisNull(rdoTables, "TABLE_SCHEMA") as string;
						trigger.Table = reader.IisNull<string>("DS_Table");
						trigger.Name = reader.IisNull<string>("DS_Trigger_Name");
						trigger.UserName = reader.IisNull<string>("DS_User_Name");
						trigger.Category = reader.IisNull<int>("NU_Category", 0);
						trigger.IsExecuted = reader.IisNull<bool>("IsExecuted", false);
						trigger.IsExecutionAnsiNullsOn = reader.IisNull<bool>("ExecIsAnsiNullsOn", false);
						trigger.IsExecutionQuotedIdentOn = reader.IisNull<bool>("ExecIsQuotedIdentOn", false);
						trigger.IsAnsiNullsOn = reader.IisNull<bool>("IsAnsiNullsOn", false);
						trigger.IsQuotedIdentOn = reader.IisNull<bool>("IsQuotedIdentOn", false);
						trigger.IsExecutionAfterTrigger = reader.IisNull<bool>("ExecIsAfterTrigger", false);
						trigger.IsExecutionDeleteTrigger = reader.IisNull<bool>("ExecIsDeleteTrigger", false);
						trigger.IsExecutionFirstDeleteTrigger = reader.IisNull<bool>("ExecIsFirstDeleteTrigger", false);
						trigger.IsExecutionFirstInsertTrigger = reader.IisNull<bool>("ExecIsFirstInsertTrigger", false);
						trigger.IsExecutionFirstUpdateTrigger = reader.IisNull<bool>("ExecIsFirstUpdateTrigger", false);
						trigger.IsExecutionInsertTrigger = reader.IisNull<bool>("ExecIsInsertTrigger", false);
						trigger.IsExecutionInsteadOfTrigger = reader.IisNull<bool>("ExecIsInsteadOfTrigger", false);
						trigger.IsExecutionLastDeleteTrigger = reader.IisNull<bool>("ExecIsLastDeleteTrigger", false);
						trigger.IsExecutionLastInsertTrigger = reader.IisNull<bool>("ExecIsLastInsertTrigger", false);
						trigger.IsExecutionLastUpdateTrigger = reader.IisNull<bool>("ExecIsLastUpdateTrigger", false);
						trigger.IsExecutionTriggerDisabled = reader.IisNull<bool>("ExecIsTriggerDisabled", false);
						trigger.IsExecutionUpdateTrigger = reader.IisNull<bool>("ExecIsUpdateTrigger", false);
						trigger.CreatedAt = reader.IisNull<DateTime>("FE_Create", DateTime.UtcNow);
						trigger.DateReference = reader.IisNull<DateTime?>("FE_Reference");
						// Añade el objeto a la colección (si es una tabla)
						schema.Triggers.Add(trigger);
				}
			}
			// Carga el contenido de los triggers
			foreach (TriggerDbModel trigger in schema.Triggers)
				trigger.Content = await LoadHelpTextAsync(connection, trigger.Name, timeout, cancellationToken);
	}

	/// <summary>
	///		Carga el texto de una función, procedimiento, trigger ...
	/// </summary>
	private async Task<string> LoadHelpTextAsync(SqlServerProvider connection, string? name, TimeSpan timeout, CancellationToken cancellationToken)
	{
		string text = string.Empty;

			// Obtiene el texto resultante de llamar a la rutina sp_helptext
			try
			{
				using (DbDataReader reader = await connection.ExecuteReaderAsync($"EXEC sp_helptext '{name}'", null, CommandType.Text, timeout, cancellationToken))
				{ 
					// Obtiene el texto
					while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
						text += reader.IisNull<string>("Text") + Environment.NewLine;
				}
			}
			catch { }
			// Devuelve el texto cargado
			return text;
	}

	/// <summary>
	///		Carga las rutinas de la base de datos
	/// </summary>
	private async Task LoadRoutinesAsync(SqlServerProvider connection, SchemaDbModel schema, TimeSpan timeout, CancellationToken cancellationToken)
	{
		string sql = """
						SELECT Routine_Catalog AS Table_Catalog, Routine_Schema AS Table_Schema,
							   Routine_Name AS Table_Name, Routine_Type, Routine_Definition
						  FROM Information_Schema.Routines
						  ORDER BY Routine_Name
					""";

			// Carga los datos
			using (DbDataReader reader = await connection.ExecuteReaderAsync(sql, null, CommandType.Text, timeout, cancellationToken))
			{ 
				// Lee los registros
				while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
				{
					RoutineDbModel routine = new();

						// Asigna los datos del recordset al objeto
						routine.Catalog = reader.IisNull<string>("Table_Catalog");
						routine.Schema = reader.IisNull<string>("Table_Schema");
						routine.Name = reader.IisNull<string>("Table_Name");
						routine.Type = GetRoutineType(reader.IisNull<string>("Routine_Type") ?? "Unknown");
						routine.Content = reader.IisNull<string>("Routine_Definition");
						// Añade el objeto a la colección
						schema.Routines.Add(routine);
				}
			}

		// Obtiene el tipo de rutina
		RoutineDbModel.RoutineType GetRoutineType(string type)
		{
			if (type.Equals("PROCEDURE", StringComparison.CurrentCultureIgnoreCase))
				return RoutineDbModel.RoutineType.Procedure;
			else if (type.Equals("FUNCTION", StringComparison.CurrentCultureIgnoreCase))
				return RoutineDbModel.RoutineType.Function;
			else
				return RoutineDbModel.RoutineType.Unknown;
		}
	}

	/// <summary>
	///		Carga las restricciones de una tabla
	/// </summary>
	private async Task LoadConstraintsAsync(SqlServerProvider connection, List<TableDbModel> tables, TimeSpan timeout, CancellationToken cancellationToken)
	{
		string sql = @"SELECT TableConstraints.Table_Catalog, TableConstraints.Table_Schema, TableConstraints.Table_Name,
							   ColumnConstraint.Column_Name, ColumnConstraint.Constraint_Name,
							   TableConstraints.Constraint_Type, Key_Column.Ordinal_Position
						  FROM Information_Schema.Table_Constraints AS TableConstraints
								INNER JOIN Information_Schema.Constraint_Column_Usage AS ColumnConstraint
									ON TableConstraints.Constraint_Catalog = ColumnConstraint.Constraint_Catalog
										AND TableConstraints.Constraint_Schema = ColumnConstraint.Constraint_Schema
										AND TableConstraints.Constraint_Name = ColumnConstraint.Constraint_Name
								INNER JOIN Information_Schema.Key_Column_Usage AS Key_Column
									ON ColumnConstraint.Constraint_Catalog = Key_Column.Constraint_Catalog
										AND ColumnConstraint.Constraint_Schema = Key_Column.Constraint_Schema
										AND ColumnConstraint.Constraint_Name = Key_Column.Constraint_Name
										AND ColumnConstraint.Column_Name = Key_Column.Column_Name
						  ORDER BY TableConstraints.Table_Name, TableConstraints.Constraint_Type, Key_Column.Ordinal_Position";

			// Carga los datos
			using (DbDataReader reader = await connection.ExecuteReaderAsync(sql, null, CommandType.Text, timeout, cancellationToken))
			{ 
				while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
				{
					TableDbModel? table = Search(tables, reader.IisNull<string>("Table_Catalog"), reader.IisNull<string>("Table_Schema"), 
												 reader.IisNull<string>("Table_Name"));
													
						if (table is not null)
						{
							ConstraintDbModel constraint = new();

								// Asigna los datos del registro
								constraint.Catalog = reader.IisNull<string>("Table_Catalog");
								constraint.Schema = reader.IisNull<string>("Table_Schema");
								constraint.Table = reader.IisNull<string>("Table_Name");
								constraint.Column = reader.IisNull<string>("Column_Name");
								constraint.Name = reader.IisNull<string>("Constraint_Name");
								constraint.Type = GetConstraintType(reader.IisNull<string>("Constraint_Type") ?? "Unknown");
								constraint.Position = reader.IisNull<int>("Ordinal_Position", 0);
								// Añade la restricción a la colección
								table.Constraints.Add(constraint);
						}
				}
			}
			// Ajusta las restricciones de clave primaria
			foreach (TableDbModel table in tables)
				foreach (ConstraintDbModel constraint in table.Constraints)
					if (constraint.Type == ConstraintDbModel.ConstraintType.PrimaryKey || constraint.Type == ConstraintDbModel.ConstraintType.ForeignKey)
					{
						FieldDbModel? field = table.Fields.FirstOrDefault(item => (item.Name?? string.Empty).Equals(constraint.Column, StringComparison.CurrentCultureIgnoreCase));

							if (field is not null)
							{
								if (constraint.Type == ConstraintDbModel.ConstraintType.PrimaryKey)
									field.IsKey = true;
								else
									field.IsForeignKey = true;
							}
					}

		// Busca una vista en una colección
		TableDbModel? Search(List<TableDbModel> tables, string? catalogue, string? schema, string? name)
		{
			return tables.FirstOrDefault(item => $"{item.Catalog}##{item.Schema}##{item.Name}".Equals($"{catalogue}##{schema}##{name}", StringComparison.CurrentCultureIgnoreCase));
		}

		// Obtiene el tipo de una restricción a partir de su nombre
		ConstraintDbModel.ConstraintType GetConstraintType(string type)
		{
			if (type.Equals("UNIQUE", StringComparison.CurrentCultureIgnoreCase))
				return ConstraintDbModel.ConstraintType.Unique;
			else if (type.Equals("PRIMARY KEY", StringComparison.CurrentCultureIgnoreCase))
				return ConstraintDbModel.ConstraintType.PrimaryKey;
			else if (type.Equals("FOREIGN KEY", StringComparison.CurrentCultureIgnoreCase))
				return ConstraintDbModel.ConstraintType.ForeignKey;
			else
				return ConstraintDbModel.ConstraintType.Unknown;
		}
	}

	/// <summary>
	///		Obtiene el tipo de campo
	/// </summary>
	private FieldDbModel.Fieldtype GetFieldType(string? columnType)
	{
		return (columnType ?? string.Empty).ToUpper() switch
					{
						null => FieldDbModel.Fieldtype.Unknown,
						"INTEGER" or "INT" or "SMALLINT" or "BIGINT" or "TINYINT" => FieldDbModel.Fieldtype.Integer,
						"REAL" or "FLOAT" or "MONEY" or "DECIMAL" => FieldDbModel.Fieldtype.Decimal,
						"DATETIME" or "DATE" or "DATETIME2" or "TIME" => FieldDbModel.Fieldtype.Date,
						"BINARY" or "VARBINARY" or "IMAGE" => FieldDbModel.Fieldtype.Binary,
						"BIT" => FieldDbModel.Fieldtype.Boolean,
						"CHAR" or "NCHAR" or "VARCHAR" or "NVARCHAR" or "TEXT" or "NTEXT" or "UNIQUEIDENTIFIER" => FieldDbModel.Fieldtype.String,
						_ => FieldDbModel.Fieldtype.Unknown,
					};
	}
}