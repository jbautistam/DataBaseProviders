using System.Data;

namespace Bau.Libraries.LibDbProviders.Base.Extensors;

/// <summary>
///		Funciones de extensión de <see cref="IDataReader"/>
/// </summary>
public static class DataReaderExtensors
{
    /// <summary>
    ///		Obtiene el valor de un campo de un IDataReader
    /// </summary>
    public static object? IisNull(this IDataReader reader, string field, object? defaultValue = null)
    {
        object result = reader[field];

            if (result is DBNull || result is null)
                return defaultValue;
            else
                return result;
    }

    /// <summary>
    ///		Obtiene el valor de un campo de un IDataReader
    /// </summary>
    public static TypeData? IisNull<TypeData>(this IDataReader reader, string field, object? defaultValue = null)
    {
        object result = reader[field];

            if (result is DBNull || result is null)
                return (TypeData?) defaultValue;
            else
                return (TypeData) result;
    }
}
