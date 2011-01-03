using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedFusion.Serialization;

namespace System.Collections.Generic
{
	public static class DictionaryExtensions
	{
		public static JsonObject ToSafeJsonObject(this IDictionary<string, object> data)
		{
			return ToJsonObject(data, false);
		}

		public static JsonObject ToJsonObject(this IDictionary<string, object> data, bool throwErrorOnMissingMethod = true)
		{
			return new JsonObject(data, throwErrorOnMissingMethod);
		}
	}
}