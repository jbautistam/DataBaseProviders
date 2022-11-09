using System;

namespace Bau.Libraries.LibDbProviders.Base.Models
{
    /// <summary>
    ///		Clase con los datos de un par�metro
    /// </summary>
    public class ParameterDb
    {
        /// <summary>
        ///		Direcci�n del par�metro
        /// </summary>
        public enum ParameterDbDirection
        {
            /// <summary>Par�metro de entrada</summary>
            Input,
            /// <summary>Par�metro de salida</summary>
            Output,
            /// <summary>Par�metro de entrada / salida</summary>
            InputOutput,
            /// <summary>Valor de retorno</summary>
            ReturnValue
        }

        public ParameterDb(string name, object? value, ParameterDbDirection direction, int length = 0)
        {
            // Asigna el nombre
            Name = name;
            if (!string.IsNullOrWhiteSpace(Name))
                Name = Name.Trim();
            // Asigna el valor
            if (value is Enum)
                Value = (int) value;
            else if (value is DBNull)
                Value = null;
            else
                Value = value;
            // Asigna el resto de propiedades
            Direction = direction;
            Length = length;
        }

        /// <summary>
        ///		Obtiene un valor de tipo objeto o nulo para la base de datos
        /// </summary>
        public object GetDBValue()
        {
            if (Value == null)
                return DBNull.Value;
            else if (Value is Enum)
                return (int)Value;
            else
                return Value;
        }

        /// <summary>
        ///		Nombre del par�metro
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///		Valor del par�metro
        /// </summary>
        public object? Value { get; }

        /// <summary>
        ///		Direcci�n del par�metro
        /// </summary>
        public ParameterDbDirection Direction { get; } = ParameterDbDirection.Input;

        /// <summary>
        ///		Longitud
        /// </summary>
        public int Length { get; }

        /// <summary>
        ///		Indica si es un par�metro de texto
        /// </summary>
        public bool IsText { get; set; }
    }
}