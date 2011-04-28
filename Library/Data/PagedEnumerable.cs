using System;
using System.Collections;
using System.Collections.Generic;

namespace Mios.WebMatrix.Data {
	public interface IPagedEnumerable : IEnumerable {
		int TotalCount { get; }
		int PageSize { get; }
		int Page { get; }
		int Pages { get; }
	}
	public interface IPagedEnumerable<out T> : IPagedEnumerable, IEnumerable<T> {
		new IEnumerator<T> GetEnumerator();
	}
	public struct PagedEnumerable<T> : IPagedEnumerable<T> {
		private readonly IEnumerable<T> enumerable;
		public int TotalCount { get; private set; }
		public int PageSize { get; private set; }
		public int Page { get; private set; }
		public int Pages { get; private set; }
		public PagedEnumerable(IEnumerable<T> enumerable, int totalCount, int pageSize, int page) : this() {
			Pages = (int)Math.Ceiling(totalCount/(decimal)pageSize);
			if(page<1 || page>Pages) {
				throw new ArgumentOutOfRangeException("page");
			}
			TotalCount = totalCount;
			PageSize = pageSize;
			Page = page;
			this.enumerable = enumerable;
		}
		public IEnumerator<T> GetEnumerator() {
			return enumerable.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}