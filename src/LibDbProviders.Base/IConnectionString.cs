using System;

namespace Bau.Libraries.LibDbProviders.Base
{
	/// <summary>
	///		Interface para los datos de conexión de un proveedor
	/// </summary>
	public interface IConnectionString
	{
		/// <summary>
		///		Asigna los parámetros de la cadena de conexión a partir de un diccionario
		/// </summary>
		void AssignParameters(Dictionary<string, string> parameters);

		/// <summary>
		///		Compone la cadena de conexión a partir de los parámetros
		/// </summary>
		string ConnectionString { get; set; }

		/// <summary>
		///		Tiempo de espera
		/// </summary>
		TimeSpan Timeout { get; set; }
	}
}
