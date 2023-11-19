namespace Bau.Libraries.LibDbProviders.Base.Models;

/// <summary>
///		Datos de paginación
/// </summary>
public class PaginationModel
{
	/// <summary>
	///		Indica si se debe paginar la consulta
	/// </summary>
	public bool MustPaginate { get; set; }

	/// <summary>
	///		Página actual
	/// </summary>
	public int Page { get; set; }

	/// <summary>
	///		Tamaño de página
	/// </summary>
	public int PageSize { get; set; }
}
