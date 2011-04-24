using System.Dynamic;
using System.Linq;
using Mios.WebMatrix;
using Xunit;

namespace Tests {
	public class DynamicHelperTests {
		[Fact]
		public void CanListExpandoMembers() {
			dynamic stuff = new ExpandoObject();
			stuff.Name = "Bob";
			stuff.Age = 42;
			var memberNames = DynamicHelper.ListMemberNames((object) stuff);
			Assert.Equal(2, memberNames.Count());
			Assert.Contains("Name", memberNames);
			Assert.Contains("Age", memberNames);
		}
		[Fact]
		public void CanGetDynamicProperty() {
			dynamic stuff = new ExpandoObject();
			stuff.Name = "Bob";
			stuff.Age = 42;
			Assert.Equal("Bob", DynamicHelper.GetValue("Name",stuff));
		}
	}
}
