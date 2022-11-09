using System;

namespace Bau.Libraries.LibDbProviders.Base.Exceptions
{
	/// <summary>
	///		Excepción de base de datos
	/// </summary>
	public class DbException : Exception
	{
		public DbException() : base() {}

		public DbException(string message) : base(message) {}

		public DbException(string message, Exception innerException) : base(message, innerException) {}

		protected DbException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) {}
	}
}
