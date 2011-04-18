using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mios.WebMatrix.Helpers {
	public static class DynamicQueryExtensions {
		public static FilteredDynamicQuery Where(this IOpenDynamicQuery query, string filter, params object[] parameters) {
			return new FilteredDynamicQuery(query, filter, parameters);
		}
		public static OrderedDynamicQuery OrderBy(this IOpenDynamicQuery query, string orderBy) {
			return new OrderedDynamicQuery(query, orderBy, false);
		}
		public static OrderedDynamicQuery OrderByDesc(this IOpenDynamicQuery query, string orderBy) {
			return new OrderedDynamicQuery(query, orderBy, true);
		}
		public static PagedDynamicQuery InPagesOf(this OrderedDynamicQuery query, int pageSize) {
			return new PagedDynamicQuery(query, pageSize);
		}
	}
}
