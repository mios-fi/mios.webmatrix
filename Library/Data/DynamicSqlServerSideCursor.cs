using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Linq;

namespace Mios.WebMatrix.Data {
	public class ServerSideSqlCursor : IPagedEnumerable<ServerSideSqlCursor.ResultRow> {
		private readonly IDbConnection con;
		private readonly string query;
		private readonly object[] parameters;

		private int? count;

		public ServerSideSqlCursor(IDbConnection con, string query, int pageSize, int page, params object[] parameters) {
			PageSize = pageSize;
			Page = Math.Max(1,page);
			this.con = con;
			this.query = query;
			this.parameters = parameters;
		}

		public IEnumerator<ResultRow> GetEnumerator() {
			EnsureCursorCreated();
			string[] fields = null;
			var skip = (Page-1)*PageSize;
			var take = PageSize;
			var cmd = con.CreateCommand();
			while(take-->0) {
				if(skip>0) {
					cmd.CommandText = "FETCH ABSOLUTE "+(skip+1)+" FROM c SELECT @@FETCH_STATUS";
					skip = 0;
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
					yield return new ResultRow(fields, values);
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

		public class ResultRow : DynamicObject, ICustomTypeDescriptor {
			private readonly string[] keys;
			private readonly object[] values;
			public ResultRow(string[] keys, object[] values) {
				this.keys = keys;
				this.values = values;
			}

			public object this[string index] {
				get {
					object value;
					TryGetMember(index, out value);
					return value;
				}
			}
			public object this[int index] {
				get {
					object value;
					TryGetIndex(index, out value);
					return value;
				}
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

			public bool TryGetMember(string member, out object result) {
				var index = Array.IndexOf(keys, member);
				if(index<0) {
					result = null;
					return false;
				}
				result = values[index];
				return true;
			}
			public bool TryGetIndex(int index, out object result) {
				if(index<0 || index>values.Length-1) {
					result = null;
					return false;
				}
				result = values[index];
				return true;
			}

			public AttributeCollection GetAttributes() {
				return new AttributeCollection();
			}
			public string GetClassName() {
				return null;
			}
			public string GetComponentName() {
				return null;
			}
			public TypeConverter GetConverter() {
				return null;
			}
			public EventDescriptor GetDefaultEvent() {
				return null;
			}
			public PropertyDescriptor GetDefaultProperty() {
				return null;
			}
			public object GetEditor(Type editorBaseType) {
				return null;
			}
			public EventDescriptorCollection GetEvents() {
				return EventDescriptorCollection.Empty;
			}
			public EventDescriptorCollection GetEvents(Attribute[] attributes) {
				return EventDescriptorCollection.Empty;
			}
			public PropertyDescriptorCollection GetProperties() {
				var descriptors = keys.Select((t, i) => new DynamicPropertyDescriptor(t, values[i].GetType()));
				return new PropertyDescriptorCollection(descriptors.OfType<PropertyDescriptor>().ToArray(), true);
			}
			public PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
				return GetProperties();
			}

			public object GetPropertyOwner(PropertyDescriptor pd) {
				return this;
			}

			private class DynamicPropertyDescriptor : PropertyDescriptor {
				private static readonly Attribute[] Empty = new Attribute[0];
				private readonly string name;
				private readonly Type type;

				public DynamicPropertyDescriptor(string name, Type type)
					: base(name, Empty) {
					this.name = name;
					this.type = type;
				}

				public override bool CanResetValue(object component) {
					return false;
				}
				public override object GetValue(object component) {
					var result = component as ResultRow;
					if(result==null) {
						throw new ArgumentException("component");
					}
					return result[name];
				}
				public override void ResetValue(object component) {
					throw new InvalidOperationException();
				}
				public override void SetValue(object component, object value) {
					throw new InvalidOperationException();
				}
				public override bool ShouldSerializeValue(object component) {
					return false;
				}
				public override Type ComponentType {
					get { return typeof(ResultRow); }
				}
				public override bool IsReadOnly {
					get { return true; }
				}
				public override Type PropertyType {
					get { return type; }
				}
			}
		}
	}
}