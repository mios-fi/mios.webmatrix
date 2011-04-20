using System.Collections.Generic;
using System.Linq;
using WebMatrix.Data;

namespace Mios.WebMatrix.Helpers.Data {
	public struct OrderedDynamicQuery : IDynamicQuery {
		private readonly IDynamicQuery query;
		private readonly string orderBy;
		private readonly bool descending;

		public OrderedDynamicQuery(IDynamicQuery query, string orderBy, bool descending) {
			this.query = query;
			this.orderBy = orderBy;
			this.descending = descending;
		}

		public string Query { 
			get { return query.Query + " ORDER BY " + orderBy + (descending ? " DESC" : " ASC"); } 
		}
		public IEnumerable<object> Parameters { get { return query.Parameters; } }
		public IEnumerable<dynamic> ExecuteIn(Database db) {
			return db.Query(Query, Parameters.ToArray());
		}
	}
}
