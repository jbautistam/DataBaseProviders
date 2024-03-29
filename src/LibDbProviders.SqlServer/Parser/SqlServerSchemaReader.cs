﻿using System.Data;
using System.Data.Common;

using Bau.Libraries.LibDbProviders.Base.Extensors;
using Bau.Libraries.LibDbProviders.Base.Models;
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
	internal async Task<SchemaDbModel> GetSchemaAsync(SqlServerProvider provider, bool includeSystemTables, TimeSpan timeout, CancellationToken cancellationToken)
	{
		SchemaDbModel schema = new();

			// Carga los datos del esquema
			using (SqlServerProvider connection = new SqlServerProvider(provider.ConnectionString))
			{
				// Abre la conexión
				await connection.OpenAsync(cancellationToken);
				// Carga los datos del esquema
				await LoadTablesAsync(connection, schema, timeout, cancellationToken);
				await LoadTriggersAsync(connection, schema, timeout, cancellationToken);
				await LoadViewsAsync(connection, schema, timeout, cancellationToken);
				await LoadRoutinesAsync(connection, schema, timeout, cancellationToken);
			}
			// Devuelve el esquema
			return schema;
	}

	/// <summary>
	///		Carga las tablas de un esquema
	/// </summary>
	private async Task LoadTablesAsync(SqlServerProvider connection, SchemaDbModel schema, TimeSpan timeout, CancellationToken cancellationToken)
	{
		//? Esta consulta duplica las tablas porque all_objects no distingue por esquema
		//string sql = @"SELECT Tables.TABLE_CATALOG, Tables.TABLE_SCHEMA, Tables.TABLE_NAME,
		//					   Tables.TABLE_TYPE, Objects.Create_Date, Objects.Modify_Date, Properties.Value AS Description
		//				  FROM INFORMATION_SCHEMA.TABLES AS Tables INNER JOIN sys.all_objects AS Objects
		//						ON Tables.Table_Name = Objects.name
		//					LEFT JOIN sys.extended_properties AS Properties
		//						ON Objects.object_id = Properties.major_id
		//							AND Properties.minor_id = 0
		//							AND Properties.name = 'MS_Description'
		//				 ORDER BY Tables.TABLE_NAME";
		string sql = @"SELECT Tables.TABLE_CATALOG, Tables.TABLE_SCHEMA, Tables.TABLE_NAME,
								  Tables.TABLE_TYPE, NULL AS Create_Date, NULL AS Modify_Date, NULL AS Description
							  FROM INFORMATION_SCHEMA.TABLES AS Tables
							  ORDER BY Tables.TABLE_NAME";

			// Carga las tablas
			using (DbDataReader reader = await connection.ExecuteReaderAsync(sql, null, CommandType.Text, timeout, cancellationToken))
			{ 
				// Recorre la colección de registros
				while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
				{
					TableDbModel table = new TableDbModel();

						// Asigna los datos del registro al objeto
						table.Catalog = reader.IisNull<string>("TABLE_CATALOG");
						table.Schema = reader.IisNull<string>("TABLE_SCHEMA");
						table.Name = reader.IisNull<string>("TABLE_NAME");
						table.CreatedAt = reader.IisNull<DateTime?>("Create_Date");
						table.UpdatedAt = reader.IisNull<DateTime?>("Modify_Date");
						table.Description = reader.IisNull<string>("Description");
						// Añade el objeto a la colección
						schema.Tables.Add(table);
				}
			}
			// Carga los datos de las tablas
			foreach (TableDbModel table in schema.Tables)
			{
				await LoadColumnsAsync(connection, table, timeout, cancellationToken);
				await LoadConstraintsAsync(connection, table, timeout, cancellationToken);
			}
	}

	/// <summary>
	///		Carga la definición de vistas
	/// </summary>
	private async Task LoadViewsAsync(SqlServerProvider connection, SchemaDbModel schema, TimeSpan timeout, CancellationToken cancellationToken)
	{
		string sql = @"SELECT Table_Catalog, Table_Schema, Table_Name, View_Definition, Check_Option, Is_Updatable
							  FROM Information_Schema.Views
							  ORDER BY Table_Name";

			// Carga las vistas
			using (DbDataReader reader = await connection.ExecuteReaderAsync(sql, null, CommandType.Text, timeout, cancellationToken))
			{ 
				// Lee los registros
				while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
				{
					ViewDbModel view = new ViewDbModel();

						// Asigna los datos al objeto
						view.Catalog = reader.IisNull<string>("Table_Catalog");
						view.Schema = reader.IisNull<string>("Table_Schema");
						view.Name = reader.IisNull<string>("Table_Name");
						view.Definition = reader.IisNull<string>("View_Definition");
						view.CheckOption = reader.IisNull<string>("Check_Option");
						view.IsUpdatable = !(reader.IisNull<string>("Is_Updatable") ?? "Unknown").Equals("NO", StringComparison.CurrentCultureIgnoreCase);
						// Añade el objeto a la colección
						schema.Views.Add(view);
				}
			}
			// Carga las columnas de la vista
			foreach (ViewDbModel view in schema.Views)
				await LoadColumnsAsync(connection, view, timeout, cancellationToken);
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
					TriggerDbModel trigger = new TriggerDbModel();

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
		string sql = @"SELECT Routine_Catalog AS Table_Catalog, Routine_Schema AS Table_Schema,
									Routine_Name AS Table_Name, Routine_Type, Routine_Definition
								FROM Information_Schema.Routines
								ORDER BY Routine_Name";

			// Carga los datos
			using (DbDataReader reader = await connection.ExecuteReaderAsync(sql, null, CommandType.Text, timeout, cancellationToken))
			{ 
				// Lee los registros
				while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
				{
					RoutineDbModel routine = new RoutineDbModel();

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
	}

	/// <summary>
	///		Obtiene el tipo de rutina
	/// </summary>
	private RoutineDbModel.RoutineType GetRoutineType(string type)
	{
		if (type.Equals("PROCEDURE", StringComparison.CurrentCultureIgnoreCase))
			return RoutineDbModel.RoutineType.Procedure;
		else if (type.Equals("FUNCTION", StringComparison.CurrentCultureIgnoreCase))
			return RoutineDbModel.RoutineType.Function;
		else
			return RoutineDbModel.RoutineType.Unknown;
	}

	/// <summary>
	///		Carga las restricciones de una tabla
	/// </summary>
	private async Task LoadConstraintsAsync(SqlServerProvider connection, TableDbModel table, TimeSpan timeout, CancellationToken cancellationToken)
	{
		ParametersDbCollection parameters = new ParametersDbCollection();
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
						  WHERE TableConstraints.Table_Catalog = @Table_Catalog
								AND TableConstraints.Table_Schema = @Table_Schema
								AND TableConstraints.Table_Name = @Table_Name
						  ORDER BY TableConstraints.Table_Name, TableConstraints.Constraint_Type, Key_Column.Ordinal_Position";

			// Añade los parámetros
			parameters.Add("@Table_Catalog", table.Catalog);
			parameters.Add("@Table_Schema", table.Schema);
			parameters.Add("@Table_Name", table.Name);
			// Carga los datos
			using (DbDataReader reader = await connection.ExecuteReaderAsync(sql, parameters, CommandType.Text, timeout, cancellationToken))
			{ 
				// Lee los datos
				while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
				{
					ConstraintDbModel constraint = new ConstraintDbModel();

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

	/// <summary>
	///		Obtiene el tipo de una restricción a partir de su nombre
	/// </summary>
	private ConstraintDbModel.ConstraintType GetConstraintType(string type)
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

	/// <summary>
	///		Carga las columnas de una tabla
	/// </summary>
	private async Task LoadColumnsAsync(SqlServerProvider connection, TableDbModel table, TimeSpan timeout, CancellationToken cancellationToken)
	{
		ParametersDbCollection parameters = new ParametersDbCollection();
		string sql;

			// Añade los parámetros
			parameters.Add("@Table_Catalog", table.Catalog);
			parameters.Add("@Table_Schema", table.Schema);
			parameters.Add("@Table_Name", table.Name);
			// Crea la cadena SQL
			sql = @"SELECT DISTINCT Columns.Column_Name, Columns.Ordinal_Position, Columns.Column_Default,
							   Columns.Is_Nullable, Columns.Data_Type, Columns.Character_Maximum_Length,
							   CONVERT(int, Columns.Numeric_Precision) AS Numeric_Precision,
							   CONVERT(int, Columns.Numeric_Precision_Radix) AS Numeric_Precision_Radix,
							   CONVERT(int, Columns.Numeric_Scale) AS Numeric_Scale,
							   CONVERT(int, Columns.DateTime_Precision) AS DateTime_Precision,
							   Columns.Character_Set_Name, Columns.Collation_Catalog, Columns.Collation_Schema, Columns.Collation_Name,
							   Objects.is_identity, CAST(CASE WHEN PrimaryKeys.Column_Name IS NULL THEN 0 ELSE 1 END AS bit) AS IsPrimaryKey,
							   Properties.value AS Description
						  FROM Information_Schema.Columns AS Columns INNER JOIN sys.all_objects AS Tables
								ON Columns.Table_Name = Tables.name
						  INNER JOIN sys.columns AS Objects
								ON Columns.Column_Name = Objects.name
									AND Tables.object_id = Objects.object_id
						  LEFT JOIN (SELECT Constraints.TABLE_CATALOG, Constraints.TABLE_SCHEMA, KeyUsage.TABLE_NAME, KeyUsage.COLUMN_NAME
									   FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS Constraints
									   INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KeyUsage
											ON Constraints.CONSTRAINT_TYPE = 'PRIMARY KEY' 
												AND Constraints.CONSTRAINT_NAME = KeyUsage.CONSTRAINT_NAME
									) AS PrimaryKeys
								ON  Columns.TABLE_CATALOG = PrimaryKeys.TABLE_CATALOG
									AND Columns.TABLE_SCHEMA = PrimaryKeys.TABLE_SCHEMA
									AND Columns.TABLE_NAME = PrimaryKeys.TABLE_NAME
									AND Columns.COLUMN_NAME = PrimaryKeys.COLUMN_NAME
							LEFT JOIN sys.extended_properties AS Properties
								ON Objects.object_id = Properties.major_id
									AND Properties.minor_id = Objects.column_id
									AND Properties.name = 'MS_Description'
						  WHERE Columns.Table_Catalog = @Table_Catalog
								AND Columns.Table_Schema = @Table_Schema
								AND Columns.Table_Name = @Table_Name
						  ORDER BY Columns.Ordinal_Position";
			// Carga los datos
			using (DbDataReader reader = await connection.ExecuteReaderAsync(sql, parameters, CommandType.Text, timeout, cancellationToken))
			{ 
				// Lee los datos
				while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
				{
					FieldDbModel column = new FieldDbModel();

						// Asigna los datos del registro
						column.Name = reader.IisNull<string>("Column_Name");
						column.OrdinalPosition = reader.IisNull<int>("Ordinal_Position", 0);
						column.Default = reader.IisNull<string>("Column_Default");
						column.IsRequired = (reader.IisNull<string>("Is_Nullable") ?? "Unknown").Equals("no", StringComparison.CurrentCultureIgnoreCase);
						column.Type = GetFieldType(reader.IisNull<string>("Data_Type"));
						column.DbType = reader.IisNull<string>("Data_Type");
						column.Length = reader.IisNull<int>("Character_Maximum_Length", 0);
						column.NumericPrecision = reader.IisNull<int>("Numeric_Precision", 0);
						column.NumericPrecisionRadix = reader.IisNull<int>("Numeric_Precision_Radix", 0);
						column.NumericScale = reader.IisNull<int>("Numeric_Scale", 0);
						column.DateTimePrecision = reader.IisNull<int>("DateTime_Precision", 0);
						column.CharacterSetName = reader.IisNull<string>("Character_Set_Name");
						column.CollationCatalog = reader.IisNull<string>("Collation_Catalog");
						column.CollationSchema = reader.IisNull<string>("Collation_Schema");
						column.CollationName = reader.IisNull<string>("Collation_Name");
						column.IsIdentity = reader.IisNull<bool>("is_identity", false);
						column.IsKey = reader.IisNull<bool>("IsPrimaryKey", false);
						column.Description = reader.IisNull<string>("Description");
						// Añade la columna a la colección
						table.Fields.Add(column);
				}
			}
	}

	/// <summary>
	///		Carga las columnas de la vista
	/// </summary>
	private async Task LoadColumnsAsync(SqlServerProvider connection, ViewDbModel view, TimeSpan timeout, CancellationToken cancellationToken)
	{
		ParametersDbCollection parameters = new ParametersDbCollection();
		string sql = @"SELECT Table_Catalog, Table_Schema, Table_Name, Column_Name, Is_Nullable, Data_Type, Character_Maximum_Length
							  FROM Information_Schema.Columns
							  WHERE Table_Catalog = @View_Catalog
									AND Table_Schema = @View_Schema
									AND Table_Name = @View_Name";

			// Asigna lo parámetros
			parameters.Add("@View_Catalog", view.Catalog);
			parameters.Add("@View_Schema", view.Schema);
			parameters.Add("@View_Name", view.Name);
			// Carga las columnas
			using (DbDataReader reader = await connection.ExecuteReaderAsync(sql, parameters, CommandType.Text, timeout, cancellationToken))
			{ 
				// Lee los registros
				while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
				{
					FieldDbModel column = new FieldDbModel();

						// Carga los datos de la columna
						column.Catalog = reader.IisNull<string>("Table_Catalog");
						column.Schema = reader.IisNull<string>("Table_Schema");
						column.Table = reader.IisNull<string>("Table_Name");
						column.Name = reader.IisNull<string>("Column_Name");
						column.IsRequired = !(reader.IisNull<string>("Is_Nullable") ?? "No").Equals("no", StringComparison.CurrentCultureIgnoreCase);
						column.Type = GetFieldType(reader.IisNull<string>("Data_Type"));
						column.DbType = reader.IisNull<string>("Data_Type");
						column.Length = reader.IisNull<int>("Character_Maximum_Length", 0);
						// Añade la columna a la colección
						view.Fields.Add(column);
				}
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