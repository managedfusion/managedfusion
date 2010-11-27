using System;
using NUnit.Framework;
using System.Dynamic;
using ManagedFusion.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace ManagedFusion.Tests
{
	[TestFixture]
	public class SerializerTest
	{
		[Test]
		public void Simple()
		{
			var expected = "test";
			var obj = new Dictionary<string, object>() { 
				{ "name", expected }
			};

			var ser = new Serializer();
			var options = new SerlizerOptions();
			var result = ser.FromObject(obj, options);

			Assert.AreEqual(expected, result["name"]);
		}

		[Test]
		public void Simple_With_Name()
		{
			var expected = "test";
			var obj = new Dictionary<string, object>() { 
				{ Serializer.ModelNameKey, expected },
				{ "name", "value" }
			};

			var ser = new Serializer();
			var options = new SerlizerOptions { CheckForObjectName = true };
			var result = ser.FromObject(obj, options);

			Assert.AreEqual(expected, result.Keys.First());
		}
	}
}
