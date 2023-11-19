using Bau.Libraries.LibDataStructures.Collections;

namespace Bau.Libraries.DbAggregator.Models;

/// <summary>
///		Clase con los datos de una conexión a base de datos
/// </summary>
public class ConnectionModel : LibDataStructures.Base.BaseExtendedModel
{
	/// <summary>
	///		Tipos de base de datos admitidos
	/// </summary>
	public enum DataBaseType
	{
		/// <summary>Desconocido. No se debería utilizar</summary>
		Unknown,
		/// <summary>SqLite</summary>
		SqLite,
		/// <summary>Sql Server</summary>
		SqlServer,
		/// <summary>Odbc</summary>
		Odbc,
		/// <summary>PostgreSql</summary>
		PostgreSql,
		/// <summary>MySql</summary>
		MySql,
		/// <summary>Spark</summary>
		Spark
	}

	public ConnectionModel(string key, string type)
	{
		Key = key;
		Type = type;
	}

	/// <summary>
	///		Clave de la conexión
	/// </summary>
	public string Key { get; }

	/// <summary>
	///		Tipo de conexión
	/// </summary>
	public string Type { get; }

	/// <summary>
	///		Parámetros de conexión
	/// </summary>
	public NormalizedDictionary<string> Parameters { get; } = new();
}
