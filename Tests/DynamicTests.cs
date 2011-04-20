using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace Tests {
	public class DynamicTests {
		[Fact]
		public void CanListDynamicMembers() {
			dynamic stuff = new ExpandoObject();
			stuff.Name = "Bob";
			stuff.Age = 42;
			var provider = stuff as IDynamicMetaObjectProvider;
			Assert.NotNull(provider);
			DynamicMetaObject metaObject = provider.GetMetaObject(Expression.Constant(stuff));
			Assert.NotNull(metaObject);
			var memberNames = metaObject.GetDynamicMemberNames();
			Assert.Equal(2, memberNames.Count());
			Assert.Contains("Name", memberNames);
			Assert.Contains("Age", memberNames);
		}
	}
}
