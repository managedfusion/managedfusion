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

		public object Deserialize(string input)
		{
			if (String.IsNullOrEmpty(input))
				return null;

			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return serializer.DeserializeObject(input);
		}

		#endregion
	}
}