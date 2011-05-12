using System;
using System.Linq;
using Mios.WebMatrix.Data;
using WebMatrix.Data;
using Xunit;

namespace Tests {
	public class DynamicQueryTests {
		private readonly Database db;

		public DynamicQueryTests() {
			db = Database.OpenConnectionString("Data Source=QueryTesting.sdf");
		}

		[Fact]
		public void Identity() {
			var query = new DynamicQuery("SELECT * FROM Users");
			Assert.Equal("SELECT * FROM Users", query.Query);
			Assert.Equal(0, query.Parameters.Count());
		}
		[Fact]
		public void SingleFilter() {
			var query = new DynamicQuery("SELECT * FROM Users")
				.Where("[firstName]=@0", "Alice");
			Assert.Equal("SELECT * FROM Users WHERE [firstName]=@0", query.Query);
			Assert.Equal("Alice", query.Parameters.ToArray()[0]);
		}
		[Fact]
		public void SingleFilterWithoutValue() {
			var query = new DynamicQuery("SELECT * FROM Users")
				.Where("[firstName]=@0", null);
			Assert.Equal("SELECT * FROM Users", query.Query);
			Assert.Equal(0, query.Parameters.Count());
		}
		[Fact]
		public void MultipleFilters() {
			var query = new DynamicQuery("SELECT * FROM Users")
				.Where("[lastName]=@0", "Johnsson")
				.Where("[firstName]=@0", "Alice");
			Assert.Equal("SELECT * FROM Users WHERE [lastName]=@0 AND [firstName]=@1", query.Query);
			Assert.Equal("Johnsson", query.Parameters.ToArray()[0]);
			Assert.Equal("Alice", query.Parameters.ToArray()[1]);
		}
		[Fact]
		public void OrderBy() {
			var query =	new DynamicQuery("SELECT * FROM Users")
				.OrderBy("lastName");
			Assert.Equal("SELECT * FROM Users ORDER BY [lastName] ASC", query.Query);
		}
		[Fact]
		public void OrderByDescending() {
			var query =	new DynamicQuery("SELECT * FROM Users")		
				.OrderBy("lastName", descending: true);
			Assert.Equal("SELECT * FROM Users ORDER BY [lastName] DESC", query.Query);
		}
		[Fact]
		public void FilterWithOrderBy() {
			var query = new DynamicQuery("SELECT * FROM Users")
				.Where("[firstName]=@0","Bob")
				.OrderBy("lastName");
			Assert.Equal("SELECT * FROM Users WHERE [firstName]=@0 ORDER BY [lastName] ASC", query.Query);
			Assert.Equal("Bob", query.Parameters.ToArray()[0]);
		}
		[Fact]
		public void OrderByEmptyColumn() {
			var query = new DynamicQuery("SELECT * FROM Users")
				.OrderBy("");
			Assert.Equal("SELECT * FROM Users", query.Query);
		}
		[Fact]
		public void OrderByDangerousColumnName() {
			var query = new DynamicQuery("SELECT * FROM Users");
			Assert.Throws<ArgumentOutOfRangeException>(() => query.OrderBy("danger]; DROP TABLE Users"));
		}
		[Fact]
		public void OrderBySimilarity() {
			var query = new DynamicQuery("SELECT * FROM Users")
				.OrderBySimilarity().To("firstName", "Alice");
			Assert.Equal("SELECT * FROM Users ORDER BY (dbo.JaroWinkler([firstName],@0)) DESC", query.Query);
			Assert.Equal("Alice", query.Parameters.ToArray()[0]);
		}
		[Fact]
		public void OrderBySimilarityWithEmptyTerm() {
			IDynamicQuery query;
			query = new DynamicQuery("SELECT * FROM Users")
				.OrderBySimilarity().To("firstName", String.Empty);
			Assert.Equal("SELECT * FROM Users", query.Query);
			query = new DynamicQuery("SELECT * FROM Users")
				.OrderBySimilarity().To("firstName", null);
			Assert.Equal("SELECT * FROM Users", query.Query);
		}
		[Fact]
		public void OrderByMultipleSimilarity() {
			var query = new DynamicQuery("SELECT * FROM Users")
				.OrderBySimilarity()
					.To("firstName", "Alice")
					.To("lastName","Stone");
			Assert.Equal("SELECT * FROM Users ORDER BY (dbo.JaroWinkler([firstName],@0)+dbo.JaroWinkler([lastName],@1)) DESC", query.Query);
			Assert.Equal("Alice", query.Parameters.ToArray()[0]);
			Assert.Equal("Stone", query.Parameters.ToArray()[1]);
		}

		[Fact]
		public void CanPageOrderedQuery() {
			Assert.Equal(1003,
				new DynamicQuery("SELECT * FROM Users")
					.OrderBy("id")
					.InPagesOf(3).Page(2)
					.ExecuteIn(db).First().Id);
		}
		[Fact]
		public void CanPageQueryWithoutResults() {
			Assert.Equal(0,
				new DynamicQuery("SELECT * FROM Users WHERE [firstName]='NOTFOUND'")
					.OrderBy("id")
					.InPagesOf(3).Page(1)
					.ExecuteIn(db).TotalCount);
		}
	}
}
