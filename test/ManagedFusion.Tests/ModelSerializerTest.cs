using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using ManagedFusion.Serialization;
using NUnit.Framework;

namespace ManagedFusion.Tests
{
	[TestFixture]
	public class ModelSerializerTest
	{
		private class TestModel : IModelSerializer
		{
			#region IModelSerializer Members

			public IDictionary<string, object> GetSerializedModel()
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

		[Test]
		public void Default_JsonObject()
		{
			// arrange
			var json = JsonObject.Parse("{\"name\":\"value\"}");

			// act, assert

			// throw error when no present
			Assert.Throws<MissingMemberException>(() => {
				object actual = json.NotPresent;
			});

			// throw error when case isn't correct
			Assert.Throws<MissingMemberException>(() => {
				object actual = json.Name;
			});

			Assert.AreEqual("value", json.name);
		}

		[Test]
		public void Member_OrdinalIgnoreCase_Comparison()
		{
			// arrange
			var json = JsonObject.Parse("{\"name\":\"value\"}", methodComparisonType: StringComparison.OrdinalIgnoreCase);

			// act, assert
			Assert.AreEqual("value", json.Name);
		}

		[Test]
		public void Member_Missing_Doesnt_Throw_Error()
		{
			// arrange
			var json = JsonObject.Parse("{\"name\":\"value\"}", throwErrorOnMissingMethod: false);

			// act, assert
			Assert.DoesNotThrow(() => {
				object actual = json.NotPresent;
			});
		}
	}
}
