using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bau.Libraries.LibDbProviders.Base.Extensors;
using Bau.Libraries.LibDbProviders.Base.Schema;

namespace Bau.Libraries.LibDbProviders.ODBC
{
    /// <summary>
    ///		Lector de esquema para ODBC
    /// </summary>
    internal class OdbcSchemaReader
	{
		/// <summary>
		///		Obtiene el esquema
		/// </summary>
		internal async Task<SchemaDbModel> GetSchemaAsync(OdbcProvider provider, TimeSpan timeout, CancellationToken cancellationToken)
		{
			SchemaDbModel schema = new SchemaDbModel();
			List<string> tables = new List<string>();

				// Carga el esquema
				using (OdbcConnection connection = new OdbcConnection(provider.ConnectionString.ConnectionString))
				{
					// Abre la conexión
					await connection.OpenAsync(cancellationToken);
					// Obtiene las tablas
					using (DataTable table = connection.GetSchema("Tables"))
					{
						foreach (DataRow row in table.Rows)
							if (!cancellationToken.IsCancellationRequested && 
									row.IisNull<string>("Table_Type").Equals("TABLE", StringComparison.CurrentCultureIgnoreCase))
								tables.Add(row.IisNull<string>("Table_Name"));
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
											   GetFieldType(row.IisNull<string>("Type_Name")),
											   row.IisNull<string>("Type_Name"),
											   row.IisNull<int>("Column_Size", 0),
											   false,
											   row.IisNull<string>("Is_Nullable").Equals("No", StringComparison.CurrentCultureIgnoreCase));
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
						return FieldDbModel.Fieldtype.Boolean;
					case "decimal":
					case "numeric":
					case "double":
					case "real":
						return FieldDbModel.Fieldtype.Decimal;
					case "int":
					case "smallint":
					case "tinyint":
						return FieldDbModel.Fieldtype.Integer;
					case "nchar":
					case "char":
					case "ntext":
					case "nvarchar":
					case "uniqueidentifier":
					case "text":
					case "varchar":
						return FieldDbModel.Fieldtype.String;
					case "smalldatetime":
					case "datetime":
					case "date":
					case "time":
						return FieldDbModel.Fieldtype.Date;
					default:
						return FieldDbModel.Fieldtype.Unknown;
				}
		}
	}
}
