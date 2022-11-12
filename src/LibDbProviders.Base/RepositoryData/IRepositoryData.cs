using System;
using System.Data;
using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.Base.RepositoryData
{
    // Delegados públicos de este espacio de nombres
    public delegate object AssignDataCallBack(IDataReader? data);

	/// <summary>
	///		Interface para los objetos de repositorio
	/// </summary>
	public interface IRepositoryData<TypeData>
	{ 
		/// <summary>
		///		Carga los datos de una colección
		/// </summary>
		List<TypeData> LoadCollection(string text, CommandType commandType, AssignDataCallBack callBack);

		/// <summary>
		///		Carga una colección utilizando genéricos para un procedimiento con un único 
		/// parámetro de entrada alfanumérico
		/// </summary>
		List<TypeData> LoadCollection(string text, string Parameter, string parameterValue, int parameterLength, 
									  CommandType commandType, AssignDataCallBack callBack);
	
		/// <summary>
		///		Carga una colección utilizando genéricos para un procedimiento con un único 
		/// parámetro de entrada numérico
		/// </summary>
		List<TypeData> LoadCollection(string text, string Parameter, int? parameterValue,
									  CommandType commandType, AssignDataCallBack callBack);
		
		/// <summary>
		///		Carga una colección utilizando genéricos
		/// </summary>
		List<TypeData> LoadCollection(string text, ParametersDbCollection parameters,
									  CommandType commandType, AssignDataCallBack callBack);
		
		/// <summary>
		///		Carga un objeto utilizando genéricos para un procedimiento con un único parámetro alfanumérico
		/// </summary>
		TypeData LoadObject(string text, string Parameter, string parameterValue, int parameterLength, 
							CommandType commandType, AssignDataCallBack callBack);
		
		/// <summary>
		///		Carga un objeto utilizando genéricos para un procedimiento con un único parámetro numérico
		/// </summary>
		TypeData LoadObject(string text, string Parameter, int? parameterValue,
							CommandType commandType, AssignDataCallBack callBack);
		
		/// <summary>
		///		Carga un objeto utilizando genéricos
		/// </summary>
		TypeData LoadObject(string text, ParametersDbCollection parametersDB, 
							CommandType commandType, AssignDataCallBack callBack);
		
		/// <summary>
		///		Ejecuta una sentencia sobre la conexión
		/// </summary>
		int Execute(string text, string Parameter, string parameterValue, int parameterLength, CommandType commandType);
		
		/// <summary>
		///		Ejecuta una sentencia sobre la conexión
		/// </summary>
		int Execute(string text, string Parameter, int? parameterValue, CommandType commandType);
		
		/// <summary>
		///		Ejecuta una sentencia sobre la conexión
		/// </summary>
		int Execute(string text, ParametersDbCollection parametersDB, CommandType commandType);
		
		/// <summary>
		///		Ejecuta una sentencia sobre la conexión y devuelve un escalar
		/// </summary>
		object? ExecuteScalar(string text, string Parameter, string parameterValue, int parameterLength, CommandType commandType);
		
		/// <summary>
		///		Ejecuta una sentencia sobre la conexión y devuelve un escalar
		/// </summary>
		object? ExecuteScalar(string text, string Parameter, int? parameterValue, CommandType commandType);
		
		/// <summary>
		///		Ejecuta una sentencia sobre la conexión y devuelve un escalar
		/// </summary>
		object? ExecuteScalar(string text, ParametersDbCollection parametersDB, CommandType commandType);		

		/// <summary>
		///		Graba los datos de un objeto
		/// </summary>
		int? ExecuteGetIdentity(string text, ParametersDbCollection parametersDB, CommandType commandType);

		/// <summary>
		///		Conexión con la que trabaja el repository
		/// </summary>
		IDbProvider Connection { get; }
	}
}