using Bau.Libraries.LibDbProviders.Base.RepositoryData;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.MySql;

/// <summary>
///		Clase para ayuda de repositorios de MySql
/// </summary>
public class MySqlRepository<TypeData>(MySqlProvider connection) : RepositoryDataBase<TypeData>(connection)
{
	/// <summary>
	///		Devuelve el valor identidad
	/// </summary>
	protected override int? GetIdentityValue(ParametersDbCollection parametersDB) => (int?) parametersDB["@return_code"].Value;
}
