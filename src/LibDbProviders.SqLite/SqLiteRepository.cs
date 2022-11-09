using System;

using Bau.Libraries.LibDbProviders.Base.RepositoryData;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.SqLite
{
    /// <summary>
    ///		Clase para ayuda de repository de SqLite
    /// </summary>
    public class SqLiteRepository<TypeData> : RepositoryDataBase<TypeData>
	{
		public SqLiteRepository(SqLiteProvider connection) : base(connection)
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
