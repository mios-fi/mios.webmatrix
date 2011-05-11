using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebMatrix.Data;

namespace Mios.WebMatrix.Data {
	public struct OrderedDynamicQuery : IDynamicQuery {
		private static readonly Regex ColumnNamePattern;
		static OrderedDynamicQuery() {
			ColumnNamePattern = new Regex("^([a-z][0-9a-z_]*|\\[[a-z][0-9a-z_]*\\])?$", RegexOptions.IgnoreCase);
		}

		private readonly IDynamicQuery wrapped;
		private readonly string orderBy;
		private readonly bool descending;

		public OrderedDynamicQuery(IDynamicQuery wrapped, string orderBy, bool descending) {
			if(orderBy == null) throw new ArgumentNullException("orderBy");
			if(!ColumnNamePattern.IsMatch(orderBy)) {
				throw new ArgumentOutOfRangeException("orderBy","Invalid column name '" + orderBy + "'");
			}
			this.wrapped = wrapped;
			this.orderBy = orderBy.TrimStart('[').TrimEnd(']');
			this.descending = descending;
		}

		public string Query { 
			get {
				if(String.IsNullOrEmpty(orderBy)) return wrapped.Query;
				return wrapped.Query + " ORDER BY " + orderBy + (descending ? " DESC" : " ASC"); 
			} 
		}
		public IEnumerable<object> Parameters { get { return wrapped.Parameters; } }
		public IEnumerable<dynamic> ExecuteIn(Database db) {
			return db.Query(Query, Parameters.ToArray());
		}
	}
}
