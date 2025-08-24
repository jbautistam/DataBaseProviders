namespace Bau.Libraries.LibDbProviders.Base.Models;

/// <summary>
///		Clase que almacena los par�metros de un comando
/// </summary>
public class ParametersDbCollection : List<ParameterDb>
{
    // Constantes privadas
    private const string ReturnCodeName = "@ReturnCode";

    /// <summary>
    ///		A�ade un par�metro a la colecci�n de par�metros del comando
    /// </summary>
    public void Add(string name, string value, int length, ParameterDb.ParameterDbDirection direction = ParameterDb.ParameterDbDirection.Input)
    {
        // Corta la cadena para que se ajuste a la colecci�n
        if (!string.IsNullOrEmpty(value) && value.Length > length)
            value = value.Substring(0, length);
        // A�ade el par�metro
        Add(new ParameterDb(name, value, direction, length));
    }

    /// <summary>
    ///		A�ade un par�metro a la colecci�n de par�metros del comando
    /// </summary>
    public void Add(string name, object? value, ParameterDb.ParameterDbDirection direction = ParameterDb.ParameterDbDirection.Input)
    {
        Add(new ParameterDb(name, value, direction));
    }

    /// <summary>
    ///		A�ade un par�metro a la colecci�n de par�metros del comando
    /// </summary>
    public void Add(string name, byte[] buffer, ParameterDb.ParameterDbDirection direction = ParameterDb.ParameterDbDirection.Input)
    {
        Add(new ParameterDb(name, buffer, direction));
    }

    /// <summary>
    ///		A�ade un par�metro de tipo Text
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
    ///		A�ade un par�metro para el c�digo de retorno
    /// </summary>
    public void AddReturnCode()
    {
        Add(ReturnCodeName, 0, ParameterDb.ParameterDbDirection.ReturnValue);
    }

    /// <summary>
    ///		Obtiene un par�metro de la colecci�n a partir del nombre, si no lo encuentra devuelve un par�metro vac�o
    /// </summary>
    public ParameterDb Search(string name)
    {
        // Recorre la colecci�n de par�metros buscando el elemento adecuado
        foreach (ParameterDb parameter in this)
            if (parameter.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                return parameter;
        // Devuelve un objeto vac�o
        return new ParameterDb(name, null, ParameterDb.ParameterDbDirection.Input);
    }

    /// <summary>
    ///		Obtiene el c�digo de retorno
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
    ///		Obtiene el valor de un par�metro o un valor predeterminado si es null
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
    ///		Indizador de la colecci�n por el nombre de par�metro
    /// </summary>
    public ParameterDb this[string name] => Search(name);
}