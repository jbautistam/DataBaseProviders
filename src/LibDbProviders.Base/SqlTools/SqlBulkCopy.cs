using System.Data;
using System.Data.Common;

using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.Base.SqlTools;

/// <summary>
///		BulkCopy genérico
/// </summary>
public class SqlBulkCopy
{
	/// <summary>
	///		Copia masiva de un <see cref="IDataReader"/> sobre una tabla
	/// </summary>
	public long Process(IDbProvider provider, IDataReader reader, string table, Dictionary<string, string> mappings, int recordsPerBlock, TimeSpan? timeout = null)
	{
		long records = 0, blockRecords = 0;
		Dictionary<string, string> mappingsConverted = Convert(reader, mappings);
		string sql = GetInsertCommand(table, mappingsConverted);

			// Abre una transacción
			provider.BeginTransaction();
			// Lee los registros e inserta
			while (reader.Read())
			{
				// Ejecuta el comando de inserción
				provider.Execute(sql, GetParameters(reader, mappingsConverted), CommandType.Text, timeout);
				// Cierra la transacción
				blockRecords++;
				if (blockRecords % recordsPerBlock == 0)
				{
					// Confirma la transacción y abre una nueva
					provider.Commit();
					provider.BeginTransaction();
					// Reinicia el número de registros del bloque
					blockRecords = 0;
				}
				// Incrementa el número de registros
				records++;
			}
			// Cierra la transacción
			if (blockRecords != 0)
				provider.Commit();
			// Devuelve el número de registros copiados
			return records;
	}

	/// <summary>
	///		Copia masiva de un <see cref="IDataReader"/> sobre una tabla de forma asíncrona
	/// </summary>
	public async Task<long> ProcessAsync(IDbProvider provider, IDataReader reader, string table, Dictionary<string, string> mappings, int recordsPerBlock, 
										 TimeSpan timeout, CancellationToken cancellationToken)
	{
		long records = 0, blockRecords = 0;
		Dictionary<string, string> mappingsConverted = Convert(reader, mappings);
		string sql = GetInsertCommand(table, mappingsConverted);

			// Abre una transacción
			provider.BeginTransaction();
			// Lee los registros e inserta
			if (reader is DbDataReader dataReader)
			{
				while (await dataReader.ReadAsync(cancellationToken))
					if (!cancellationToken.IsCancellationRequested)
					{
						// Ejecuta el comando de inserción
						await provider.ExecuteAsync(sql, GetParameters(reader, mappingsConverted), CommandType.Text, timeout, cancellationToken);
						// Cierra la transacción
						blockRecords++;
						if (blockRecords % recordsPerBlock == 0)
						{
							// Confirma la transacción y abre una nueva
							provider.Commit();
							provider.BeginTransaction();
							// Reinicia el número de registros del bloque
							blockRecords = 0;
						}
						// Incrementa el número de registros
						records++;
					}
			}
			else
				throw new ArgumentException($"Can't convert IDataReader reader to DbDataReader");
			// Cancela o confirma la transacción si es necesario
			if (cancellationToken.IsCancellationRequested)
				provider.RollBack();
			else if (blockRecords != 0)
				provider.Commit();
			// Devuelve el número de registros copiados
			return records;
	}

	/// <summary>
	///		Convierte el diccionario
	/// </summary>
	private Dictionary<string, string> Convert(IDataReader reader, Dictionary<string, string> mappings)
	{
		Dictionary<string, string> converted = [];

			// Transforma las claves de mapeo a mayúsculas o crea el mapeo a partir del IDataReader
			if (mappings != null && mappings.Count > 0)
				foreach (KeyValuePair<string, string> mapping in mappings)
					converted.Add(mapping.Key.ToUpperInvariant(), mapping.Value);
			else
				for (int index = 0; index < reader.FieldCount; index++)
					converted.Add(reader.GetName(index).ToUpperInvariant(), reader.GetName(index));
			// Devuelve el diccionario convertido
			return converted;
	}

	/// <summary>
	///		Obtiene la cabecera de una cadena SQL de inserción de datos
	/// </summary>
	private string GetInsertCommand(string table, Dictionary<string, string> mappings)
	{
		string header = $"INSERT INTO {GetFieldName(table)} (";
		string parameters = string.Empty;
		int index = 0;

			// Añade las cabeceras
			foreach (KeyValuePair<string, string> mapping in mappings)
			{
				// Añade el nombre de columna a la cabecera
				header += $"{GetFieldName(mapping.Value)}";
				parameters += GetParameterName(mapping.Value);
				// Añade el separador
				if (index++ < mappings.Count - 1)
				{
					header += ", ";
					parameters += ", ";
				}
			}
			// Devuelve el comando
			return $"{header}) VALUES ({parameters})";
	}

	/// <summary>
	///		Obtiene el nombre de un campo o tabla
	/// </summary>
	private string GetFieldName(string field)
	{
		// Quita los espacios
		field = field.Trim();
		// Añade los corchetes
		if (!field.StartsWith("["))
			field = '[' + field;
		if (!field.EndsWith("]"))
			field += ']';
		// Devuelve el campo
		return field;
	}

	/// <summary>
	///		Obtiene un nombre de parámetro
	/// </summary>
	private string GetParameterName(string name) => $"@{name.Replace(' ', '_')}";

	/// <summary>
	///		Obtiene la colección de parámetros
	/// </summary>
	private ParametersDbCollection GetParameters(IDataReader reader, Dictionary<string, string> mappings)
	{
		ParametersDbCollection parametersDb = [];

			// Asigna los parámetros
			for (int index = 0; index < reader.FieldCount; index++)
			{
				string parameterName = reader.GetName(index).ToUpperInvariant();

					// Añade el valor del campo si está entre los mapeos
					if (mappings.ContainsKey(parameterName))
						parametersDb.Add(GetParameterName(mappings[parameterName]), reader[index]);
			}
			// Devuelve la colección de parámetros
			return parametersDb;
	}
}
