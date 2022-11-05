﻿using System;

namespace Bau.Libraries.LibDbProviders.Spark.Parser
{
	/// <summary>
	///		Parser para consultas en SQL de Spark
	/// </summary>
	internal class SparkSelectParser : Base.SqlTools.BaseSqlHelper
	{
		/// <summary>
		///		Obtiene una cadena SQL con paginación en el servidor
		/// </summary>
		/// <remarks>
		///		En Spark no se admite offset, sólo se puede lanzar con un límite
		/// </remarks>
		public override string GetSqlPagination(string sql, int pageNumber, int pageSize)
		{
			return $"{sql} LIMIT {pageSize}";
		}

		/// <summary>
		///		Separador inicial para el nombre de campos tablas
		/// </summary>
		public override string SeparatorStart { get; } = "`";

		/// <summary>
		///		Separador inicial para el nombre de campos tablas
		/// </summary>
		public override string SeparatorEnd { get; } = "`";

		/// <summary>
		///		Prefijos de los parámetros
		/// </summary>
		public override string ParameterPrefix { get; } = "$";
	}
}