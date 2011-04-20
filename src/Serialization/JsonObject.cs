using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Collections;
using System.Reflection;

namespace ManagedFusion.Serialization
{
	public class JsonObject : DynamicObject, IModelSerializer
	{
		private IDictionary<string, object> _model;
		private bool _throwErrorOnMissingMethod;

		#region Static Methods

		public static dynamic Parse(string json, bool throwErrorOnMissingMethod = true, StringComparison methodComparisonType = StringComparison.Ordinal)
		{
			return new JsonObject(json, throwErrorOnMissingMethod, methodComparisonType);
		}

		public static dynamic Parse(object obj, bool throwErrorOnMissingMethod = true, StringComparison methodComparisonType = StringComparison.Ordinal)
		{
			return new JsonObject(obj, throwErrorOnMissingMethod, methodComparisonType);
		}

		#endregion

		internal JsonObject(string json, bool throwErrorOnMissingMethod = true, StringComparison methodComparisonType = StringComparison.Ordinal)
			: this(json.FromJson(), throwErrorOnMissingMethod, methodComparisonType) { }

		internal JsonObject(object obj, bool throwErrorOnMissingMethod = true, StringComparison methodComparisonType = StringComparison.Ordinal)
			: this(obj.ToDictionary(), throwErrorOnMissingMethod, methodComparisonType) { }

		internal JsonObject(IDictionary<string, object> model, bool throwErrorOnMissingMethod = true, StringComparison methodComparisonType = StringComparison.Ordinal)
		{
			_model = new Dictionary<string, object>(model, GetStringComparer(methodComparisonType));
			_throwErrorOnMissingMethod = throwErrorOnMissingMethod;
		}

		private StringComparer GetStringComparer(StringComparison comparisonType)
		{
			switch (comparisonType)
			{
				case StringComparison.CurrentCulture:
					return StringComparer.CurrentCulture;

				case StringComparison.CurrentCultureIgnoreCase:
					return StringComparer.CurrentCultureIgnoreCase;

				case StringComparison.InvariantCulture:
					return StringComparer.InvariantCulture;

				case StringComparison.InvariantCultureIgnoreCase:
					return StringComparer.InvariantCultureIgnoreCase;

				case StringComparison.Ordinal:
					return StringComparer.Ordinal;

				case StringComparison.OrdinalIgnoreCase:
					return StringComparer.OrdinalIgnoreCase;

				default:
					throw new ArgumentException(comparisonType + " is not a support method comparison type.", "methodComparisonType");
			}
		}

		private static string GetSingleIndexOrNull(object[] indexes)
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
			else if (result is ICollection || result is Array)
			{
				var itemList = new List<object>();
				foreach (var item2 in (IEnumerable)result)
					itemList.Add(WrapObjectIfNessisary(item2));

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
			var key = GetSingleIndexOrNull(indexes);

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

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			result = _model.GetType().InvokeMember(binder.Name, BindingFlags.InvokeMethod, null, _model, args);
			result = WrapObjectIfNessisary(result);

			return true;
		}

		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
		{
			var key = GetSingleIndexOrNull(indexes);

			if (!String.IsNullOrEmpty(key))
				_model[key] = WrapObjectIfNessisary(value);

			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			_model[binder.Name] = WrapObjectIfNessisary(value);
			return true;
		}

		public override string ToString()
		{
			return _model.ToJson();
		}

		#region IModelSerializer Members

		IDictionary<string, object> IModelSerializer.GetSerializedModel()
		{
			return _model;
		}

		#endregion
	}
}