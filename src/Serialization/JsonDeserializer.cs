using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace ManagedFusion.Serialization
{
	public class JsonDeserializer : IDeserializer
	{
		#region IDeserializer Members

		public IDictionary<string, object> Deserialize(string input)
		{
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return serializer.Deserialize<Dictionary<string, object>>(input);
		}

		#endregion
	}
}