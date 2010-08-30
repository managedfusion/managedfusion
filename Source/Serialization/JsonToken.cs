using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ManagedFusion.Serialization
{
	/// <summary>
	/// Possible JSON tokens in parsed input.
	/// </summary>
	public enum JsonToken
	{
		Unknown,
		LeftBrace,
		RightBrace,
		Colon,
		Comma,
		LeftBracket,
		RightBracket,
		String,
		Number,
		True,
		False,
		Null
	}
}
