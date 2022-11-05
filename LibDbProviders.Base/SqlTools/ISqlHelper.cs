using System;

using Bau.Libraries.LibDbProviders.Base.Parameters;

namespace Bau.Libraries.LibDbProviders.Base.SqlTools
{
	/// <summary>
	///		Interface para rutinas de ayuda en el tratamiento de SQL
	/// </summary>
	public interface ISqlHelper
	{
		/// <summary>
		///		Obtiene una cadena SQL que cuenta los elementos resultantes de una consulta
		/// </summary>
		string GetSqlCount(string sql);

		/// <summary>
		///		Obtiene la cadena SQL necesaria para paginar
		/// </summary>
		string GetSqlPagination(string sql, int pageNumber, int pageSize);

		/// <summary>
		///		Obtiene una cadena SQL con los valores de los parámetros insertados en la propia cadena
		/// </summary>
		string ConvertSqlNoParameters(string sql, ParametersDbCollection parametersDb);

		/// <summary>
		///	Normaliza la SQL cambiando los parámetros por índices
		///	Obtiene los parámetros de base de datos buscando en la cadena SQL las cadenas: @xxxx (@ es el prefijo del argumento de consulta)
		/// Añade un parámetro a la colección cada vez que se encuentre un nombre de variable
		/// Se hace así porque OleDb y ODBC no admiten parámetros por nombre si no que sustituye los nombres
		/// de parámetros por posición (utilizando el marcador ?)
		/// </summary>
		(string sql, ParametersDbCollection parametersDb) NormalizeSql(string sql, ParametersDbCollection parametersDb);

		/// <summary>
		///		Formatea un nombre de campo / tabla, es decir, SeparatorStart + field + SeparatorEnd
		/// </summary>
		string FormatName(string field);

		/// <summary>
		///		Formatea un nombre de campo y tabla, es decir, SeparatorStart + table + SeparatorEnd + . + SeparatorStart + field + SeparatorEnd
		/// </summary>
		string FormatName(string table, string field);

		/// <summary>
		///		Prefijos de los parámetros
		/// </summary>
		string ParameterPrefix { get; }

		/// <summary>
		///		Separador inicial para el nombre de campos tablas
		/// </summary>
		string SeparatorStart { get; }

		/// <summary>
		///		Separador final para el nombre de campos tablas
		/// </summary>
		string SeparatorEnd { get; }
	}
}
