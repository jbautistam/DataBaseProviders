using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bau.Libraries.LibDbProviders.Base.Extensors;
using Bau.Libraries.LibDbProviders.Base.Schema;

namespace Bau.Libraries.LibDbProviders.Spark.Parser
{
    /// <summary>
    ///		Lector de esquema para Spark
    /// </summary>
    internal class SparkSchemaReader
	{
		/// <summary>
		///		Obtiene el esquema
		/// </summary>
		internal async Task<SchemaDbModel> GetSchemaAsync(SparkProvider provider, bool includeSystemTables, TimeSpan timeout, CancellationToken cancellationToken)
		{
			SchemaDbModel schema = new SchemaDbModel();
			List<string> tables = new List<string>();
			
				// Obtiene el esquema
				using (OdbcConnection connection = new OdbcConnection(provider.ConnectionString.ConnectionString))
				{
					// Abre la conexión
					await connection.OpenAsync(cancellationToken);
					// Obtiene las tablas
					using (DataTable table = connection.GetSchema("Tables"))
					{
						foreach (DataRow row in table.Rows)
							if (!cancellationToken.IsCancellationRequested && 
									(row.IisNull<string>("Table_Type") ?? "Unknown").Equals("TABLE", StringComparison.CurrentCultureIgnoreCase))
								tables.Add(row.IisNull<string>("Table_Name") ?? "Unkwnonw");
					}
					// Carga las columnas
					if (!cancellationToken.IsCancellationRequested)
						using (DataTable table = connection.GetSchema("Columns"))
						{
							foreach (DataRow row in table.Rows)
								if (!cancellationToken.IsCancellationRequested && 
										tables.FirstOrDefault(item => item.Equals(row.IisNull<string>("Table_Name"), StringComparison.CurrentCultureIgnoreCase)) != null)
									schema.Add(true,
											   row.IisNull<string>("Table_Schem"),
											   row.IisNull<string>("Table_Name"),
											   row.IisNull<string>("Column_Name"),
											   GetFieldType(row.IisNull<string>("Type_Name") ?? "Unknown"),
											   row.IisNull<string>("Type_Name"),
											   row.IisNull<int>("Column_Size", 0),
											   false,
											   (row.IisNull<string>("Is_Nullable") ?? "Unknown").Equals("No", StringComparison.CurrentCultureIgnoreCase));
						}
				}
				// Devuelve el esquema
				return schema;
		}

		/// <summary>
		///		Tipo de campo
		/// </summary>
		private FieldDbModel.Fieldtype GetFieldType(string fieldType)
		{
			if (string.IsNullOrEmpty(fieldType))
				return FieldDbModel.Fieldtype.Unknown;
			else
				switch (fieldType.ToLower())
				{
					case "bit":
					case "boolean":
						return FieldDbModel.Fieldtype.Boolean;
					case "decimal":
					case "numeric":
					case "double":
					case "real":
					case "float":
						return FieldDbModel.Fieldtype.Decimal;
					case "int":
					case "smallint":
					case "tinyint":
					case "bigint":
						return FieldDbModel.Fieldtype.Integer;
					case "binary":
						return FieldDbModel.Fieldtype.Binary;
					case "nchar":
					case "char":
					case "ntext":
					case "nvarchar":
					case "uniqueidentifier":
					case "text":
					case "varchar":
					case "string":
						return FieldDbModel.Fieldtype.String;
					case "smalldatetime":
					case "datetime":
					case "date":
					case "time":
					case "timestamp":
						return FieldDbModel.Fieldtype.Date;
					default:
						return FieldDbModel.Fieldtype.Unknown;
				}
		}
	}
}
