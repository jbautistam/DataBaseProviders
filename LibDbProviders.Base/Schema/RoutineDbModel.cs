using System;

namespace Bau.Libraries.LibDbProviders.Base.Schema
{
	/// <summary>
	///		Clase con los datos de una rutina
	/// </summary>
	public class RoutineDbModel : BaseSchemaDbModel
	{
		// Enumerados
		/// <summary>
		///		Tipo de rutina
		/// </summary>
		public enum RoutineType
		{
			/// <summary>Desconocido. No se debería utilizar</summary>
			Unknown,
			/// <summary>Procedimiento</summary>
			Procedure,
			/// <summary>Función</summary>
			Function
		}

		/// <summary>
		///		Contenido de la rutina 
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		///		Tipo de la rutina
		/// </summary>
		public RoutineType Type { get; set; }
	}
}
