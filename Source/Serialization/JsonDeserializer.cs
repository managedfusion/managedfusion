using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ManagedFusion.Serialization
{
	/// <seealso cref="http://json.org" />
	/// <seealso href="http://dimebrain.com/2010/04/how-to-parse-json.html"/>
	public class JsonDeserializer : IDeserializer
	{
		private const NumberStyles JsonNumberStyle = NumberStyles.Float;

		#region IDeserializer Members

		public IDictionary<string, object> Deserialize(string input)
		{
			var data = input.ToCharArray();
			var index = 0;

			return ParseObject(data, ref index);
		}

		#endregion

		private IDictionary<string, object> ParseMembers(IList<char> data, ref int index)
		{
			var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			while (index < data.Count)
			{
				var token = NextToken(data, ref index);
				switch (token)
				{
					case JsonToken.String:
						var pair = ParsePair(data, ref index);
						result.Add(pair.Key, pair.Value);
						break;

					case JsonToken.Comma:
						index++;
						break;

					case JsonToken.RightBrace:	// End Object
						return result;

					default:
						throw new InvalidJsonException(string.Format(
							"Invalid JSON found while parsing an object at index {0}.", index
							), data, index);
				}
			}

			throw new InvalidJsonException(String.Format(
				"Invalid JSON found while parsing an array at index {0}.", index
				), data, index);
		}

		private IDictionary<string, object> ParseObject(IList<char> data, ref int index)
		{
			IDictionary<string, object> result = new Dictionary<string, object>();

			index++; // Skip first brace
			while (index < data.Count)
			{
				var token = NextToken(data, ref index);
				switch (token)
				{
					case JsonToken.RightBrace:
						index++;
						return result;

					case JsonToken.String:
						result = ParseMembers(data, ref index);
						break;

					default:
						throw new InvalidJsonException(string.Format(
							"Invalid JSON found while parsing an object at index {0}.", index
							), data, index);
				}
			}

			throw new InvalidJsonException(String.Format(
				"Invalid JSON found while parsing an array at index {0}.", index
				), data, index);
		}

		private IEnumerable<object> ParseElements(IList<char> data, ref int index)
		{
			var result = new List<object>();

			while (index < data.Count)
			{
				var token = NextToken(data, ref index);
				switch (token)
				{
					case JsonToken.LeftBrace:           // Start Object
					case JsonToken.LeftBracket:         // Start Array
					case JsonToken.String:
					case JsonToken.Number:
					case JsonToken.True:
					case JsonToken.False:
					case JsonToken.Null:
						var value = ParseValue(data, ref index);
						result.Add(value);
						break;

					case JsonToken.Comma:
						index++;
						break;

					case JsonToken.RightBracket:
						return result;

					default:
						throw new InvalidJsonException(string.Format(
							"Invalid JSON found while parsing an object at index {0}.", index
							), data, index);
				}
			}

			throw new InvalidJsonException(String.Format(
				"Invalid JSON found while parsing an array at index {0}.", index
				), data, index);
		}

		private IEnumerable<object> ParseArray(IList<char> data, ref int index)
		{
			IEnumerable<object> result = new List<object>();

			index++; // Skip first bracket
			while (index < data.Count)
			{
				var token = NextToken(data, ref index);
				switch (token)
				{
					case JsonToken.RightBracket:        // End Array
						index++;
						return result;

					case JsonToken.LeftBrace:           // Start Object
					case JsonToken.LeftBracket:         // Start Array
					case JsonToken.String:
					case JsonToken.Number:
					case JsonToken.True:
					case JsonToken.False:
					case JsonToken.Null:
						result = ParseElements(data, ref index);
						break;

					default:
						throw new InvalidJsonException(String.Format(
							"Invalid JSON found while parsing an array at index {0}.", index
							), data, index);
				}
			}

			throw new InvalidJsonException(String.Format(
				"Invalid JSON found while parsing an array at index {0}.", index
				), data, index);
		}

		private string ParseString(IList<char> data, ref int index)
		{
			var symbol = data[index];
			IgnoreWhitespace(data, ref index, symbol);
			symbol = data[++index]; // Skip first quotation

			var sb = new StringBuilder();
			while (true)
			{
				if (index >= data.Count - 1)
				{
					return null;
				}
				switch (symbol)
				{
					case '"':  // End String
						index++;
						return sb.ToString();
					case '\\': // Control Character
						symbol = data[++index];
						switch (symbol)
						{
							case '\\':
							case '/':
							case 'b':
							case 'f':
							case 'n':
							case 'r':
							case 't':
								sb.Append('\\').Append(symbol);
								break;
							case 'u': // Unicode literals
								if (index < data.Count - 5)
								{
									var array = data.ToArray();
									var buffer = new char[4];
									Array.Copy(array, index + 1, buffer, 0, 4);

									// http://msdn.microsoft.com/en-us/library/aa664669%28VS.71%29.aspx
									// http://www.yoda.arachsys.com/csharp/unicode.html
									// http://en.wikipedia.org/wiki/UTF-32/UCS-4
									var hex = new string(buffer);
									var unicode = (char)Convert.ToInt32(hex, 16);
									sb.Append(unicode);
									index += 4;
								}
								else
								{
									break;
								}
								break;
						}
						break;
					default:
						sb.Append(symbol);
						break;
				}
				symbol = data[++index];
			}
		}

		private object ParseNumber(IList<char> data, ref int index)
		{
			var symbol = data[index];
			IgnoreWhitespace(data, ref index, symbol);

			var start = index;
			var length = 0;
			while (ParseToken(JsonToken.Number, data, ref index))
			{
				length++;
				index++;
			}

			var number = new char[length];
			Array.Copy(data.ToArray(), start, number, 0, length);

			decimal result;
			var buffer = new String(number);

			if (!Decimal.TryParse(buffer, JsonNumberStyle, CultureInfo.InvariantCulture, out result))
				throw new InvalidJsonException(
					String.Format("Value '{0}' was not a valid JSON number", buffer), data, index
					);

			return result;
		}

		private KeyValuePair<string, object> ParsePair(IList<char> data, ref int index)
		{
			var valid = true;

			var name = ParseString(data, ref index);
			if (name == null)
				valid = false;

			if (!ParseToken(JsonToken.Colon, data, ref index))
				valid = false;

			index++;
			if (!valid)
				throw new InvalidJsonException(String.Format(
							"Invalid JSON found while parsing a value pair at index {0}.", index
							), data, index);

			var value = ParseValue(data, ref index);
			//index++;

			return new KeyValuePair<string, object>(name, value);
		}

		private object ParseValue(IList<char> data, ref int index)
		{
			var token = NextToken(data, ref index);
			switch (token)
			{
				case JsonToken.String:
					return ParseString(data, ref index);
				case JsonToken.Number:
					return ParseNumber(data, ref index);
				case JsonToken.LeftBrace:
					return ParseObject(data, ref index);
				case JsonToken.LeftBracket:
					return ParseArray(data, ref index);
				case JsonToken.True:
					return true;
				case JsonToken.False:
					return false;
				case JsonToken.Null:
					return null;

				default:
					throw new InvalidJsonException(string.Format(
							"Invalid JSON found while parsing a value at index {0}.", index
							), data, index);
			}
		}

		private bool ParseToken(JsonToken token, IList<char> data, ref int index)
		{
			var nextToken = NextToken(data, ref index);
			return token == nextToken;
		}

		private JsonToken NextToken(IList<char> data, ref int index)
		{
			var symbol = data[index];
			var token = GetTokenFromSymbol(symbol);
			token = IgnoreWhitespace(data, ref index, ref token, symbol);

			GetKeyword("true", JsonToken.True, data, ref index, ref token);
			GetKeyword("false", JsonToken.False, data, ref index, ref token);
			GetKeyword("null", JsonToken.Null, data, ref index, ref token);

			return token;
		}

		private JsonToken GetTokenFromSymbol(char symbol)
		{
			return GetTokenFromSymbol(symbol, JsonToken.Unknown);
		}

		private JsonToken GetTokenFromSymbol(char symbol, JsonToken token)
		{
			switch (symbol)
			{
				case '{':
					token = JsonToken.LeftBrace;
					break;
				case '}':
					token = JsonToken.RightBrace;
					break;
				case ':':
					token = JsonToken.Colon;
					break;
				case ',':
					token = JsonToken.Comma;
					break;
				case '[':
					token = JsonToken.LeftBracket;
					break;
				case ']':
					token = JsonToken.RightBracket;
					break;
				case '"':
					token = JsonToken.String;
					break;
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case '.':
				case 'e':
				case 'E':
				case '+':
				case '-':
					token = JsonToken.Number;
					break;
			}
			return token;
		}

		private void IgnoreWhitespace(IList<char> data, ref int index, char symbol)
		{
			var token = JsonToken.Unknown;
			IgnoreWhitespace(data, ref index, ref token, symbol);
			return;
		}

		private JsonToken IgnoreWhitespace(IList<char> data, ref int index, ref JsonToken token, char symbol)
		{
			switch (symbol)
			{
				case ' ':
				case '\\':
				case '/':
				case '\b':
				case '\f':
				case '\n':
				case '\r':
				case '\t':
					index++;
					token = NextToken(data, ref index);
					break;
			}
			return token;
		}

		private void GetKeyword(string word, JsonToken target, IList<char> data, ref int index, ref JsonToken result)
		{
			var buffer = data.Count - index;
			if (buffer < word.Length)
			{
				return;
			}

			for (var i = 0; i < word.Length; i++)
			{
				if (data[index + i] != word[i])
				{
					return;
				}
			}

			result = target;
			index += word.Length;
		}
	}
}