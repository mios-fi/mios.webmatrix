using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace Mios.WebMatrix {
	internal static class DynamicHelper {
		public static IEnumerable<string> ListMemberNames(object obj) {
			var provider = obj as IDynamicMetaObjectProvider;
			if(provider==null) return Enumerable.Empty<string>();
			DynamicMetaObject metaObject = provider.GetMetaObject(Expression.Constant(obj));
			return metaObject.GetDynamicMemberNames();
		}
		public static object GetValue(string name, object obj) {
			var binder = Binder.GetMember(CSharpBinderFlags.None, name, typeof(DynamicHelper), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
			var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
			return callsite.Target(callsite, obj);
		}
	}
}