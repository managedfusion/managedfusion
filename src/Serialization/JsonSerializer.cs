using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ManagedFusion.Serialization
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso href="http://www.ietf.org/rfc/rfc4627.txt"/>
	/// <seealso href="http://www.json.org"/>
	/// <seelaos href="http://tools.ietf.org/html/draft-zyp-json-schema-02"/>
	/// <seealso href="http://json-schema.org/"/>
	public class JsonSerializer : ISerializer
	{
		private const char BeginObject = '{';
		private const char EndObject = '}';
		private const char BeginArray = '[';
		private const char EndArray = ']';
		private const char ValueSeperator = ',';
		private const char NameSeperator = ':';
		private const char BeginString = '\"';
		private const char EndString = '\"';
		private const string BeginDate = "";
		private const string EndDate = "";
		private const string TrueValue = "true";
		private const string FalseValue = "false";
		private const string NullValue = "null";

		private static readonly DateTime UnixTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		static JsonSerializer()
		{
			DateTimeFormat = JsonDateTimeFormat.DateTime;
		}

		public static JsonDateTimeFormat DateTimeFormat { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonSerializer"/> class.
		/// </summary>
		public JsonSerializer()
		{
			CheckForObjectName = false;
			MaxSerializableLevelsSupported = null;
		}

		/// <summary>
		/// Gets a value indicating whether to check for object name.
		/// </summary>
		/// <value>
		/// 	<see langword="true"/> if [check for object name]; otherwise, <see langword="false"/>.
		/// </value>
		public virtual bool CheckForObjectName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the max levels allowed to be serialized
		/// </summary>
		public virtual int? MaxSerializableLevelsSupported
		{
			get;
			set;
		}

		/// <summary>
		/// Serializes to JSON
		/// </summary>
		/// <param name="serialization">The object to serialize.</param>
		public virtual string Serialize(IDictionary<string, object> serialization)
		{
			StringBuilder builder = new StringBuilder();
			Serialize(serialization, new StringWriter(builder));
			return builder.ToString();
		}

		/// <summary>
		/// Serializes to JSON using stream.
		/// </summary>
		/// <param name="serialization">The object to serialize.</param>
		public virtual void Serialize(IDictionary<string, object> serialization, TextWriter writer)
		{
			BuildObject(writer, serialization);
		}

		/// <summary>
		/// Builds the name/value pair.
		/// </summary>
		public void BuildPair(TextWriter builder, string name, object value)
		{
			BuildString(builder, name);
			builder.Write(NameSeperator);
			BuildValue(builder, value);
		}

		/// <summary>
		/// Builds the JSON object.
		/// </summary>
		private void BuildObject(TextWriter builder, IDictionary<string, object> serialization)
		{
			builder.Write(BeginObject);

			int count = 0;
			var finalCount = serialization.Count;
			foreach (var entry in serialization)
			{
				if (entry.Key == Serializer.ModelNameKey)
					continue;

				BuildPair(builder,
					entry.Key.TrimStart(new char[] { Serializer.AttributeMarker, Serializer.CollectionItemMarker }),
					entry.Value
				);

				if (count++ < finalCount)
					builder.Write(ValueSeperator);
			}

			builder.Write(EndObject);
		}

		/// <summary>
		/// Builds the JSON array.
		/// </summary>
		private void BuildArray(TextWriter builder, IEnumerable array)
		{
			builder.Write(BeginArray);

			int count = 0;
			var finalCount = array.Cast<object>().Count();
			foreach (var obj in array)
			{
				BuildValue(builder, obj);

				if (count++ < finalCount)
					builder.Write(ValueSeperator);
			}

			builder.Write(EndArray);
		}

		/// <summary>
		/// Builds the JSON value.
		/// </summary>
		private void BuildValue(TextWriter builder, object value)
		{
			if (value == null)
			{
				builder.Write(NullValue);
			}
			else if (value is IDictionary<string,object>)
			{
				BuildObject(builder, value as IDictionary<string,object>);
			}
			else if (value is String)
			{
				BuildString(builder, value as string);
			}
			else if (value is DateTime || value is DateTimeOffset)
			{
				DateTime dt = (value is DateTimeOffset) ? ((DateTimeOffset)value).UtcDateTime : ((DateTime)value);
				BuildDate(builder, dt);
			}
			else if (value is Boolean)
			{
				builder.Write(((bool)value) ? TrueValue : FalseValue);
			}
			else if (value is Int16 || value is Int32 || value is Int64 || value is Decimal || value is Byte || value is SByte || value is UInt16 || value is UInt32 || value is UInt64)
			{
				builder.Write(value);
			}
			else if (value is Double || value is Single)
			{
				builder.Write("{0:R}", value);
			}
			else if (value is byte[])
			{
				BuildString(builder, Convert.ToBase64String(value as byte[]));
			}
			else if (value is IEnumerable)
			{
				BuildArray(builder, value as IEnumerable);
			}
			// else if (value is Char)
			// else if (value is Enum)
			// else if (value is Guid)
			// else if (value is TimeSpan)
			else
			{
				BuildString(builder, Convert.ToString(value));
			}
		}

		/// <summary>
		/// Builds the JSON DateTime.
		/// </summary>
		private void BuildDate(TextWriter builder, DateTime datetime)
		{
			datetime = datetime.ToUniversalTime();

			switch (DateTimeFormat)
			{
				case JsonDateTimeFormat.Unix:
					builder.Write(Math.Floor((datetime - UnixTime).TotalMilliseconds));
					break;

				case JsonDateTimeFormat.RFC1123:
					BuildString(builder, datetime.ToString("R"));
					break;

				case JsonDateTimeFormat.ISO8601:
					BuildString(builder, datetime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));
					break;

				case JsonDateTimeFormat.Date:
					BuildString(builder, datetime.ToString("yyyy'-'MM'-'dd'"));
					break;

				case JsonDateTimeFormat.Time:
					BuildString(builder, datetime.ToString("'HH':'mm':'ss"));
					break;
			}
		}

		/// <summary>
		/// Escapes the string.
		/// </summary>
		private void BuildString(TextWriter builder, string s)
		{
			builder.Write(BeginString);

			for (int i = 0; i < s.Length; i++)
			{
				switch (s[i])
				{
					case  '"': builder.Write(@"\"""); break;
					case '\\': builder.Write(@"\\"); break;
					case  '/': builder.Write(@"\/"); break;
					case '\b': builder.Write(@"\b"); break;
					case '\f': builder.Write(@"\f"); break;
					case '\n': builder.Write(@"\n"); break;
					case '\r': builder.Write(@"\r"); break;
					case '\t': builder.Write(@"\t"); break;
					default:
						// chedk for a instance character and escape as unicode
						if (Char.IsControl(s, i))
							builder.Write(@"\u" + ((int)s[i]).ToString("X4"));
						else
							builder.Write(s[i]);
						break;
				}
			}

			builder.Write(EndString);
		}
	}
}
