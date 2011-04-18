using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagedFusion.Serialization
{
	public interface IDeserializer : IDeserializerOptions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		object Deserialize(string input);
	}
}
