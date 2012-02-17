using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using Mios.WebMatrix.Data;

namespace Tests.Data {
	internal class CursorEnumerable : IPagedEnumerable<object> {
		private readonly IDbConnection con;
		private readonly string query;
		private readonly object[] parameters;

		private int? count;

		public CursorEnumerable(IDbConnection con, string query, int pageSize, int page, params object[] parameters) {
			PageSize = pageSize;
			Page = Math.Max(1,page);
			this.con = con;
			this.query = query;
			this.parameters = parameters;
		}

		public IEnumerator<dynamic> GetEnumerator() {
			EnsureCursorCreated();
			string[] fields = null;
			var skip = (Page-1)*PageSize;
			var cmd = con.CreateCommand();
			while(true) {
				if(skip>0) {
					cmd.CommandText = "FETCH RELATIVE "+(skip+1)+" FROM c SELECT @@FETCH_STATUS";
				} else {
					cmd.CommandText = "FETCH NEXT FROM c SELECT @@FETCH_STATUS";
				}
				using(var reader = cmd.ExecuteReader()) {
					if(!reader.Read()) break;
					if(fields==null) {
						fields = new string[reader.FieldCount-1];
						for(var i=0; i<fields.Length; i++) {
							fields[i] = reader.GetName(i);
						}
					}
					var values = new object[fields.Length];
					reader.GetValues(values);
					yield return new Result(fields, values);
				}
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public int TotalCount {
			get {
				EnsureCursorCreated();
				return count ?? 0;
			}
		}

		public int PageSize {
			get; protected set;
		}

		public int Page {
			get; protected set;
		}

		public int Pages {
			get {
				return (int)Math.Ceiling(TotalCount/(decimal)PageSize);
			}
		}

		private void EnsureCursorCreated() {
			if(count!=null) return;

			if(con.State != ConnectionState.Closed) {
				con.Close();
			}
			con.Open();

			var cmd = con.CreateCommand();
			cmd.CommandText = "DECLARE c SCROLL CURSOR FOR " + query + " OPEN	c SELECT @@CURSOR_ROWS";
			for(var i = 0; i < parameters.Length; i++) {
				var param = cmd.CreateParameter();
				param.Value = parameters[i];
				param.ParameterName = "@" + i;
				cmd.Parameters.Add(param);
			}
			count = (int) cmd.ExecuteScalar();
		}

		public class Result : DynamicObject {
			private readonly string[] keys;
			private readonly object[] values;
			public Result(string[] keys, object[] values) {
				this.keys = keys;
				this.values = values;
			}
			public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
				if(indexes.Length!=1) {
					result = null;
					return false;
				}
				var stringIndex = indexes[0] as string;
				if(stringIndex!=null) {
					return TryGetMember(stringIndex, out result);
				}
				if(indexes[0] is int) {
					return TryGetIndex((int)indexes[0], out result);
				}
				result = null;
				return false;
			}
			public override bool TryGetMember(GetMemberBinder binder, out object result) {
				if(binder.Name=="Columns") {
					result = keys;
					return true;
				}
				var member = binder.Name.ToLowerInvariant();
				return TryGetMember(member, out result);
			}

			private bool TryGetMember(string member, out object result) {
				var index = Array.IndexOf(keys, member);
				if(index<0) {
					result = null;
					return false;
				}
				result = values[index];
				return true;
			}
			private bool TryGetIndex(int index, out object result) {
				if(index<0 || index>values.Length-1) {
					result = null;
					return false;
				}
				result = values[index];
				return true;
			}
		}
	}
}