﻿using Bau.Libraries.LibDbProviders.Base.RepositoryData;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.PostgreSql;

/// <summary>
///		Clase para ayuda de repository de PostgreSql
/// </summary>
public class PostgreSqlRepository<TypeData> : RepositoryDataBase<TypeData>
{
	public PostgreSqlRepository(PostgreSqlProvider connection) : base(connection)
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
