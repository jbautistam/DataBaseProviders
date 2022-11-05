using System;

namespace Bau.Libraries.LibDbProviders.Base.Parameters
{
	/// <summary>
	///		Clase con los datos de un par�metro
	/// </summary>
	public class ParameterDb
	{ 
		/// <summary>
		///		Direcci�n del par�metro
		/// </summary>
		public enum ParameterDbDirection
		{
			/// <summary>Par�metro de entrada</summary>
			Input,
			/// <summary>Par�metro de salida</summary>
			Output,
			/// <summary>Par�metro de entrada / salida</summary>
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
		///		Nombre del par�metro
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///		Valor del par�metro
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		///		Direcci�n del par�metro
		/// </summary>
		public System.Data.ParameterDirection Direction { get; set; }

		/// <summary>
		///		Longitud
		/// </summary>
		public int Length { get; set; }

		/// <summary>
		///		Indica si es un par�metro de texto
		/// </summary>
		public bool IsText { get; set; }
	}
}