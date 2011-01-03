using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Collections;

namespace ManagedFusion.Serialization
{
	public class JsonObject : DynamicObject
	{
		private IDictionary<string, object> _model;
		private bool _throwErrorOnMissingMethod;

		internal JsonObject(IDictionary<string, object> model, bool throwErrorOnMissingMethod = true)
		{
			_model = new Dictionary<string, object>(model, StringComparer.OrdinalIgnoreCase);
			_throwErrorOnMissingMethod = throwErrorOnMissingMethod;
		}

		public IDictionary<string, object> Model { get { return _model; } }

		private static string GetKey(object[] indexes)
		{
			if (indexes.Length == 1)
				return (string)indexes[0];

			return null;
		}

		private bool TryGetValue(string name, out object result)
		{
			result = null;

			if (String.IsNullOrEmpty(name))
				return true;

			return _model.TryGetValue(name, out result);
		}

		private object WrapObjectIfNessisary(object result)
		{
			// handle special types in model object
			if (result is IDictionary<string, object>)
				result = new JsonObject((IDictionary<string, object>)result);
			else if (result is ICollection)
			{
				var itemList = new List<object>();
				foreach (var item2 in (ICollection)result)
					itemList.Add(WrapObjectIfNessisary(item2));

				if (itemList.Count > 0)
					result = itemList;
			}

			return result;
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return _model.Keys;
		}

		public override bool TryConvert(ConvertBinder binder, out object result)
		{
			result = null;

			if (!binder.Type.IsAssignableFrom(_model.GetType()))
				throw new InvalidOperationException(String.Format(@"Unable to convert to ""{0}"".", binder.Type));

			result = _model;
			return true;
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			var key = GetKey(indexes);

			if (!TryGetValue(key, out result) && _throwErrorOnMissingMethod)
				throw new MissingMemberException(String.Format(@"Member ""{0}"" was not found in the body of the JSON posted.", key));

			result = WrapObjectIfNessisary(result);

			return true;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (!TryGetValue(binder.Name, out result) && _throwErrorOnMissingMethod)
				throw new MissingMemberException(String.Format(@"Member ""{0}"" was not found in the body of the JSON posted.", binder.Name));

			result = WrapObjectIfNessisary(result);

			return true;
		}

		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
		{
			var key = GetKey(indexes);

			if (!String.IsNullOrEmpty(key))
				_model[key] = WrapObjectIfNessisary(value);

			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			_model[binder.Name] = WrapObjectIfNessisary(value);
			return true;
		}
	}
}