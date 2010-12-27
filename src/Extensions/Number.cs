using System;
using System.Text;

namespace System
{
	public static class NumberExtensions
	{
		private const long Base62 = 62;
		private static readonly char[] Index = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

		public static string ToBase62(this int i)
		{
			var sb = new StringBuilder();
			ToBase62(i, sb);

			return sb.ToString();
		}

		public static string ToBase62(this long l)
		{
			var sb = new StringBuilder();
			ToBase62(l, sb);

			return sb.ToString();
		}

		private static void ToBase62(long l, StringBuilder sb)
		{
			long radix = l % Base62;

			if (l - radix != 0)
				ToBase62((l - radix) / Base62, sb);

			sb.Append(Index[radix]);
		}
	}
}
