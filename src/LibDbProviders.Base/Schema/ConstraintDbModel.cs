using System;

namespace Bau.Libraries.LibDbProviders.Base.Schema
{
	/// <summary>
	///		Clase con los datos de una restricción
	/// </summary>
	public class ConstraintDbModel : BaseSchemaDbModel
	{ 
		// Enumerados
		/// <summary>
		///		Tipo de restricción
		/// </summary>
		public enum ConstraintType
		{
			/// <summary>Desconocido</summary>
			Unknown,
			/// <summary>Clave primaria</summary>
			PrimaryKey,
			/// <summary>Clave foránea</summary>
			ForeignKey,
			/// <summary>Unico</summary>
			Unique
		}

		/// <summary>
		///		Tabla
		/// </summary>
		public string? Table { get; set; }

		/// <summary>
		///		Columna
		/// </summary>
		public string? Column { get; set; }

		/// <summary>
		///		Tipo de restricción
		/// </summary>
		public ConstraintType Type { get; set; }

		/// <summary>
		///		Posición de la restricción
		/// </summary>
		public int Position { get; set; }
	}
}
