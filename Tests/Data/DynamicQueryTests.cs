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
		public void CanExecuteQuery() {
			var query = new DynamicQuery("SELECT * FROM Users");
			Assert.True(query.ExecuteIn(db).Any(t => t.firstName=="Alice"));
		}
		[Fact]
		public void CanExecuteFilteredQuery() {
			var query = new DynamicQuery("SELECT * FROM Users")
				.Where("[firstName]=@0", "Alice");
			Assert.Equal(1, query.ExecuteIn(db).Count());
		}
		[Fact]
		public void DoNotApplyFilterWithNullArgument() {
			var query = new DynamicQuery("SELECT * FROM Users")
				.Where("[firstName]=@0", null);
			Assert.True(query.ExecuteIn(db).Count()>1);
		}
		[Fact]
		public void CanExecuteMultipleFilteredQueries() {
			var query = new DynamicQuery("SELECT * FROM Users")
				.Where("[lastName]=@0", "Johnsson")
				.Where("[firstName]=@0", "Alice");
			Assert.Equal(0, query.ExecuteIn(db).Count());
		}
		[Fact]
		public void CanOrderQuery() {
			Assert.Equal("Bobson",
				new DynamicQuery("SELECT * FROM Users")
					.OrderBy("[lastName]")
					.ExecuteIn(db).First().LastName);
		}
		[Fact]
		public void CanOrderFilteredQuery() {
			Assert.Equal("Bobson",
				new DynamicQuery("SELECT * FROM Users")
					.Where("[firstName]=@0","Bob")
					.OrderBy("[lastName]")
					.ExecuteIn(db).First().LastName);
		}
		[Fact]
		public void CanPageOrderedQuery() {
			Assert.Equal(1003,
				new DynamicQuery("SELECT * FROM Users")
					.OrderBy("[id]")
					.InPagesOf(3).Page(2)
					.ExecuteIn(db).First().Id);
		}
	}
}
