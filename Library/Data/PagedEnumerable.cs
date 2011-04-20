using System;
using System.Collections.Generic;

namespace Mios.WebMatrix.Helpers.Data {
	public struct PagedEnumerable<T> : IEnumerable<T> {
		private readonly IEnumerable<T> enumerable;
		public int TotalCount { get; private set; }
		public int PageSize { get; set; }
		public int Page { get; private set; }
		public int Pages { get; private set; }
		public PagedEnumerable(IEnumerable<T> enumerable, int totalCount, int pageSize, int page) : this() {
			TotalCount = totalCount;
			PageSize = pageSize;
			Page = page;
			Pages = (int)Math.Ceiling(totalCount/(decimal)pageSize);
			this.enumerable = enumerable;
		}
		public IEnumerator<T> GetEnumerator() {
			return enumerable.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}