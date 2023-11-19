using System.Data;

namespace Bau.Libraries.LibDbProviders.Base.Extensors;

/// <summary>
///		Funciones de extensión de <see cref="DataTable"/>
/// </summary>
public static class DataTableExtensors
{
    /// <summary>
    ///		Tipo de datos reducidos de los campos
    /// </summary>
    /// <remarks>
    ///		La propiedad DataType admite de un dataTable admite los siguientes tipos de datos base .NET Framework 
    ///		(<seealso cref="https://docs.microsoft.com/es-es/dotnet/api/system.data.datacolumn.datatype?view=netcore-3.1"/>):
    ///		Boolean, Byte, Char, DateTime, Decimal, Double, Guid, Int16, Int32, Int64, SByte, Single, String, TimeSpan, UInt16, UInt32, UInt64
    ///		y tipo de matriz siguiente: Byte[]
    /// </remarks>
    public enum FieldType
    {
        /// <summary>Desconocido. No se ha podido interpretar el tipo</summary>
        Unknown,
        /// <summary>Cadena</summary>
        String,
        /// <summary>Valor entero</summary>
        Integer,
        /// <summary>Valor decimal</summary>
        Decimal,
        /// <summary>Valor de tipo fecha</summary>
        Datetime,
        /// <summary>Valor lógico</summary>
        Bool,
        /// <summary>Array de bytes</summary>
        ByteArray,
        /// <summary>Global Id</summary>
        Guid,
        /// <summary>Timespan</summary>
        TimeSpan
    }

    /// <summary>
    ///		Obtiene el valor de un campo de un <see cref="DataRow"/>
    /// </summary>
    public static object? IisNull(this DataRow row, string field, object? defaultValue = null)
    {
        object result = row[field];

            if (result is DBNull)
                return defaultValue;
            else
                return result;
    }

    /// <summary>
    ///		Obtiene el valor de un campo de un <see cref="DataRow"/>
    /// </summary>
    public static TypeData? IisNull<TypeData>(this DataRow row, string field, object? defaultValue = null)
    {
        object result = row[field];

            if (result is DBNull)
                return (TypeData?) defaultValue;
            else
                return (TypeData) result;
    }

    /// <summary>
    ///		Devuelve los tipos de datos recucidos de un <see cref="DataTable"/>
    /// </summary>
    public static List<(string field, FieldType type)> GetFieldTypes(this DataTable table)
    {
        List<(string field, FieldType type)> schema = new List<(string field, FieldType type)>();

            // Recorre las columnas devolviendo los campos
            foreach (DataColumn column in table.Columns)
                schema.Add((column.ColumnName, column.GetFieldType()));
            // Devuelve el esquema
            return schema;
    }

    /// <summary>
    ///		Obtiene el tipo de una columna
    /// </summary>
    public static FieldType GetFieldType(this DataColumn column)
    {
        try
        {
            if (column.DataType == Type.GetType("System.Boolean"))
                return FieldType.Bool;
            else if (column.DataType == Type.GetType("System.Byte") ||
                        column.DataType == Type.GetType("System.Int16") ||
                        column.DataType == Type.GetType("System.Int32") ||
                        column.DataType == Type.GetType("System.Int64") ||
                        column.DataType == Type.GetType("System.UInt16") ||
                        column.DataType == Type.GetType("System.UInt32") ||
                        column.DataType == Type.GetType("System.UInt64") ||
                        column.DataType == Type.GetType("System.SByte"))
                return FieldType.Integer;
            else if (column.DataType == Type.GetType("System.Char") ||
                        column.DataType == Type.GetType("System.String"))
                return FieldType.String;
            else if (column.DataType == Type.GetType("System.DateTime"))
                return FieldType.Datetime;
            else if (column.DataType == Type.GetType("System.Decimal") ||
                        column.DataType == Type.GetType("System.Double") ||
                        column.DataType == Type.GetType("System.Single"))
                return FieldType.Decimal;
            else if (column.DataType == Type.GetType("System.Guid"))
                return FieldType.Guid;
            else if (column.DataType == Type.GetType("System.TimeSpan"))
                return FieldType.TimeSpan;
            else
                return FieldType.Unknown;
        }
        catch
        {
            return FieldType.Unknown;
        }

        //así como el tipo de matriz siguiente:
        //Byte[]
    }
}
