using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mios.WebMatrix.Data {
  public static class StaticPagedEnumerableExtensions {
    public static StaticPagedEnumerable<T> Realize<T>(this IPagedEnumerable<T> pagedEnumerable) {
      return new StaticPagedEnumerable<T>(pagedEnumerable);
    }
  }

  public class StaticPagedEnumerable<T> : IPagedEnumerable<T> {
		private readonly IEnumerable<T> buffer;
		public StaticPagedEnumerable(IPagedEnumerable<T> wrapped) {
			buffer = wrapped.ToArray();
			Pages = wrapped.Pages;
			PageSize = wrapped.PageSize;
			Page = wrapped.Page;
			TotalCount = wrapped.TotalCount;
		}
		public int Pages { get; private set; }
		public int PageSize { get; private set; }
		public int Page { get; private set; }
		public int TotalCount { get; private set; }
		public IEnumerator<T> GetEnumerator() {
			return buffer.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() { 
			return GetEnumerator();
		}
	}
}