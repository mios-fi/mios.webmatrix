using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using WebMatrix.Data;

namespace Mios.WebMatrix.Data {
	public class DynamicQuery : IPagedDynamicQuery {
		private readonly string baseQuery;
		private readonly object[] parameters;
		private int page;
		private int pageSize = 10;
		private readonly IList<WhereClause> whereClauses = new List<WhereClause>();
		private readonly IList<OrderingClause> orderByClauses = new List<OrderingClause>();
		private readonly IList<SimilarityOrderingClause> similarityOrderClauses = new List<SimilarityOrderingClause>();

		protected DynamicQuery(string baseQuery, params object[] parameters) {
			this.baseQuery = baseQuery;
			this.parameters = parameters;
		}

		public IPagedDynamicQuery Page(int page) {
			this.page = page;
			return this;
		}

		public IPagedDynamicQuery InPagesOf(int pageSize) {
			this.pageSize = pageSize;
			return this;
		}

		public IDynamicQuery Where(string expression, params object[] parameters) {
			whereClauses.Add(new WhereClause { 
				Expression = expression,
				Parameters = parameters ?? new object[] {null}
			});
			return this;
		}

		public IDynamicQuery OrderBy(string column, bool descending) {
			if(String.IsNullOrEmpty(column)) {
				return this;
			}
			if(!SafeColumnPattern.IsMatch(column)) {
				throw new ArgumentOutOfRangeException("column","Unsafe column name '"+column+"'");
			}
			column = column.TrimStart('[').TrimEnd(']');
			orderByClauses.Add(new OrderingClause { 
				Expression = "["+column+"]", 
				Parameters = new object[0],
				Descending = descending 
			});
			return this;
		}

		public IDynamicQuery OrderBySimilarity(string expression, string parameter, bool ignoreCase, string method) {
			similarityOrderClauses.Add(new SimilarityOrderingClause {
				Expression = expression,
				Parameter = parameter,
				IgnoreCase = ignoreCase,
				Method = method
			});
			return this;
		}


		IPagedEnumerable<dynamic> IPagedDynamicQuery.ExecuteIn(Database db) {
			int count;
			if(FieldSelectionPattern.IsMatch(baseQuery)) {
				var countQuery = BuildCountQuery();
				try {
					count = db.QueryValue(countQuery.Statement, countQuery.Parameters);
				} catch(Exception e) {
					throw new DynamicQueryException("Exception when executing COUNT() query '" + countQuery.Statement + "'", e);
				}
			} else {
				count = -1;
			}
			var items = ExecuteIn(db).Skip(Math.Max(0, page - 1)*pageSize).Take(pageSize);
			return new PagedEnumerable<dynamic>(items, count, pageSize, page);
		}

		public IEnumerable<dynamic> ExecuteIn(Database db) {
			var query = BuildItemQuery();
			try {
				return db.Query(query.Statement, query.Parameters);
			} catch(Exception e) {
				throw new DynamicQueryException("Exception when executing query '" + query.Statement + "'", e);
			}
		}

		public static IDynamicQuery For(string baseQuery, params object[] parameters) {
			return new DynamicQuery(baseQuery,parameters);
		}

		public struct Query {
			public string Statement;
			public object[] Parameters;
		}

		public Query BuildCountQuery() {
			var parameters = new List<object>(this.parameters);
			var expression = new StringBuilder();
			expression.Append(FieldSelectionPattern.Replace(baseQuery, "SELECT COUNT(*) FROM"));
			AddWhereClauses(expression, parameters);
			return new Query {
				Statement = expression.ToString(),
				Parameters = parameters.ToArray()
			};
		}

		public Query BuildItemQuery() {
			var parameters = new List<object>(this.parameters);
			var expression = new StringBuilder();
			expression.Append(baseQuery);
			AddWhereClauses(expression, parameters);
			AddOrderByClauses(expression, parameters);
			return new Query {
				Statement = expression.ToString(),
				Parameters = parameters.ToArray()
			};
		}

		private void AddWhereClauses(StringBuilder sb, List<object> parameters) {
			var clauses = whereClauses.Where(t => t.Parameters==null || t.Parameters.All(IsNonEmptyParameter));
			if(!clauses.Any()) return;
			sb.Append(" WHERE ");
			var separator = "";
			foreach(var clause in clauses) {
				sb.Append(separator + ReindexParameters(clause.Expression, parameters.Count));
				parameters.AddRange(clause.Parameters ?? new object[0]);
				separator = " AND ";
			}
		}

		private void AddOrderByClauses(StringBuilder sb, List<object> parameters) {
			var similarityClauses = similarityOrderClauses
				.Where(t => IsNonEmptyParameter(t.Parameter));
			var simpleClauses = orderByClauses
				.Where(t => !String.IsNullOrEmpty(t.Expression) && t.Parameters.All(IsNonEmptyParameter));

			var hasSimilarityClauses = similarityClauses.Any();
			var hasSimpleClauses = simpleClauses.Any();
			if(!(hasSimilarityClauses||hasSimpleClauses)) return;

			sb.Append(" ORDER BY ");

			if(hasSimilarityClauses) {
				sb.Append("(");
				var similaritySeparator = "";
				foreach(var clause in similarityClauses) {
					sb.Append(similaritySeparator);
					if(clause.IgnoreCase) {
						sb.AppendFormat("{0}(LOWER({1}),@{2})", clause.Method, clause.Expression, parameters.Count);
						parameters.Add(clause.Parameter.ToLower());
					} else {
						sb.AppendFormat("{0}({1},@{2})", clause.Method, clause.Expression, parameters.Count);
						parameters.Add(clause.Parameter);
					}
					similaritySeparator = "+";
				}
				sb.Append(") DESC");
			}

			if(hasSimpleClauses) {
				if(hasSimilarityClauses) sb.Append(", ");
				var simpleSeparator = "";
				foreach(var clause in simpleClauses) {
					sb.Append(simpleSeparator);
					sb.Append(clause.Expression + (clause.Descending ? " DESC" : " ASC"));
					parameters.AddRange(clause.Parameters);
					simpleSeparator = ", ";
				}
			}
		}

		static bool IsNonEmptyParameter(object parameter) {
			return parameter!=null && !(parameter is string && String.IsNullOrEmpty(parameter as string));
		}

		static private readonly Regex FieldSelectionPattern 
			= new Regex(@"^\s*SELECT.*?FROM", RegexOptions.Singleline|RegexOptions.IgnoreCase|RegexOptions.Compiled);
		static private readonly Regex ParameterPattern 
			= new Regex(@"@\d+");
		static private readonly Regex SafeColumnPattern 
			= new Regex(@"^([a-z][a-z0-9_]*|\[[a-z][a-z0-9_]*\])$", RegexOptions.Compiled|RegexOptions.IgnoreCase);

		static string ReindexParameters(string expression, int startIndex) {
			return ParameterPattern.Replace(expression, match => {
				var i = int.Parse(match.Value.Substring(1))+startIndex;
				return "@"+i;
			});
		}

		struct OrderingClause {
			public string Expression;
			public bool Descending;
			public object[] Parameters;
		}
		struct SimilarityOrderingClause {
			public string Expression;
			public string Parameter;
			public bool IgnoreCase;
			public string Method;
		}

		struct WhereClause {
			public string Expression;
			public object[] Parameters;
		}
	}

	[Serializable]
	public class DynamicQueryException : Exception {
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public DynamicQueryException() {
		}

		public DynamicQueryException(string message) : base(message) {
		}

		public DynamicQueryException(string message, Exception inner) : base(message, inner) {
		}

		protected DynamicQueryException(
			SerializationInfo info,
			StreamingContext context) : base(info, context) {
		}
	}
}