namespace Bau.Libraries.DbAggregator.Exceptions;

/// <summary>
///		Excepción del proveedor de datos
/// </summary>
public class DataProviderException : Exception
{
	public DataProviderException() : base() {}

	public DataProviderException(string message) : base(message) {}

	public DataProviderException(string message, Exception innerException) : base(message, innerException) {}
}
