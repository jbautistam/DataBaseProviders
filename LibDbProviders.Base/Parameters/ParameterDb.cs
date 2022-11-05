using System;

namespace Bau.Libraries.LibDbProviders.Base.Parameters
{
	/// <summary>
	///		Clase con los datos de un parámetro
	/// </summary>
	public class ParameterDb
	{ 
		/// <summary>
		///		Dirección del parámetro
		/// </summary>
		public enum ParameterDbDirection
		{
			/// <summary>Parámetro de entrada</summary>
			Input,
			/// <summary>Parámetro de salida</summary>
			Output,
			/// <summary>Parámetro de entrada / salida</summary>
			InputOutput,
			/// <summary>Valor de retorno</summary>
			ReturnValue
		}

		public ParameterDb() {}

		public ParameterDb(string name, object value, System.Data.ParameterDirection direction, int length = 0)
		{ 
			if (!string.IsNullOrEmpty(name))
				Name = name.Trim();
			if (value is Enum)
				Value = (int) value;
			else
				Value = value;
			Direction = direction;
			Length = length;
		}

		/// <summary>
		///		Obtiene un valor de tipo objeto o nulo para la base de datos
		/// </summary>
		public object GetDBValue()
		{ 
			if (Value == null)
				return DBNull.Value;
			else if (Value is Enum)
				return (int) Value;
			else 
				return Value;
		}

		/// <summary>
		///		Nombre del parámetro
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///		Valor del parámetro
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		///		Dirección del parámetro
		/// </summary>
		public System.Data.ParameterDirection Direction { get; set; }

		/// <summary>
		///		Longitud
		/// </summary>
		public int Length { get; set; }

		/// <summary>
		///		Indica si es un parámetro de texto
		/// </summary>
		public bool IsText { get; set; }
	}
}