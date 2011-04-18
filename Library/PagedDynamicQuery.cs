using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebMatrix.Data;

namespace Mios.WebMatrix.Helpers {
	public struct PagedDynamicQuery : IDynamicQuery {
		private readonly IDynamicQuery query;
		private readonly int pageSize;
		private int selectedPage;

		public PagedDynamicQuery(IDynamicQuery query, int pageSize) : this() {
			this.query = query;
			this.pageSize = pageSize;
		}
		public PagedDynamicQuery Page(int page) {
			selectedPage = page;
			return this;
		}
		public string Query { get { return query.Query; } }
		public IEnumerable<object> Parameters { get { return query.Parameters; } }
		public IEnumerable<dynamic> ExecuteIn(Database db) {
			var countQuery = MakeCountQuery(Query);
			var totalCount = db.QueryValue(countQuery, Parameters.ToArray());
			var result = db.Query(Query, Parameters.ToArray()).Skip(selectedPage*pageSize).Take(pageSize);
			return new PagedEnumerable<dynamic>(result, totalCount, pageSize, selectedPage);
		}

		static string MakeCountQuery(string query) {
			var replacedQuery = FieldSelectionPattern.Replace(query, "SELECT COUNT(*) FROM");
			query = OrderingPattern.Replace(replacedQuery, "");
			return query;
		}
		private static readonly Regex FieldSelectionPattern = new Regex(@"^SELECT.*FROM");
		private static readonly Regex OrderingPattern = new Regex(@"ORDER BY.*$");
	}
}
