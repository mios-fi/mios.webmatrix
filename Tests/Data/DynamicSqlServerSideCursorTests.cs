using System.Data;
using System.Data.Common;
using System.Linq;
using Mios.WebMatrix;
using Mios.WebMatrix.Data;
using Xunit;

namespace Tests.Data {
	public class DynamicSqlServerSideCursorTests {
		private readonly DbProviderFactory factory;

		public DynamicSqlServerSideCursorTests() {
			factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
		}

		IDbConnection CreateConnection() {
			var con = factory.CreateConnection();
			con.ConnectionString = @"Server=(local);Database=Test;Trusted_Connection=True";
			return con;
		}

		[Fact]
		public void IsAccessibleByDynamicHelper() {
			using(var con = CreateConnection()) {
				con.Open();
				var cursor = new DynamicSqlServerSideCursor(con, "SELECT * FROM Users ORDER BY [id]", 10, 1);
				var r = cursor.FirstOrDefault<dynamic>();
				Assert.Equal("bob", DynamicHelper.GetValue("name", r));
			}
		}

		[Fact]
		public void CanGetResultByProperty() {
			using(var con = CreateConnection()) {
				con.Open();
				var cursor = new DynamicSqlServerSideCursor(con, "SELECT * FROM Users ORDER BY [id]", 10, 1);
				var r = cursor.ToArray<dynamic>();
				Assert.Equal(1, r[0].Id);
				Assert.Equal("bob", r[0].Name);
				Assert.Equal(2, r[1].Id);
				Assert.Equal("alice", r[1].Name);
				Assert.Equal(3, r[2].Id);
				Assert.Equal("cecil", r[2].Name);
			}
		}
		[Fact]
		public void CanGetResultByIndexes() {
			using(var con = CreateConnection()) {
				con.Open();
				var cursor = new DynamicSqlServerSideCursor(con, "SELECT * FROM Users ORDER BY [id]", 10, 1);
				var r = cursor.ToArray<dynamic>();
				Assert.Equal(1, r[0]["id"]);
				Assert.Equal("bob", r[0]["name"]);
				Assert.Equal(1, r[0][0]);
				Assert.Equal("bob", r[0][1]);
			}
		}

		[Fact]
		public void CanListFields() {
			using(var con = CreateConnection()) {
				con.Open();
				var cursor = new DynamicSqlServerSideCursor(con, "SELECT * FROM Users ORDER BY [id]", 10, 1);
				var r = cursor.ToArray<dynamic>();
				Assert.Equal(new[] { "id", "name" }, r[0].Columns);
			}
		}
		[Fact]
		public void CanUseParameters() {
			using(var con = CreateConnection()) {
				con.Open();
				var cursor = new DynamicSqlServerSideCursor(con, "SELECT * FROM Users WHERE [id]>@0 ORDER BY [id]", 10, 1, 1);
				var r = cursor.ToArray<dynamic>();
				Assert.Equal(2, r[0]["id"]);
				Assert.Equal("alice", r[0]["name"]);
			}
		}
		[Fact]
		public void CanPresentTotalCount() {
			using(var con = CreateConnection()) {
				con.Open();
				var cursor = new DynamicSqlServerSideCursor(con, "SELECT * FROM Users ORDER BY [id]", 2, 1);
				Assert.Equal(7, cursor.TotalCount);
				Assert.Equal(4, cursor.Pages);
			}
		}
		[Fact]
		public void CanPageResults() {
			using(var con = CreateConnection()) {
				con.Open();
				var cursor = new DynamicSqlServerSideCursor(con, "SELECT * FROM Users ORDER BY [id]", 2, 2);
				var r = cursor.ToArray<dynamic>();
				Assert.Equal(2, r.Length);
				Assert.Equal("cecil", r[0].Name);
				Assert.Equal("dave", r[1].Name);
			}
		}
	}
}
