using System;

namespace Bau.Libraries.LibDbProviders.Base.Schema
{
	/// <summary>
	///		Clase base para los datos del modelo
	/// </summary>
	public abstract class BaseSchemaDbModel
	{
		/// <summary>
		///		Esquema de la tabla
		/// </summary>
		public string? Schema { get; set; }

		/// <summary>
		///		Catálogo
		/// </summary>
		public string? Catalog { get; set; }

		/// <summary>
		///		Nombre completo
		/// </summary>
		public string FullName
		{
			get 
			{ 
				string fullName = string.Empty;

					// Añade el catálogo
					if (!string.IsNullOrWhiteSpace(Catalog))
						fullName = $"[{Catalog}]";
					// Añade el esquema
					if (!string.IsNullOrWhiteSpace(Schema))
					{
						// Añade el separador
						if (!string.IsNullOrWhiteSpace(fullName))
							fullName += ".";
						// Añade el esquema
						fullName += $"[{Schema}]";
					}
					// Añade el nombre
					if (!string.IsNullOrWhiteSpace(Name))
					{
						// Añade el separador
						if (!string.IsNullOrWhiteSpace(fullName))
							fullName += ".";
						// Añade el nombre
						fullName += $"[{Name}]";
					}
					// Devuelve el nombre completo
					return fullName; 
			}
		}

		/// <summary>
		///		Nombre
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		///		Descripción
		/// </summary>
		public string? Description { get; set; }

		/// <summary>
		///		Fecha de creación
		/// </summary>
		public DateTime? CreatedAt { get; set; }

		/// <summary>
		///		Fecha de modificación
		/// </summary>
		public DateTime? UpdatedAt { get; set; }
	}
}
