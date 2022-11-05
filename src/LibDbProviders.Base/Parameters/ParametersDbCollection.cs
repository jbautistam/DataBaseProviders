using System;
using System.Data;

namespace Bau.Libraries.LibDbProviders.Base.Parameters
{
	/// <summary>
	///		Clase que almacena los parámetros de un comando
	/// </summary>
	public class ParametersDbCollection : System.Collections.Generic.List<ParameterDb>
	{ 
		// Constantes privadas
		private const string ReturnCodeName = "@ReturnCode";

		/// <summary>
		///		Añade un parámetro a la colección de parámetros del comando
		/// </summary>
		public void Add(string name, string value, int length, ParameterDirection direction = ParameterDirection.Input)
		{ 
			// Corta la cadena para que se ajuste a la colección
			if (!string.IsNullOrEmpty(value) && value.Length > length)
				value = value.Substring(0, length);
			// Añade el parámetro
			Add(new ParameterDb(name, value, direction, length));
		}

		/// <summary>
		///		Añade un parámetro a la colección de parámetros del comando
		/// </summary>
		public void Add(string name, object value, ParameterDirection direction = ParameterDirection.Input)
		{ 
			Add(new ParameterDb(name, value, direction));
		}		
		
		/// <summary>
		///		Añade un parámetro a la colección de parámetros del comando
		/// </summary>
		public void Add(string name, byte [] buffer, ParameterDirection direction = ParameterDirection.Input)
		{ 
			Add(new ParameterDb(name, buffer, direction));
		}

		/// <summary>
		///		Añade un parámetro de tipo Text
		/// </summary>
		public void AddText(string name, string value, ParameterDirection direction = ParameterDirection.Input)
		{
			Add(new ParameterDb(name, value, direction)
								{
									IsText = true
								}
				);
		}

		/// <summary>
		///		Añade un parámetro para el código de retorno
		/// </summary>
		public void AddReturnCode()
		{ 
			Add(ReturnCodeName, 0, ParameterDirection.ReturnValue);
		}

		/// <summary>
		///		Obtiene un parámetro de la colección a partir del nombre, si no lo encuentra devuelve un parámetro vacío
		/// </summary>
		public ParameterDb Search(string name)
		{ 
			// Recorre la colección de parámetros buscando el elemento adecuado
			foreach (ParameterDb parameter in this)
				if (parameter.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
					return parameter;
			// Devuelve un objeto vacío
			return new ParameterDb(name, null, ParameterDirection.Input);
		}
		
		/// <summary>
		///		Obtiene el código de retorno
		/// </summary>
		public int? GetReturnCode()
		{ 
			ParameterDb parameterDB = Search(ReturnCodeName);

				if (parameterDB.Value == null || !(parameterDB.Value is int))
					return null;
				else
					return (int) parameterDB.Value;
		}

		/// <summary>
		///		Obtiene el valor de un parámetro o un valor predeterminado si es null
		/// </summary>
		public object IisNull(string name)
		{ 
			ParameterDb parameterDB = Search(name);
		
				if (parameterDB.Value == DBNull.Value)
					return null;
				else
					return parameterDB.Value;
		}

		/// <summary>
		///		Normaliza dos fechas (sin nulos)
		/// </summary>
		public void NormalizeDates(ref DateTime start, ref DateTime end) 
		{ 
			DateTime? startNull = start, endNull = end;

				// Normaliza las fechas
				NormalizeDates(ref startNull, ref endNull);
				// Asigna las fechas de salida (en realidad, nunca pueden ser nulas porque se ha hecho ya una asignación)
				start = startNull ?? start;
				end = endNull ?? end;
		}

		/// <summary>
		///		Normaliza las fechas
		/// </summary>
		public void NormalizeDates(ref DateTime? start, ref DateTime? end) 
		{ 
			// Cambia las fechas
			SwapDates(ref start, ref end);
			// Normaliza las fechas de inicio o fin
			if (start != null)
				start = GetNormalizedDate(start, 0, 0, 0);
			if (end != null)
				end = GetNormalizedDate(end, 23, 59, 59);
		}
		
		/// <summary>
		///		Intercambia dos fechas para un filtro
		/// </summary>
		private void SwapDates(ref DateTime? first, ref DateTime? second)
		{ 
			if (first != null && second != null && first > second)
		    { 
				DateTime? value = first;
				
					first = second;
					second = value;
		    }
		}

		/// <summary>
		///		Obtiene una fecha normalizada al inicio o fin del día
		/// </summary>
		private DateTime? GetNormalizedDate(DateTime? value, int hour, int minute, int second) 
		{ 
			DateTime normalized = value ?? DateTime.Now;

				// Devuelve la fecha normalizada
				return new DateTime(normalized.Year, normalized.Month, normalized.Day, hour, minute, second);
		}

		/// <summary>
		///		Indizador de la colección por el nombre de parámetro
		/// </summary>
		public ParameterDb this[string name]
		{ 
			get { return Search(name); }
		}
	}
}