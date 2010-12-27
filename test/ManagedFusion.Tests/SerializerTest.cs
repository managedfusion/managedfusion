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
			// arrange
			var expected = "test";
			var obj = new Dictionary<string, object>() { 
				{ "name", expected }
			};

			var ser = new Serializer();
			var options = new SerlizerOptions();
			
			// act
			var result = ser.FromObject(obj, options);

			// assert
			Assert.AreEqual(expected, result["name"]);
		}

		[Test]
		public void Simple_With_Name()
		{
			// arrange
			var expected = "test";
			var obj = new Dictionary<string, object>() { 
				{ Serializer.ModelNameKey, expected },
				{ "name", "value" }
			};

			var ser = new Serializer();
			var options = new SerlizerOptions { CheckForObjectName = true };
			
			// act
			var result = ser.FromObject(obj, options);

			// assert
			Assert.AreEqual(expected, result.Keys.First());
		}

		[Test]
		public void Should_Not_Contain_ModelName()
		{
			// arrange
			var expected = "test";
			var obj = new Dictionary<string, object>() { 
				{ Serializer.ModelNameKey, expected },
				{ "name", "value" }
			};

			var ser = new Serializer();
			var options = new SerlizerOptions { CheckForObjectName = true };
			
			// act
			var result = ser.FromObject(obj, options);

			// assert
			var model = result.First();
			Assert.AreEqual(expected, model.Key);
			Assert.IsInstanceOf<IDictionary<string, object>>(model.Value);

			var modelValue = model.Value as IDictionary<string,object>;
			Assert.IsFalse(modelValue.ContainsKey(Serializer.ModelNameKey));
		}
	}
}
