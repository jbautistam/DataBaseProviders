using System;
using System.Collections.Generic;
using System.Data;

using Bau.Libraries.LibDbProviders.Base.Parameters;

namespace Bau.Libraries.LibDbProviders.Base.RepositoryData
{
	/// <summary>
	///		Objeto base para los objetos que cumplan la interface <see cref="IRepositoryData{TypeData}"/>
	/// </summary>
	public abstract class RepositoryDataBase<TypeData> : IRepositoryData<TypeData>, IDisposable
	{
		protected RepositoryDataBase(IDbProvider connection)
		{
			Connection = connection;
		}

		/// <summary>
		///		Carga los datos de una colección
		/// </summary>
		public List<TypeData> LoadCollection(string text, CommandType commandType, AssignDataCallBack callBack)
		{
			return LoadCollection(text, null, commandType, callBack);
		}

		/// <summary>
		///		Carga una colección utilizando genéricos para un procedimiento con un único 
		/// parámetro de entrada alfanumérico
		/// </summary>
		public List<TypeData> LoadCollection(string text, string Parameter, string parameterValue, int parameterLength,
											 CommandType commandType, AssignDataCallBack callBack)
		{
			return LoadCollection(text, GetParameters(Parameter, parameterValue, parameterLength),
								  commandType, callBack);
		}

		/// <summary>
		///		Carga una colección utilizando genéricos para un procedimiento con un único 
		/// parámetro de entrada numérico
		/// </summary>
		public List<TypeData> LoadCollection(string text, string Parameter, int? parameterValue,
											 CommandType commandType, AssignDataCallBack callBack)
		{
			return LoadCollection(text, GetParameters(Parameter, parameterValue), commandType, callBack);
		}

		/// <summary>
		///		Carga una colección utilizando genéricos
		/// </summary>
		public List<TypeData> LoadCollection(string text, ParametersDbCollection parameters,
											 CommandType commandType, AssignDataCallBack callBack)
		{
			List<TypeData> results = new List<TypeData>();

				// Abre la conexión
				Connection.Open();
				// Carga los datos
				using (IDataReader data = Connection.ExecuteReader(text, parameters, commandType))
				{ 
					// Lee los datos
					while (data.Read())
						results.Add((TypeData)callBack(data));
				}
				// Cierra la conexión
				Connection.Close();
				// Devuelve la lista
				return results;
		}

		/// <summary>
		///		Carga una colección paginada utilizando genéricos
		/// </summary>
		public List<TypeData> LoadCollection(string text, ParametersDbCollection parametersDB,
											 CommandType commandType, AssignDataCallBack callBack, int page, int recordsPerPage)
		{
			List<TypeData> results = new List<TypeData>();

				// Abre la conexión
				Connection.Open();
				// Carga los datos
				using (IDataReader rdoData = Connection.ExecuteReader(text, parametersDB, commandType, page, recordsPerPage))
				{ 
					while (rdoData.Read())
						results.Add((TypeData) callBack(rdoData));
				}
				// Cierra la conexión
				Connection.Close();
				// Devuelve la lista
				return results;
		}

		/// <summary>
		///		Carga un objeto utilizando genéricos para un procedimiento con un único parámetro alfanumérico
		/// </summary>
		public TypeData LoadObject(string text, string Parameter, string parameterValue, int parameterLength,
								   CommandType commandType, AssignDataCallBack callBack)
		{
			ParametersDbCollection parametersDB = new ParametersDbCollection();

				// Asigna los parámetros
				parametersDB.Add(Parameter, parameterValue, parameterLength);
				// Carga los datos
				return LoadObject(text, parametersDB, commandType, callBack);
		}

		/// <summary>
		///		Carga un objeto utilizando genéricos para un procedimiento con un único parámetro numérico
		/// </summary>
		public TypeData LoadObject(string text, string Parameter, int? parameterValue,
								   CommandType commandType, AssignDataCallBack callBack)
		{
			ParametersDbCollection parametersDB = new ParametersDbCollection();

				// Asigna los parámetros
				parametersDB.Add(Parameter, parameterValue);
				// Carga los datos
				return LoadObject(text, parametersDB, commandType, callBack);
		}

		/// <summary>
		///		Carga un objeto utilizando genéricos
		/// </summary>
		public TypeData LoadObject(string text, ParametersDbCollection parametersDB,
								   CommandType commandType, AssignDataCallBack callBack)
		{
			TypeData data;

				// Abre la conexión
				Connection.Open();
				// Lee los datos
				using (IDataReader reader = Connection.ExecuteReader(text, parametersDB, commandType))
				{ 
					// Lee los datos
					if (reader.Read())
						data = (TypeData) callBack(reader);
					else
						data = (TypeData) callBack(null);
					// Cierra el recordset
					reader.Close();
				}
				// Cierra la conexión
				Connection.Close();
				// Devuelve el objeto
				return data;
		}

		/// <summary>
		///		Carga un objeto utilizando genéricos para un procedimiento con un único parámetro alfanumérico
		/// </summary>
		public int Execute(string text, string Parameter, string parameterValue, int parameterLength,
						   CommandType commandType = CommandType.Text)
		{
			return Execute(text, GetParameters(Parameter, parameterValue, parameterLength), commandType);
		}

		/// <summary>
		///		Carga un objeto utilizando genéricos para un procedimiento con un único parámetro numérico
		/// </summary>
		public int Execute(string text, string Parameter, int? parameterValue, CommandType commandType = CommandType.Text)
		{
			return Execute(text, GetParameters(Parameter, parameterValue), commandType);
		}

		/// <summary>
		///		Ejecuta una sentencia sobre la conexión
		/// </summary>
		public int Execute(string text, ParametersDbCollection parametersDB, CommandType commandType = CommandType.Text)
		{
			int rows;

				// Abre la conexión
				Connection.Open();
				// Ejecuta sobre la conexión
				rows = Connection.Execute(text, parametersDB, commandType);
				// Cierra la conexión
				Connection.Close();
				// Devuelve el número de registros afectados
				return rows;
		}

		/// <summary>
		///		Ejecuta una sentencia sobre la conexión y devuelve un escalar
		/// </summary>
		public object ExecuteScalar(string text, string Parameter, string parameterValue,
									int parameterLength, CommandType commandType = CommandType.Text)
		{
			return ExecuteScalar(text, GetParameters(Parameter, parameterValue, parameterLength), commandType);
		}

		/// <summary>
		///		Ejecuta una sentencia sobre la conexión y devuelve un escalar
		/// </summary>
		public object ExecuteScalar(string text, string Parameter, int? parameterValue, CommandType commandType = CommandType.Text)
		{
			return ExecuteScalar(text, GetParameters(Parameter, parameterValue), commandType);
		}

		/// <summary>
		///		Ejecuta una sentencia sobre la conexión y devuelve un escalar
		/// </summary>
		public object ExecuteScalar(string text, ParametersDbCollection parametersDB, CommandType commandType = CommandType.Text)
		{
			object value;

				// Abre la conexión
				Connection.Open();
				// Ejecuta sobre la conexión
				value = Connection.ExecuteScalar(text, parametersDB, commandType);
				// Cierra la conexión
				Connection.Close();
				// Devuelve el resultado
				return value;
		}

		/// <summary>
		///		Ejecuta sobre una conexión para obtener una identidad
		/// </summary>
		public int? ExecuteGetIdentity(string text, ParametersDbCollection parametersDB, CommandType commandType = CommandType.Text)
		{
			int? intIdentity;

				// Abre la conexión
				Connection.Open();
				// Ejecuta sobre la conexión
				Connection.Execute(text, parametersDB, commandType);
				// Obtiene el valor identidad
				intIdentity = GetIdentityValue(parametersDB);
				// Cierra la conexión
				Connection.Close();
				// Devuelve el valor identidad
				return intIdentity;
		}

		/// <summary>
		///		Obtiene un valor identidad resultante del último INSERT realizado
		/// </summary>
		protected abstract int? GetIdentityValue(ParametersDbCollection parametersDB);

		/// <summary>
		///		Obtiene una colección de parámetros con un único parámetro de tipo cadena
		/// </summary>
		private ParametersDbCollection GetParameters(string Parameter, string parameterValue, int length)
		{
			ParametersDbCollection parametersDB = new ParametersDbCollection();

				// Asigna los parámetros
				parametersDB.Add(Parameter, parameterValue, length);
				// Devuelve los parámetros
				return parametersDB;
		}

		/// <summary>
		///		Obtiene una colección de parámetros con un único parámetro de tipo entero
		/// </summary>
		private ParametersDbCollection GetParameters(string Parameter, int? parameterValue)
		{
			ParametersDbCollection parametersDB = new ParametersDbCollection();

				// Asigna los parámetros
				parametersDB.Add(Parameter, parameterValue);
				// Devuelve los parámetros
				return parametersDB;
		}

		/// <summary>
		///		Libera la conexión
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				// Libera la conexión
				if (disposing)
					Connection?.Dispose();
				// Indica que se ha liberado
				Disposed = true;
			}
		}

		/// <summary>
		///		Destruye la conexión
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		///		Conexión a la base de datos
		/// </summary>
		public IDbProvider Connection { get; }

		/// <summary>
		///		Indica si se ha liberado la conexión
		/// </summary>
		public bool Disposed { get; private set; }
	}
}
