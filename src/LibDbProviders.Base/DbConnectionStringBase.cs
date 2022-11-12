using System;

namespace Bau.Libraries.LibDbProviders.Base 
{
	/// <summary>
	///		Clase que implementa la base de una cadena de conexión
	/// </summary>
	public abstract class DbConnectionStringBase : IConnectionString
	{
		// Variables privadas
		private string _connectionString = string.Empty;

		protected DbConnectionStringBase(string connectionString)
		{ 
			ConnectionString = connectionString;
		}

		protected DbConnectionStringBase(Dictionary<string, string> parameters)
		{
			AssignParameters(parameters);
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
						Timeout = TimeSpan.FromMinutes(GetInt(parameter.Value, 15));
					else
						AssignParameter(parameter.Key.Trim(), Trim(parameter.Value));
				}
		}

		/// <summary>
		///		Asigna un parámetro
		/// </summary>
		protected abstract void AssignParameter(string key, string value);

		/// <summary>
		///		Genera la cadena de conexión
		/// </summary>
		protected abstract string GenerateConnectionString();

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
		public string ConnectionString
		{ 
			get 
			{
				// Si no se ha definido la cadena, se genera
				if (string.IsNullOrWhiteSpace(_connectionString))
					_connectionString = GenerateConnectionString();
				// Devuelve la cadena de conexión
				return _connectionString;
			}
			set { _connectionString = value ?? string.Empty; }
		}

		/// <summary>
		///		Tiempo de espera
		/// </summary>
		public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);
	}
}
