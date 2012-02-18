using System;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using Mios.WebMatrix.Data;
using WebMatrix.Data;
using Xunit;

namespace Tests.Data {
	public class DynamicQueryTests : IDisposable {
		private readonly Database db;
		const string dbFile = "QueryTesting.sdf";

		public DynamicQueryTests() {
			if(File.Exists(dbFile)) {
				File.Delete(dbFile);
			}
			new SqlCeEngine("Data Source=" + dbFile).CreateDatabase();
			db = Database.OpenConnectionString("Data Source="+dbFile);
			db.Execute("CREATE TABLE Users ([id] INT IDENTITY(1000,1) PRIMARY KEY, [firstName] NVARCHAR(128), [lastName] NVARCHAR(128))");
			db.Execute("INSERT Users ([firstName],[lastName]) VALUES (@0,@1)", "Alice", "Allison");
			db.Execute("INSERT Users ([firstName],[lastName]) VALUES (@0,@1)", "Bob", "Bobson");
			db.Execute("INSERT Users ([firstName],[lastName]) VALUES (@0,@1)", "Cecil", "Cyrus");
			db.Execute("INSERT Users ([firstName],[lastName]) VALUES (@0,@1)", "Donald", "Donaldson");
		}

		public void Dispose() {
			db.Close();
			if(File.Exists(dbFile)) {
				File.Delete(dbFile);
			}
		}

		[Fact]
		public void Identity() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users");
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT * FROM Users", query.Statement);
			Assert.Equal(0, query.Parameters.Count());
		}

		[Fact]
		public void SingleFilter() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users")
				.Where("[firstName]=@0", "Alice");
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT * FROM Users WHERE [firstName]=@0", query.Statement);
			Assert.Equal("Alice", query.Parameters.ToArray()[0]);
		}
		[Fact]
		public void SingleFilterWithoutValue() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users")
				.Where("[firstName]=@0", null);
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT * FROM Users", query.Statement);
			Assert.Equal(0, query.Parameters.Count());
		}
		[Fact]
		public void MultipleFilters() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users")
				.Where("[lastName]=@0", "Johnsson")
				.Where("[firstName]=@0", "Alice");
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT * FROM Users WHERE [lastName]=@0 AND [firstName]=@1", query.Statement);
			Assert.Equal("Johnsson", query.Parameters.ToArray()[0]);
			Assert.Equal("Alice", query.Parameters.ToArray()[1]);
		}
		[Fact]
		public void OrderBy() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users")
				.OrderBy("[lastName]",false);
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT * FROM Users ORDER BY [lastName] ASC", query.Statement);
		}
		[Fact]
		public void OrderByDescending() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users")		
				.OrderBy("[lastName]", true);
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT * FROM Users ORDER BY [lastName] DESC", query.Statement);
		}
		[Fact]
		public void MultipleOrderBy() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users")
				.OrderBy("[lastName]", false)
				.OrderBy("[firstName]", false);
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT * FROM Users ORDER BY [lastName] ASC, [firstName] ASC", query.Statement);
		}
		[Fact]
		public void FilterWithOrderBy() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users")
				.Where("[firstName]=@0","Bob")
				.OrderBy("[lastName]",false);
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT * FROM Users WHERE [firstName]=@0 ORDER BY [lastName] ASC", query.Statement);
			Assert.Equal("Bob", query.Parameters.ToArray()[0]);
		}
		[Fact]
		public void OrderByEmptyColumn() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users")
				.OrderBy("",false);
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT * FROM Users", query.Statement);
		}
		[Fact]
		public void OrderByDangerousColumnName() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users");
			Assert.Throws<ArgumentOutOfRangeException>(() => dynamicQuery.OrderBy("danger]; DROP TABLE Users",false));
		}
		[Fact]
		public void OrderBySimilarity() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users")
				.OrderBySimilarity("[firstName]", "Alice");
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT * FROM Users ORDER BY (dbo.JaroWinkler(LOWER([firstName]),@0)) DESC", query.Statement);
			Assert.Equal("alice", query.Parameters.ToArray()[0]);
		}
		[Fact]
		public void OrderBySimilarityWithEmptyTerm() {
			IDynamicQuery query;
			query = DynamicQuery.For("SELECT * FROM Users")
				.OrderBySimilarity("firstName", String.Empty);
			Assert.Equal("SELECT * FROM Users", ((DynamicQuery)query).BuildPagedItemQuery().Statement);
			query = DynamicQuery.For("SELECT * FROM Users")
				.OrderBySimilarity("firstName", null);
			Assert.Equal("SELECT * FROM Users", ((DynamicQuery)query).BuildPagedItemQuery().Statement);
		}
		[Fact]
		public void OrderByMultipleSimilarity() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users")
				.OrderBySimilarity("[firstName]", "Alice")
				.OrderBySimilarity("[lastName]","Stone");
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT * FROM Users ORDER BY (dbo.JaroWinkler(LOWER([firstName]),@0)+dbo.JaroWinkler(LOWER([lastName]),@1)) DESC", query.Statement);
			Assert.Equal("alice", query.Parameters.ToArray()[0]);
			Assert.Equal("stone", query.Parameters.ToArray()[1]);
		}
		[Fact]
		public void OrderByMixedSimpleAndSimilarity() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users")
				.OrderBySimilarity("[firstName]", "Alice")
				.OrderBy("lastName",false);
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT * FROM Users ORDER BY (dbo.JaroWinkler(LOWER([firstName]),@0)) DESC, [lastName] ASC", query.Statement);
			Assert.Equal("alice", query.Parameters.ToArray()[0]);
		}


		[Fact]
		public void ParameterizedQueryWithWheres() {
			var dynamicQuery = DynamicQuery.For("SELECT @0 AS [x] FROM Users",1234)
				.Where("[id]=@0",1003);
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT @0 AS [x] FROM Users WHERE [id]=@1", query.Statement);
			Assert.Equal(2, query.Parameters.Count());
			Assert.Equal(1234, query.Parameters[0]);
			Assert.Equal(1003, query.Parameters[1]);
		}

		[Fact]
		public void SelectsAtMostNPlusOnePageSizesForPagedQuery() {
			var dynamicQuery = DynamicQuery.For("SELECT [firstName]+' '+[lastName] AS [name] FROM Users")
				.InPagesOf(20).Page(2);
			var query = ((DynamicQuery)dynamicQuery).BuildPagedItemQuery();
			Assert.Equal("SELECT TOP 60 [firstName]+' '+[lastName] AS [name] FROM Users", query.Statement);
		}

		[Fact]
		public void CanPageOrderedQuery() {
			Assert.Equal(1003,
				DynamicQuery.For("SELECT * FROM Users")
					.OrderBy("id", false)
					.InPagesOf(3).Page(2)
					.ExecuteIn(db).First().Id);
		}


		[Fact]
		public void CanCountMultilineQuery() {
			Assert.Equal(4,
				DynamicQuery.For(
					@"
					SELECT 
					99 AS [id] 
					FROM 
					Users")
					.OrderBy("id", false)
					.InPagesOf(3).Page(2)
					.ExecuteIn(db).TotalCount);
		}

		[Fact]
		public void CanCountNestedQuery() {
			Assert.Equal(4,
				DynamicQuery.For(
					@"SELECT 99 AS [id] FROM ( SELECT * FROM Users ) AS Users")
					.OrderBy("id", false)
					.InPagesOf(3).Page(2)
					.ExecuteIn(db).TotalCount);
		}

		
		[Fact]
		public void CanPageQueryWithoutResults() {
			Assert.Equal(0,
				DynamicQuery.For("SELECT * FROM Users WHERE [firstName]='NOTFOUND'")
					.OrderBy("id",false)
					.InPagesOf(3).Page(1)
					.ExecuteIn(db).TotalCount);
		}

		[Fact]
		public void TotalCountOfUncountableQueryIsNegative() {
			Assert.Equal(-1,
				DynamicQuery.For("SELECT 42")
					.InPagesOf(3).Page(1)
					.ExecuteIn(db).TotalCount);
		}

		[Fact]
		public void ExplicitCountQueryOverridesDerived() {
			var dynamicQuery = DynamicQuery.For("SELECT * FROM Users", 42, "42")
				.CountBy("SELECT 42");
			var query = ((DynamicQuery)dynamicQuery).BuildCountQuery();
			Assert.Equal("SELECT 42", query.Statement);
			var parameters = query.Parameters.ToArray();
			Assert.Equal(42,parameters[0]);
			Assert.Equal("42",parameters[1]);
		}
	}
}
