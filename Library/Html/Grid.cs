using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Mios.WebMatrix.Html {
	public class Grid {
		private readonly IEnumerable<dynamic> source;
		public Grid(IEnumerable<dynamic> source) {
			this.source = source;
		}

		private dynamic[] expandedResults;
		private dynamic[] ExpandedResults {
			get {
				if(expandedResults == null) {
					expandedResults = source.ToArray();
				}
				return expandedResults;
			}
		}

		public HtmlString GetHtml(string tableClass = null, object columns = null) {
			var table = new TagBuilder("table");
			table.AddCssClass(tableClass);
			table.InnerHtml = CreateHeader() + CreateBody();
			return new HtmlString(table.ToString());
		}

		private string CreateBody() {
			var tbody = new TagBuilder("tbody");
			var sb = new StringBuilder();
			var rows = ExpandedResults;
			for(var i=0; i<rows.Length; i++) {
				CreateRow(rows[i],sb);
			}
			tbody.InnerHtml = sb.ToString();
			return tbody.ToString();
		}

		private void CreateRow(dynamic row, StringBuilder sb) {
			sb.Append("<tr>");
			sb.Append("</tr>");
		}

		private string CreateHeader() {
			if(ExpandedResults.Length <= 0) return String.Empty;
			object firstRow = expandedResults[0];
			var headers = DynamicHelper.ListMemberNames(firstRow);
			var thead = new TagBuilder("thead");
			thead.InnerHtml = String.Join("",headers.Select(CreateHeaderColumn).ToArray());
			return thead.ToString();
		}
		private string CreateHeaderColumn(string t) {
			var col = new TagBuilder("th");
			col.SetInnerText(t);
			return col.ToString();
		}

		class Column {
			public string Name { get; set; }
			public Func<string, dynamic> Header { get; set; }
			public Func<string, dynamic> Cell { get; set; }
		}
	}
}
