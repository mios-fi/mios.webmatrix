using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WebMatrix.Data;

namespace Mios.WebMatrix.Data {
	public struct SimilarityOrderedDynamicQuery : IDynamicQuery {
		private static readonly Regex ColumnNamePattern;
		static SimilarityOrderedDynamicQuery() {
			ColumnNamePattern = new Regex("^([a-z][0-9a-z_]*|\\[[a-z][0-9a-z_]*\\])?$", RegexOptions.IgnoreCase|RegexOptions.Compiled);
		}

		private readonly IDynamicQuery wrapped;
		private readonly IDictionary<string, string> fields;

		public string Method { get; set; }

		public SimilarityOrderedDynamicQuery(IDynamicQuery wrapped)
			: this() {
			Method = "dbo.JaroWinkler";
			fields = new Dictionary<string, string>();
			this.wrapped = wrapped;
		}
		public SimilarityOrderedDynamicQuery To(string column, string term) {
			if(column == null) throw new ArgumentNullException("column");
			if(!ColumnNamePattern.IsMatch(column)) {
				throw new ArgumentOutOfRangeException("column","Invalid column name '" + column + "'");
			}
			if(!String.IsNullOrEmpty(term)) {
				fields[column] = term;
			}
			return this;
		}

		public string Query { 
			get {
				if(fields.Count==0) {
					return wrapped.Query;
				}
				var i = 0;
				return wrapped.Query + 
					" ORDER BY (" + 
					String.Join("+",fields.Select(t=>String.Format("dbo.JaroWinkler([{0}],@{1})", t.Key, i++)).ToArray()) + 
					") DESC"; 
			} 
		}
		public IEnumerable<object> Parameters { 
			get { 
				return fields.Count==0
					?	wrapped.Parameters
					: wrapped.Parameters.Concat(fields.Select(t=>t.Value)); 
			} 
		}
		public IEnumerable<dynamic> ExecuteIn(Database db) {
			return db.Query(Query, Parameters.ToArray());
		}
	}
}