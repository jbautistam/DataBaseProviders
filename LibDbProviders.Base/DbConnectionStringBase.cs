using System;
using System.Collections.Generic;

namespace Bau.Libraries.LibDbProviders.Base 
{
	/// <summary>
	///		Clase que implementa la base de una cadena de conexión
	/// </summary>
	public abstract class DbConnectionStringBase : IConnectionString
	{
		protected DbConnectionStringBase(string connectionString, int timeout = 15)
		{ 
			ConnectionString = connectionString;
			Timeout = timeout;
		}

		protected DbConnectionStringBase(Dictionary<string, string> parameters, int timeout = 15)
		{
			AssignParameters(parameters);
			Timeout = timeout;
		}

		/// <summary>
		///		Asigna los parámetros de la cadena de conexión a partir de un diccionario
		/// </summary>
		public void AssignParameters(Dictionary<string, string> parameters)
		{
			foreach (KeyValuePair<string, string> parameter in parameters)
				if (!string.IsNullOrWhiteSpace(parameter.Key))
				{
					if (IsEqual(parameter.Key, nameof(Timeout)))
						Timeout = GetInt(parameter.Value, 15);
					else
						AssignParameter(parameter.Key.Trim(), Trim(parameter.Value));
				}
		}

		/// <summary>
		///		Asigna un parámetro
		/// </summary>
		protected abstract void AssignParameter(string key, string value);

		/// <summary>
		///		Comprueba si dos cadenas son iguales (sin tener en cuenta las mayúsculas)
		/// </summary>
		protected bool IsEqual(string first, string second)
		{
			return Trim(first).Equals(Trim(second), StringComparison.CurrentCultureIgnoreCase);
		}

		/// <summary>
		///		Quita los espacios de una cadena
		/// </summary>
		protected string Trim(string value)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;
			else
				return value.Trim();
		}

		/// <summary>
		///		Convierte un valor entero
		/// </summary>
		protected int GetInt(string value, int defaultValue)
		{
			if (int.TryParse(value, out int result))
				return result;
			else
				return defaultValue;
		}

		/// <summary>
		///		Convierte un valor lógico
		/// </summary>
		protected bool GetBool(string value, bool defaultValue = false)
		{
			if (string.IsNullOrWhiteSpace(value))
				return defaultValue;
			else
				return value.Equals("true", StringComparison.CurrentCultureIgnoreCase) || value.Equals("1");
		}

		/// <summary>
		///		Obtiene el valor de un enumerado
		/// </summary>
		protected TypeEnum GetEnum<TypeEnum>(string value, TypeEnum defaultValue) where TypeEnum : struct
		{
			if (Enum.TryParse(value, true, out TypeEnum result))
				return result;
			else
				return defaultValue;
		}

		/// <summary>
		///		Cadena de conexión
		/// </summary>
		public virtual string ConnectionString { get; set; }

		/// <summary>
		///		Tiempo de espera
		/// </summary>
		public int Timeout { get; set; }
	}
}
