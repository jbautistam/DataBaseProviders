using System;

namespace Bau.Libraries.LibDbProviders.ODBC.Parser
{
	/// <summary>
	///		Parser para consultas en SQL de ODBC
	/// </summary>
	internal class OdbcSelectParser : Base.SqlTools.BaseSqlHelper
	{
		/// <summary>
		///		Obtiene una cadena SQL con paginación en el servidor
		/// </summary>
		public override string GetSqlPagination(string sql, int pageNumber, int pageSize)
		{
			string offset = "";

				// Calcula la cadena de Offset
				if (pageNumber > 0)
					offset = $"{pageNumber * pageSize}, ";
				// Devuelve la cadena SQL con la paginación
				return $"{sql} LIMIT {offset} {pageSize}";
		}

		/// <summary>
		///		Separador inicial para el nombre de campos tablas
		/// </summary>
		public override string SeparatorStart { get; } = "[";

		/// <summary>
		///		Separador inicial para el nombre de campos tablas
		/// </summary>
		public override string SeparatorEnd { get; } = "[";

		/// <summary>
		///		Prefijos de los parámetros
		/// </summary>
		public override string ParameterPrefix { get; } = "@";
	}
}