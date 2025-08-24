namespace Bau.Libraries.LibDbProviders.Base.Schema;

/// <summary>
///		Clase con los datos de una tabla de base de datos
/// </summary>
public class TableDbModel : BaseTableDbModel
{
	/// <summary>
	///		Restricciones
	/// </summary>
	public List<ConstraintDbModel> Constraints { get; } = [];
}
