using System;

namespace Bau.Libraries.LibDbProviders.Base.Schema
{
	/// <summary>
	///		Clase con los datos de una vista de base de datos
	/// </summary>
	public class ViewDbModel : BaseTableDbModel
	{
		/// <summary>
		///		Definición
		/// </summary>
		public string? Definition { get; set; }

		/// <summary>
		///		Opción check
		/// </summary>
		public string? CheckOption { get; set; }

		/// <summary>
		///		Indica si es modificable
		/// </summary>
		public bool IsUpdatable { get; set; }
	}
}
