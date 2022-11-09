using System;

namespace Bau.Libraries.LibDbProviders.Base.Models
{
    /// <summary>
    ///		Clase con los datos de una consulta
    /// </summary>
    public class QueryModel
	{
		/// <summary>
		///		Tipo de consulta
		/// </summary>
		public enum QueryType
		{
			/// <summary>Texto</summary>
			Text,
			/// <summary>Procedimiento almacenado</summary>
			StoredProcedure,
			/// <summary>Nombre de tabla</summary>
			Table
		}

		/// <summary>
		///		Consulta SQL o script
		/// </summary>
		public string Sql { get; set; }

		/// <summary>
		///		Tipo de consulta
		/// </summary>
		public QueryType Type { get; set; }

		/// <summary>
		///		Argumentos / parámetros de la consulta
		/// </summary>
		public ParametersDbCollection Parameters { get; } = new();

		/// <summary>
		///		Tiempo de espera de la consulta
		/// </summary>
		public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

		/// <summary>
		///		Modo de paginación
		/// </summary>
		public PaginationModel Pagination { get; } = new PaginationModel();
	}
}
