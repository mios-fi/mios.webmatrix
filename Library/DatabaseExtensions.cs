using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebMatrix.Data;

namespace Mios.WebMatrix.Helpers {
	public static class DatabaseExtensions {
		public static IOpenDynamicQuery DynamicQuery(this Database database, string query) {
			return new DynamicQuery(query);
		}
	}
}
