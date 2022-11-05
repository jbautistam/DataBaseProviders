using System;
using System.Collections.Generic;

namespace Bau.Libraries.LibDbProviders.Base.Parameters
{
	/// <summary>
	///		Parámetros de paginación de una consulta
	/// </summary>
	public class Pagination
	{
		public Pagination()
		{ 
			FieldsOrder = new List<string>();
		}

		/// <summary>
		///		Indica si se debe recoger el número de registros
		/// </summary>
		public bool MustCountRecords { get; set; }

		/// <summary>
		///		Página que se debe leer
		/// </summary>
		public int Page { get; set; }

		/// <summary>
		///		Registros por página leída
		/// </summary>
		public int RecordsPerPage { get; set; }

		/// <summary>
		///		Numero de registros total de la consulta
		/// </summary>
		public int RecordsNumber { get; set; }

		/// <summary>
		///		Indica si se deben eliminar las cláusulas DISTINCT en las cadenas de consulta
		/// </summary>
		public bool RemoveDistinct { get; set; }

		/// <summary>
		///		Campos de ordenación
		/// </summary>
		public List<string> FieldsOrder { get; private set; }
	}
}
