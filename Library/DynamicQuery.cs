﻿using System.Collections.Generic;
using System.Linq;
using WebMatrix.Data;

namespace Mios.WebMatrix.Helpers {
	public struct DynamicQuery : IOpenDynamicQuery {
		public string Query { get; private set; }
		public IEnumerable<object> Parameters { get; private set; }
		public DynamicQuery(string query, params object[] parameters) : this() {
			Query = query;
			Parameters = parameters;
		}
		public IEnumerable<dynamic> ExecuteIn(Database db) {
			return db.Query(Query, Parameters.ToArray());
		}
	}
}