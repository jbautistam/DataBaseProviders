namespace Bau.Libraries.DbAggregator.Exceptions;

/// <summary>
///		Excepción del proveedor de datos
/// </summary>
public class DataProviderException : Exception
{
	public DataProviderException() : base() {}

	protected DataProviderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) {}

	public DataProviderException(string message) : base(message) {}

	public DataProviderException(string message, Exception innerException) : base(message, innerException) {}
}
