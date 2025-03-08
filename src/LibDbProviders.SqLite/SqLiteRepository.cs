using Bau.Libraries.LibDbProviders.Base.RepositoryData;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.SqLite;

/// <summary>
///		Clase para ayuda de repositorios de SqLite
/// </summary>
public class SqLiteRepository<TypeData> : RepositoryDataBase<TypeData>
{
	public SqLiteRepository(SqLiteProvider connection) : base(connection)
	{
	}

	/// <summary>
	///		Devuelve el valor identidad
	/// </summary>
	protected override int? GetIdentityValue(ParametersDbCollection parametersDB) => (int?) parametersDB["@return_code"].Value;
	
	/// <summary>
	///		Devuelve el valor identidad de una tabla
	/// </summary>
	protected async Task<long?> GetIdentityValueAsync(string table, CancellationToken cancellationToken)
	{ 
		long? identity;

			// Abre la conexión
			await Connection.OpenAsync(cancellationToken);
			// Carga el último valor de la secuencia de la tabla
			identity = (long?) await Connection.ExecuteScalarAsync($"""
																	SELECT seq 
																		FROM sqlite_sequence 
																		WHERE Name = '{table.Replace("'", "''")}'
																	""",
																	null, System.Data.CommandType.Text, null, cancellationToken);
			// Cierra la base de datos
			Connection.Close();
			// Devuelve el valor de la indentidad
			return identity;
	}
}
