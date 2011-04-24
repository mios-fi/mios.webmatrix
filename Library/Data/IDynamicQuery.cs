using System.Collections.Generic;
using WebMatrix.Data;

namespace Mios.WebMatrix.Data {
	public interface IOpenDynamicQuery : IDynamicQuery {
	}

	public interface IDynamicQuery {
		string Query { get; }
		IEnumerable<object> Parameters { get; }
		IEnumerable<dynamic> ExecuteIn(Database db);
	}
}