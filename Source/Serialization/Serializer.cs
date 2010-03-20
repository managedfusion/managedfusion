using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web;

namespace ManagedFusion.Serialization
{
	public class Serializer
	{
		/// <summary>
		/// 
		/// </summary>
		public const char AttributeMarker = '*';

		/// <summary>
		/// 
		/// </summary>
		public const char CollectionItemMarker = '+';

		/// <summary>
		/// 
		/// </summary>
		public Serializer()
		{
			SerializePublicMembers = false;
			FollowFrameworkIgnoreAttributes = false;
			LevelsToSerialize = 5;
		}

		/// <summary>
		/// Gets or sets a value indicating whether [serialize public members].
		/// </summary>
		/// <value>
		/// 	<see langword="true"/> if [serialize public members]; otherwise, <see langword="false"/>.
		/// </value>
		public bool SerializePublicMembers { get; set; }

		/// <summary>
		/// Gets or sets a value indicating if the serializer should follow the .NET Framework Ignore attributes.
		/// </summary>
		public bool FollowFrameworkIgnoreAttributes { get; set; }

		/// <summary>
		/// Gets or sets the number of levels to serialize.
		/// </summary>
		public int LevelsToSerialize { get; set; }

		/// <summary>
		/// Serializes the specified obj.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="serializer">The serializer.</param>
		/// <returns></returns>
		public string SerializeToString(object obj, ISerializer serializer)
		{
			return serializer.SerializeToString(SerializeToDictionary(obj, serializer));
		}

		/// <summary>
		/// Serializes the specified obj.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="checkForObjectName">if set to <see langword="true"/> [check for object name].</param>
		/// <returns></returns>
		public Dictionary<string, object> SerializeToDictionary(object obj, ISerializerOptions options)
		{
			object value = SerializeValue(obj, 0 /* level */, options.MaxSerializableLevelsSupported ?? LevelsToSerialize);

			if (options.CheckForObjectName || !(value is Dictionary<string, object>))
			{
				string name = obj is IEnumerable ? "collection" : "object";

				// make sure the object isn't an easily handled primity type with IEnumerable
				if (Type.GetTypeCode(obj.GetType()) != TypeCode.Object)
					name = "object";

				// get what the object likes to be called
				if (obj.GetType().IsDefined(typeof(SerializableObjectAttribute), true))
				{
					object[] attrs = obj.GetType().GetCustomAttributes(typeof(SerializableObjectAttribute), true);

					if (attrs.Length > 0)
					{
						SerializableObjectAttribute attr = attrs[0] as SerializableObjectAttribute;

						name = attr.Name;
					}
				}

				Dictionary<string, object> response = new Dictionary<string, object>(1);
				response.Add(name, value);

				value = response;
			}

			return value as Dictionary<string, object>;
		}

		/// <summary>
		/// Serializes the object.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		private IDictionary<string, object> SerializeObject(object obj, int level, int levelLimit)
		{
			IDictionary<string, object> values = new Dictionary<string, object>();

			if (level >= levelLimit)
				return values;
			else
				level++;

			Type type = obj.GetType();
			bool anonymousType = type.Name.IndexOf("__AnonymousType") >= 0;

			foreach (FieldInfo info in type.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (FollowFrameworkIgnoreAttributes && (info.IsDefined(typeof(System.Xml.Serialization.XmlIgnoreAttribute), true) || info.IsDefined(typeof(System.Xml.Serialization.SoapIgnoreAttribute), true)))
					continue;

				if ((SerializePublicMembers && info.IsPublic) || info.IsDefined(typeof(SerializablePropertyAttribute), true))
					values.Add(SerializeName(info), SerializeValue(info.GetValue(obj), level, levelLimit));
			}

			foreach (PropertyInfo info2 in type.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				MethodInfo getMethod = info2.GetGetMethod(true);

				if (FollowFrameworkIgnoreAttributes && (info2.IsDefined(typeof(System.Xml.Serialization.XmlIgnoreAttribute), true) || info2.IsDefined(typeof(System.Xml.Serialization.SoapIgnoreAttribute), true)))
					continue;

				if ((getMethod != null) && (getMethod.GetParameters().Length <= 0))
					if (anonymousType || (SerializePublicMembers && getMethod.IsPublic) || info2.IsDefined(typeof(SerializablePropertyAttribute), true))
						values.Add(SerializeName(info2), SerializeValue(getMethod.Invoke(obj, null), level, levelLimit));
			}

			return values;
		}

		/// <summary>
		/// Serializes the name.
		/// </summary>
		/// <param name="member">The member.</param>
		/// <returns></returns>
		private string SerializeName(MemberInfo member)
		{
			string name = null;

			if (member.IsDefined(typeof(SerializablePropertyAttribute), true))
			{
				object[] attrs = member.GetCustomAttributes(typeof(SerializablePropertyAttribute), true);

				if (attrs.Length > 0)
				{
					SerializablePropertyAttribute attr = attrs[0] as SerializablePropertyAttribute;

					name = (attr.IsAttribute ? AttributeMarker.ToString() : String.Empty) + attr.Name;
				}
			}

			if (String.IsNullOrEmpty(name))
				name = null;

			return name ?? member.Name;
		}

		/// <summary>
		/// Serializes the value.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// 
		/// <returns></returns>
		private object SerializeValue(object obj, int level, int levelLimit)
		{
			if (obj == null)
				return obj;

			Type objectType = obj.GetType();

			// make sure the object isn't an easily handled primity type
			if (Type.GetTypeCode(objectType) != TypeCode.Object)
				return obj;

			if (obj is IDictionary)
				return obj;

			if (obj is IEnumerable)
			{
				object[] attrs = objectType.GetCustomAttributes(typeof(SerializableCollectionObjectAttribute), true);
				string collectionItemName = null;

				if (attrs.Length > 0)
				{
					SerializableCollectionObjectAttribute attr = attrs[0] as SerializableCollectionObjectAttribute;
					collectionItemName = CollectionItemMarker + attr.Name;
				}

				IList<object> list = new List<object>();
				IEnumerable collection = (IEnumerable)obj;

				if (attrs.Length == 0)
				{
					foreach (object o in collection)
					{
						if (Type.GetTypeCode(o.GetType()) != TypeCode.Object)
							list.Add(o);
						else
							list.Add(SerializeValue(o, level, levelLimit));
					}
				}
				else
				{
					foreach (object o in collection)
					{
						IDictionary<string, object> list2 = new Dictionary<string, object>();

						if (Type.GetTypeCode(o.GetType()) != TypeCode.Object)
							list2.Add(collectionItemName, o);
						else
							list2.Add(collectionItemName, SerializeValue(o, level, levelLimit));

						list.Add(list2);
					}
				}

				return list;
			}

			IDictionary<string, object> serializedObject = SerializeObject(obj, level, levelLimit);
			return serializedObject.Count == 0 ? obj : serializedObject;
		}
	}
}
