using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebMatrix.Data;

namespace Mios.WebMatrix.Data {
	public struct OrderedDynamicQuery : IDynamicQuery {
		private static readonly Regex ColumnNamePattern = new Regex("^[a-z][0-9a-z_]*$");
		private readonly IDynamicQuery query;
		private readonly string orderBy;
		private readonly bool descending;

		public OrderedDynamicQuery(IDynamicQuery query, string orderBy, bool descending) {
			if(!String.IsNullOrEmpty(orderBy) && !ColumnNamePattern.IsMatch(orderBy)) {
				throw new ArgumentOutOfRangeException("Invalid column name '" + orderBy + "'");
			}
			this.query = query;
			this.orderBy = orderBy;
			this.descending = descending;
		}

		public string Query { 
			get {
				if(String.IsNullOrEmpty(orderBy)) return Query;
				return query.Query + " ORDER BY " + orderBy + (descending ? " DESC" : " ASC"); 
			} 
		}
		public IEnumerable<object> Parameters { get { return query.Parameters; } }
		public IEnumerable<dynamic> ExecuteIn(Database db) {
			return db.Query(Query, Parameters.ToArray());
		}
	}
}
