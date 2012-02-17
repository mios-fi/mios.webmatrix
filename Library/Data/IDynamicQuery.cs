using System.Collections.Generic;
using WebMatrix.Data;

namespace Mios.WebMatrix.Data {
	public interface IPagedDynamicQuery : IDynamicQuery {
		IPagedDynamicQuery Page(int page);
		new IPagedEnumerable<dynamic> ExecuteIn(Database db);
		IPagedEnumerable<dynamic> ExecuteCursorIn(Database db);
	}
	public interface IDynamicQuery {
		IEnumerable<dynamic> ExecuteIn(Database db);
		IDynamicQuery CountBy(string countQuery);
		IDynamicQuery Where(string filter, params object[] parameters);
		IDynamicQuery OrderBy(string column, bool descending);
		IDynamicQuery OrderByDescending(string column);
		IDynamicQuery OrderBySimilarity(string column, string parameter, bool ignoreCase = true, string method = "dbo.JaroWinkler");
		IPagedDynamicQuery InPagesOf(int pageSize);
	}
}