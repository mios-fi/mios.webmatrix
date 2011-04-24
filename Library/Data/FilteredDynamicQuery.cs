using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebMatrix.Data;

namespace Mios.WebMatrix.Data {
	public struct FilteredDynamicQuery : IOpenDynamicQuery {
		private readonly IDynamicQuery query;
		private readonly string filter;
		private readonly object[] parameters;

		public string Query {
			get {
				var reindexedFilter = ReindexParameters(filter, query.Parameters.Count());
				if(query is FilteredDynamicQuery) {
					return query.Query + " AND "+reindexedFilter;
				}
				return query.Query + " WHERE "+reindexedFilter;
			}
		}
		public IEnumerable<object> Parameters {
			get { return query.Parameters.Concat(parameters); }
		}
		public FilteredDynamicQuery(IOpenDynamicQuery query, string filter, params object[] parameters) {
			this.query = query;
			this.filter = filter;
			this.parameters = parameters;
		}
		public IEnumerable<object> ExecuteIn(Database db) {
			return db.Query(Query, Parameters.ToArray());
		}
		static private readonly Regex ParameterPattern = new Regex(@"@\d+");
		static string ReindexParameters(string filter, int startIndex) {
			return ParameterPattern.Replace(filter,match=>{
				var i = int.Parse(match.Value.Substring(1))+startIndex++;
				return "@"+i;
			});
		}
	}
}