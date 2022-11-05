using System;

namespace Bau.Libraries.LibDbProviders.ODBC
{
	/// <summary>
	///		Cadena de conexión de OleDB
	/// </summary>
	public class OdbcConnectionString : Base.DbConnectionStringBase
	{ 
		public OdbcConnectionString(string connectionString, int timeout = 15) : base(connectionString, timeout) {}

		public OdbcConnectionString(System.Collections.Generic.Dictionary<string, string> parameters, int timeout = 15) : base(parameters, timeout) {}

		/// <summary>
		///		Asigna el valor de un parámetro
		/// </summary>
		protected override void AssignParameter(string key, string value)
		{
			if (IsEqual(key, nameof(ConnectionString)))
				ConnectionString = value;
		}
	}
}