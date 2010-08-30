using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ManagedFusion.Serialization
{
	/// <summary>
	/// Exception raised when <see cref="JsonParser" /> encounters an invalid token.
	/// </summary>
	public class InvalidJsonException : Exception
	{
		public InvalidJsonException(string message, IList<char> data, int index)
			: base(message)
		{
			Data = data;
			Index = index;
		}

		public IList<char> Data { get; private set; }
		public int Index { get; private set; }

		public override string Message
		{
			get
			{
				return String.Format("{0} \"{1}\"", base.Message, new String(Data.Skip(Index).Take(30).ToArray()));
			}
		}
	}
}
