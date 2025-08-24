using Bau.Libraries.LibDbProviders.Base.Models;
using Bau.Libraries.LibDbProviders.Base.RepositoryData;

namespace Bau.Libraries.LibDbProviders.SqlServer;

/// <summary>
///		Clase para ayuda de repository de SQL Server
/// </summary>
public class SqlServerRepository<TypeData>(SqlServerProvider connection) : RepositoryDataBase<TypeData>(connection)
{
	/// <summary>
	///		Devuelve el valor identidad
	/// </summary>
	protected override int? GetIdentityValue(ParametersDbCollection parametersDB) => (int?) parametersDB["@return_code"].Value;
}
