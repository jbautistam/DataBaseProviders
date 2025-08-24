namespace Bau.Libraries.LibDbProviders.Base.Models;

/// <summary>
///		Clase que almacena los parámetros de un comando
/// </summary>
public class ParametersDbCollection : List<ParameterDb>
{
    // Constantes privadas
    private const string ReturnCodeName = "@ReturnCode";

    /// <summary>
    ///		Añade un parámetro a la colección de parámetros del comando
    /// </summary>
    public void Add(string name, string value, int length, ParameterDb.ParameterDbDirection direction = ParameterDb.ParameterDbDirection.Input)
    {
        // Corta la cadena para que se ajuste a la colección
        if (!string.IsNullOrEmpty(value) && value.Length > length)
            value = value.Substring(0, length);
        // Añade el parámetro
        Add(new ParameterDb(name, value, direction, length));
    }

    /// <summary>
    ///		Añade un parámetro a la colección de parámetros del comando
    /// </summary>
    public void Add(string name, object? value, ParameterDb.ParameterDbDirection direction = ParameterDb.ParameterDbDirection.Input)
    {
        Add(new ParameterDb(name, value, direction));
    }

    /// <summary>
    ///		Añade un parámetro a la colección de parámetros del comando
    /// </summary>
    public void Add(string name, byte[] buffer, ParameterDb.ParameterDbDirection direction = ParameterDb.ParameterDbDirection.Input)
    {
        Add(new ParameterDb(name, buffer, direction));
    }

    /// <summary>
    ///		Añade un parámetro de tipo Text
    /// </summary>
    public void AddText(string name, string value, ParameterDb.ParameterDbDirection direction = ParameterDb.ParameterDbDirection.Input)
    {
        Add(new ParameterDb(name, value, direction)
                                {
                                    IsText = true
                                }
            );
    }

    /// <summary>
    ///		Añade un parámetro para el código de retorno
    /// </summary>
    public void AddReturnCode()
    {
        Add(ReturnCodeName, 0, ParameterDb.ParameterDbDirection.ReturnValue);
    }

    /// <summary>
    ///		Obtiene un parámetro de la colección a partir del nombre, si no lo encuentra devuelve un parámetro vacío
    /// </summary>
    public ParameterDb Search(string name)
    {
        // Recorre la colección de parámetros buscando el elemento adecuado
        foreach (ParameterDb parameter in this)
            if (parameter.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                return parameter;
        // Devuelve un objeto vacío
        return new ParameterDb(name, null, ParameterDb.ParameterDbDirection.Input);
    }

    /// <summary>
    ///		Obtiene el código de retorno
    /// </summary>
    public int? GetReturnCode()
    {
        ParameterDb parameterDB = Search(ReturnCodeName);

            if (parameterDB.Value is null || !(parameterDB.Value is int))
                return null;
            else
                return (int) parameterDB.Value;
    }

    /// <summary>
    ///		Obtiene el valor de un parámetro o un valor predeterminado si es null
    /// </summary>
    public object? IisNull(string name)
    {
        ParameterDb parameterDB = Search(name);

            if (parameterDB.Value == DBNull.Value)
                return null;
            else
                return parameterDB.Value;
    }

    /// <summary>
    ///		Indizador de la colección por el nombre de parámetro
    /// </summary>
    public ParameterDb this[string name] => Search(name);
}