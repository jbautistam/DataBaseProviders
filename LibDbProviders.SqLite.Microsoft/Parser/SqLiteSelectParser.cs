using System;

namespace Bau.Libraries.LibDbProviders.SqLite.Parser
{
	/// <summary>
	///		Parser para consultas en SQL de SqLite
	/// </summary>
	internal class SqLiteSelectParser : Base.SqlTools.SqlSelectParserBase
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
	}
}