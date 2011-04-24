using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Mios.WebMatrix.Html;
using Xunit;

namespace Tests.Html {
	public class GridTests {
		[Fact]
		public void AppliesClassToTable() {
			Assert.Contains("<table class=\"className\">", new Grid(Enumerable.Empty<dynamic>()).GetHtml(tableClass: "className").ToString());
		}
		[Fact]
		public void CreatesHeaderForEachProperty() {
			dynamic obj = new ExpandoObject();
			obj.Name = "Bob";
			obj.Id = 1;
			var html = new Grid(new [] { obj }).GetHtml().ToString();
			Assert.Contains("<th>Name</th>", html);
			Assert.Contains("<th>Id</th>", html);
		}
	}
}
