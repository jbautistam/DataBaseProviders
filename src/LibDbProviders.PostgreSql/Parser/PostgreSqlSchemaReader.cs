using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Bau.Libraries.LibDbProviders.Base;
using Bau.Libraries.LibDbProviders.Base.Extensors;
using Bau.Libraries.LibDbProviders.Base.Schema;

namespace Bau.Libraries.LibDbProviders.PostgreSql.Parser
{
    /// <summary>
    ///		Clase para lectura del esquema en una conexión postgress
    /// </summary>
    internal class PostgreSqlSchemaReader
	{
		/// <summary>
		///		Obtiene el esquema
		/// </summary>
		internal async Task<SchemaDbModel> GetSchemaAsync(PostgreSqlProvider provider, TimeSpan timeout, CancellationToken cancellationToken)
		{
			SchemaDbModel schema = new SchemaDbModel();

				// Carga el esquema
				using (PostgreSqlProvider connection = new PostgreSqlProvider(provider.ConnectionString))
				{
					// Abre la conexión
					await connection.OpenAsync(cancellationToken);
					// Carga los datos
					using (DbDataReader rdoData = await connection.ExecuteReaderAsync(GetSqlReadSchema(), null, CommandType.Text, timeout, cancellationToken))
					{ 
						while (!cancellationToken.IsCancellationRequested && await rdoData.ReadAsync(cancellationToken))
							schema.Add(rdoData.IisNull<string>("TableType").Equals("TABLE", StringComparison.CurrentCultureIgnoreCase),
									   rdoData.IisNull<string>("SchemaName"),
									   rdoData.IisNull<string>("TableName"),
									   rdoData.IisNull<string>("ColumnName"),
									   GetFieldType(rdoData.IisNull<string>("ColumnType")),
									   rdoData.IisNull<string>("ColumnType"),
									   rdoData.IisNull<int>("ColumnLength", 0),
									   rdoData.IisNull<int>("PrimaryKey") == 1,
									   rdoData.IisNull<int>("IsRequired") == 1);
					}
				}
				// Devuelve el esquema
				return schema;
		}

		/// <summary>
		///		Cadena SQL para lectura del esquema
		/// </summary>
		private string GetSqlReadSchema()
		{
			return @"SELECT 'TABLE' AS TableType, Columns.TABLE_SCHEMA AS SchemaName, Columns.TABLE_NAME AS TableName, Columns.COLUMN_NAME AS ColumnName, 
						    Columns.UDT_Name AS ColumnType, Columns.CHARACTER_MAXIMUM_LENGTH AS ColumnLength,
						    CASE WHEN tblPrimaryKeys.COLUMN_NAME IS NULL THEN 0
								 ELSE 1 END AS PrimaryKey,
						    CASE WHEN Columns.IS_NULLABLE = 'YES' THEN 1
								 ELSE 0 END AS IsRequired
						FROM INFORMATION_SCHEMA.Columns AS Columns
							LEFT JOIN (SELECT Constraints.TABLE_SCHEMA, Constraints.TABLE_NAME, Constraints.CONSTRAINT_NAME, KeyColumns.COLUMN_NAME
										 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS Constraints
												INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KeyColumns
											ON Constraints.TABLE_SCHEMA = KeyColumns.TABLE_SCHEMA
												AND Constraints.TABLE_NAME = KeyColumns.TABLE_NAME
												AND Constraints.CONSTRAINT_NAME = KeyColumns.CONSTRAINT_NAME
												AND Constraints.CONSTRAINT_TYPE = 'PRIMARY KEY') AS tblPrimaryKeys
								ON Columns.TABLE_SCHEMA = tblPrimaryKeys.TABLE_SCHEMA
									AND Columns.TABLE_NAME = tblPrimaryKeys.TABLE_NAME
									AND Columns.COLUMN_NAME = tblPrimaryKeys.COLUMN_NAME
						WHERE Columns.TABLE_SCHEMA = 'public'	
					 UNION ALL
					 SELECT 'VIEW' AS TableType, ViewColumns.VIEW_SCHEMA AS SchemaName, ViewColumns.VIEW_NAME AS TableName, ViewColumns.COLUMN_NAME AS ColumnName,
						    TableColumns.UDT_Name AS ColumnType, TableColumns.CHARACTER_MAXIMUM_LENGTH AS ColumnLength,
						    0 AS PrimaryKey,
						    CASE WHEN TableColumns.IS_NULLABLE = 'YES' THEN 1
								 ELSE 0 END AS IsRequired
						FROM INFORMATION_SCHEMA.VIEW_COLUMN_USAGE AS ViewColumns
							LEFT JOIN INFORMATION_SCHEMA.COLUMNS AS TableColumns
								ON ViewColumns.TABLE_SCHEMA = TableColumns.TABLE_SCHEMA
									AND ViewColumns.TABLE_NAME = TableColumns.TABLE_NAME
									AND ViewColumns.COLUMN_NAME = TableColumns.COLUMN_NAME
						WHERE ViewColumns.TABLE_SCHEMA = 'Public'";
		}

		/// <summary>
		///		Obtiene el tipo de campo
		/// </summary>
		private FieldDbModel.Fieldtype GetFieldType(string columnType)
		{
			// Normaliza el nombre de columna
			columnType = (columnType ?? "").ToUpper();
			// Obtiene el tipo de campo
			switch (columnType)
			{
				case "INT2":
				case "_INT2":
				case "INT4":
				case "_INT4":
				case "INT8":
				case "_INT8":
					return FieldDbModel.Fieldtype.Integer;
				case "FLOAT4":
				case "_FLOAT4":
				case "FLOAT8":
				case "_FLOAT8":
					return FieldDbModel.Fieldtype.Decimal;
				case "CHAR":
				case "_CHAR":
				case "TEXT":
				case "_TEXT":
				case "VARCHAR":
					return FieldDbModel.Fieldtype.String;
				case "DATE":
					return FieldDbModel.Fieldtype.Date;
				case "BOOL":
					return FieldDbModel.Fieldtype.Boolean;
				default:
					return FieldDbModel.Fieldtype.Unknown;
			}
		}
	}
}
