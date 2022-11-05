using System;

using Bau.Libraries.LibDbProviders.Base.Parameters;
using Bau.Libraries.LibDbProviders.Base.RepositoryData;

namespace Bau.Libraries.LibDbProviders.SqlServer
{
	/// <summary>
	///		Clase para ayuda de repository de SQL Server
	/// </summary>
	public class SqlServerRepository<TypeData> : RepositoryDataBase<TypeData>
	{
		public SqlServerRepository(SqlServerProvider connection) : base(connection)
		{
		}

		/// <summary>
		///		Devuelve el valor identidad
		/// </summary>
		protected override int? GetIdentityValue(ParametersDbCollection parametersDB)
		{ 
			return (int?) parametersDB["@return_code"].Value;
		}
	}
}
