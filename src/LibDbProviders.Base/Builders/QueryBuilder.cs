using System;

using Bau.Libraries.LibDbProviders.Base.Models;

namespace Bau.Libraries.LibDbProviders.Base.Builders
{
	/// <summary>
	///		Generador de <see cref="QueryModel"/>
	/// </summary>
	public class QueryBuilder
	{
		public QueryBuilder()
		{
			Query = new QueryModel();
		}

		/// <summary>
		///		Asigna la cadena SQL
		/// </summary>
		public QueryBuilder WithSql(string sql, QueryModel.QueryType type)
		{
			// Asigna la cadena SQL
			Query.Sql = sql;
			Query.Type = type;
			// Devuelve el generador
			return this;
		}

		/// <summary>
		///		Asigna el tiempo de espera
		/// </summary>
		public QueryBuilder WithTimeout(TimeSpan timeout)
		{
			// Asigna el tiempo de espera
			if (timeout.TotalSeconds > 0)
				Query.Timeout = timeout;
			// Devuelve el generador
			return this;
		}

		/// <summary>
		///		Asigna la paginación
		/// </summary>
		public QueryBuilder WithPagination(int page, int size)
		{
			// Asigna la paginación
			if (page == 0 || size == 0)
				Query.Pagination.MustPaginate = false;
			else
			{
				Query.Pagination.MustPaginate = true;
				Query.Pagination.Page = page;
				Query.Pagination.PageSize = size;
			}
			// Devuelve el generador
			return this;
		}

        /// <summary>
        ///		Añade un parámetro a la colección de parámetros del comando
        /// </summary>
        public QueryBuilder WithParameter(string name, string value, int length, ParameterDb.ParameterDbDirection direction = ParameterDb.ParameterDbDirection.Input)
        {
            // Añade el parámetro
			Query.Parameters.Add(name, value, length, direction);
			// Devuelve el generador
			return this;
        }

        /// <summary>
        ///		Añade un parámetro a la colección de parámetros del comando
        /// </summary>
        public QueryBuilder WithParameter(string name, object? value, ParameterDb.ParameterDbDirection direction = ParameterDb.ParameterDbDirection.Input)
        {
            // Añade el parámetro
			Query.Parameters.Add(name, value, direction);
			// Devuelve el generador
			return this;
        }

        /// <summary>
        ///		Añade un parámetro a la colección de parámetros del comando
        /// </summary>
        public QueryBuilder WithParameter(string name, byte[] buffer, ParameterDb.ParameterDbDirection direction = ParameterDb.ParameterDbDirection.Input)
        {
            // Añade el parámetro
			Query.Parameters.Add(name, buffer, direction);
			// Devuelve el generador
			return this;
        }

        /// <summary>
        ///		Añade un parámetro de tipo Text
        /// </summary>
        public QueryBuilder WithTextParameter(string name, string value, ParameterDb.ParameterDbDirection direction = ParameterDb.ParameterDbDirection.Input)
        {
            // Añade el parámetro
			Query.Parameters.Add(name, value, direction);
			// Devuelve el generador
			return this;
        }

        /// <summary>
        ///		Añade un parámetro para el código de retorno
        /// </summary>
        public QueryBuilder WithReturnCodeParameter()
        {
            // Añade el parámetro
			Query.Parameters.AddReturnCode();
			// Devuelve el generador
			return this;
        }

		/// <summary>
		///		Añade una serie de parámetros
		/// </summary>
		public QueryBuilder WithParameters(Dictionary<string, object> parameters)
		{
			// Añade los parámetros
			foreach (KeyValuePair<string, object> keyValue in parameters)
				Query.Parameters.Add(keyValue.Key, keyValue.Value);
			// Devuelve el generador
			return this;
		}

		/// <summary>
		///		Añade una colección de parámetros
		/// </summary>
		public QueryBuilder WithParameters(ParametersDbCollection parameters)
		{
			// Añade los parámetros
			Query.Parameters.AddRange(parameters);
			// Devuelve el generador
			return this;
		}

		/// <summary>
		///		Genera la consulta
		/// </summary>
		public QueryModel Build()
		{
			return Query;
		}

		/// <summary>
		///		Datos de la consulta que se está generando
		/// </summary>
		private QueryModel Query { get; }
	}
}
