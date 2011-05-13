using System.Collections.Generic;
using WebMatrix.Data;

namespace Mios.WebMatrix.Data {
	public interface IPagedDynamicQuery : IDynamicQuery {
		IPagedDynamicQuery Page(int page);
		new IPagedEnumerable<dynamic> ExecuteIn(Database db);
	}
	public interface IDynamicQuery {
		IEnumerable<dynamic> ExecuteIn(Database db);
		IDynamicQuery Where(string filter, params object[] parameters);
		IDynamicQuery OrderBy(string column, bool descending);
		IDynamicQuery OrderBySimilarity(string column, string parameter);
		IPagedDynamicQuery InPagesOf(int pageSize);
	}
}