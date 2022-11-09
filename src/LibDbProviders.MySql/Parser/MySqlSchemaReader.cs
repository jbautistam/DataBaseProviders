using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Bau.Libraries.LibDbProviders.Base;
using Bau.Libraries.LibDbProviders.Base.Extensors;
using Bau.Libraries.LibDbProviders.Base.Schema;

namespace Bau.Libraries.LibDbProviders.MySql.Parser
{
    /// <summary>
    ///		Clase de lectura de esquemas para MySql
    /// </summary>
    internal class MySqlSchemaReader
	{
		/// <summary>
		///		Obtiene el esquema
		/// </summary>
		internal async Task<SchemaDbModel> GetSchemaAsync(MySqlProvider provider, TimeSpan timeout, CancellationToken cancellationToken)
		{
			SchemaDbModel schema = new SchemaDbModel();

				// Carga los datos del esquema
				using (MySqlProvider connection = new MySqlProvider(provider.ConnectionString))
				{
					// Abre la conexión
					await connection.OpenAsync(cancellationToken);
					// Carga los datos 
					//? Obtiene el DataTable y crea un IDataReader a partir de él porque cuando se llama a provider.ExecuteReader da un error de 
					//? "Invalid attempt to Read when reader is closed"
					using (DbDataReader rdoData = (await connection.GetDataTableAsync(GetSqlReadSchema(GetDataBase(provider.ConnectionString.ConnectionString)), 
																				      null, CommandType.Text))
															.CreateDataReader())
					{
						while (!cancellationToken.IsCancellationRequested && await rdoData.ReadAsync(cancellationToken))
							schema.Add(rdoData.IisNull<string>("TableType").Equals("TABLE", StringComparison.CurrentCultureIgnoreCase),
									   rdoData.IisNull<string>("SchemaName"),
									   rdoData.IisNull<string>("TableName"),
									   rdoData.IisNull<string>("ColumnName"),
									   GetFieldType(rdoData.IisNull<string>("ColumnType")),
									   rdoData.IisNull<string>("ColumnType"),
									   ConvertToInt(rdoData.IisNull<string>("ColumnLength")),
									   rdoData.IisNull<long>("PrimaryKey") == 1,
									   rdoData.IisNull<long>("IsRequired") == 1);
					}
					// Cierra la conexión
					connection.Close();
				}
				// Devuelve el esquema
				return schema;
		}

		/// <summary>
		///		Convierte una cadena a entero
		/// </summary>
		private int ConvertToInt(string value)
		{
			if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int result))
				return result;
			else
				return 0;
		}

		/// <summary>
		///		Obtiene la base de datos de una cadena de conexión
		/// </summary>
		private string GetDataBase(string connectionString)
		{
			// Separa las secciones de la cadena de conexión para obtener la base de datos
			if (!string.IsNullOrEmpty(connectionString))
				foreach (string part in connectionString.Split(';'))
					if (!string.IsNullOrEmpty(part))
					{
						string [] parts = part.Split('=');

							if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]) &&
									!string.IsNullOrWhiteSpace(parts[1]) &&
									parts[0].Equals("DataBase", StringComparison.CurrentCultureIgnoreCase))
								return parts[1];
					}
			// Si ha llegado hasta aquí es porque no ha encontrado nada
			return "";
		}

		/// <summary>
		///		Cadena SQL para lectura del esquema
		/// </summary>
		private string GetSqlReadSchema(string dataBase)
		{
			return $@"SELECT CASE WHEN Tables.Table_Type = 'VIEW' THEN 'VIEW'
								  ELSE 'TABLE' END AS TableType, 
						     Columns.Table_Schema AS SchemaName, Columns.Table_Name AS TableName, Columns.Column_Name AS ColumnName,
						     Columns.Data_Type AS ColumnType, CAST(Columns.Character_Maximum_Length AS Char) AS ColumnLength, 
						     CASE WHEN tblPrimaryKeys.Column_Name IS NULL THEN 0
	   							  ELSE 1 END AS PrimaryKey,
						     CASE WHEN Columns.Is_Nullable = 'YES' THEN 1
	   							  ELSE 0 END AS IsRequired
						FROM `Information_Schema`.`Columns` AS Columns INNER JOIN `Information_Schema`.`Tables` AS Tables
							ON Columns.Table_Schema = Tables.Table_Schema
								AND Columns.Table_Name = Tables.Table_Name
						LEFT JOIN (SELECT Constraints.TABLE_SCHEMA, Constraints.TABLE_NAME, Constraints.CONSTRAINT_NAME, KeyColumns.COLUMN_NAME
									 FROM `INFORMATION_SCHEMA`.`TABLE_CONSTRAINTS` AS Constraints
											INNER JOIN `INFORMATION_SCHEMA`.`KEY_COLUMN_USAGE` AS KeyColumns
										ON Constraints.TABLE_SCHEMA = KeyColumns.TABLE_SCHEMA
											AND Constraints.TABLE_NAME = KeyColumns.TABLE_NAME
											AND Constraints.CONSTRAINT_NAME = KeyColumns.CONSTRAINT_NAME
											AND Constraints.CONSTRAINT_TYPE = 'PRIMARY KEY') AS tblPrimaryKeys
							ON Columns.TABLE_SCHEMA = tblPrimaryKeys.TABLE_SCHEMA
								AND Columns.TABLE_NAME = tblPrimaryKeys.TABLE_NAME
								AND Columns.COLUMN_NAME = tblPrimaryKeys.COLUMN_NAME
						WHERE Tables.Table_Schema = '{dataBase}'";
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
				case "INT":
				case "BIGINT":
				case "TINYINT":
				case "SMALLINT":
					return FieldDbModel.Fieldtype.Integer;
				case "FLOAT":
				case "DECIMAL":
				case "DOUBLE":
					return FieldDbModel.Fieldtype.Decimal;
				case "CHAR":
				case "TEXT":
				case "VARCHAR":
				case "LONGTEXT":
				case "MEDIUMTEXT":
					return FieldDbModel.Fieldtype.String;
				case "DATETIME":
				case "TIME":
					return FieldDbModel.Fieldtype.Date;
				case "BOOL":
					return FieldDbModel.Fieldtype.Boolean;
				default:
					return FieldDbModel.Fieldtype.Unknown;
			}
		}
	}
}