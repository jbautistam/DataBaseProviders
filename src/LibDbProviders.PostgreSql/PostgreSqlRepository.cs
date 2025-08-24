using Bau.Libraries.LibDbProviders.Base.RepositoryData;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.PostgreSql;

/// <summary>
///		Clase para ayuda de repository de PostgreSql
/// </summary>
public class PostgreSqlRepository<TypeData>(PostgreSqlProvider connection) : RepositoryDataBase<TypeData>(connection)
{
	/// <summary>
	///		Devuelve el valor identidad
	/// </summary>
	protected override int? GetIdentityValue(ParametersDbCollection parametersDB) => (int?) parametersDB["@return_code"].Value;
}
