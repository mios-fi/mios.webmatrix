using System.Linq;

namespace Mios.WebMatrix.Data {
	public static class DynamicQueryExtensions {
		public static IOpenDynamicQuery Where(this IOpenDynamicQuery query, string filter, params object[] parameters) {
			if(parameters.Any(t => t==null)) return query;
			return new FilteredDynamicQuery(query, filter, parameters);
		}
		public static OrderedDynamicQuery OrderBy(this IOpenDynamicQuery query, string orderBy) {
			return new OrderedDynamicQuery(query, orderBy, false);
		}
		public static OrderedDynamicQuery OrderBy(this IOpenDynamicQuery query, string orderBy, bool descending) {
			return new OrderedDynamicQuery(query, orderBy, descending);
		}
		public static OrderedDynamicQuery OrderByDesc(this IOpenDynamicQuery query, string orderBy) {
			return new OrderedDynamicQuery(query, orderBy, true);
		}
		public static PagedDynamicQuery InPagesOf(this OrderedDynamicQuery query, int pageSize) {
			return new PagedDynamicQuery(query, pageSize);
		}
	}
}
