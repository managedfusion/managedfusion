using System;
using NUnit.Framework;
using System.Dynamic;
using ManagedFusion.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace ManagedFusion.Tests
{
	[TestFixture]
	public class ModelSerializerTest
	{
		private class TestModel : IModelSerializer
		{
			#region IModelSerializer Members

			IDictionary<string, object> IModelSerializer.GetSerializedModel()
			{
				dynamic model = new ExpandoObject();

				((IDictionary<string, object>)model).Add(Serializer.ModelNameKey, "test");
				model.name = "value";

				return model;
			}

			#endregion
		}

		[Test]
		public void Simple_With_Name()
		{
			// arrange
			var expected = "test";
			var obj = new TestModel();

			var ser = new Serializer();
			var options = new SerlizerOptions { CheckForObjectName = true };

			// act
			var result = ser.FromObject(obj, options);

			// assert
			Assert.AreEqual(expected, result.Keys.First());
		}
	}
}
