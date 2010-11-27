using System;
using System.Collections.Generic;

namespace ManagedFusion.Serialization
{
	public interface IModelSerializer
	{
		IDictionary<string, object> GetSerializedModel();
	}
}
