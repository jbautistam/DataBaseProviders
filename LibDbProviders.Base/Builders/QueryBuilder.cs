using System;
using System.Collections.Generic;

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
		///		Añade un parámetro
		/// </summary>
		public QueryBuilder WithParameter(string key, object value)
		{
			// Añade un parámetro a los argumentos
			Query.Parameters.Add(key, value);
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
