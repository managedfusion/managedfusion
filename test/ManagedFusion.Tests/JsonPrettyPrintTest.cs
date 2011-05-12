using ManagedFusion.Serialization;
using NUnit.Framework;

namespace ManagedFusion.Tests
{
	[TestFixture]
	public class JsonPrettyPrintTest
	{
		[SetUp]
		public void SetUp()
		{
			JsonSerializer.PrettyPrint = true;
		}

		[TearDown]
		public void TearDown()
		{
			JsonSerializer.PrettyPrint = false;
		}

		private string Clean(string json)
		{
			return json.Replace("\r\n", "\n");
		}

		[Test]
		public void Simple()
		{
			// arrange
			var expected = Clean(@"{
	""X"": {
		""Y"": 0
	}
}");
			var obj = new {
				X = new { Y = 0 }
			};

			// act
			var actual = obj.ToJson();

			// assert 
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void Simple_Two_First_Level_Properties()
		{
			// arrange
			var expected = Clean(@"{
	""X"": {
		""Y"": 0
	},
	""X2"": {
		""Y"": 1
	}
}");
			var obj = new {
				X = new { Y = 0 },
				X2 = new { Y = 1 }
			};

			// act
			var actual = obj.ToJson();

			// assert 
			Assert.AreEqual(expected, actual);
		}


		[Test]
		public void Simple_Two_Second_Level_Properties()
		{
			// arrange
			var expected = Clean(@"{
	""X"": {
		""Y"": 0,
		""Z"": 1
	},
	""X2"": {
		""Y"": 1,
		""Z"": ""End Of Line""
	}
}");
			var obj = new {
				X = new { Y = 0, Z = 1 },
				X2 = new { Y = 1, Z = "End Of Line" }
			};

			// act
			var actual = obj.ToJson();

			// assert 
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void SimpleArray()
		{
			// arrange
			var expected = Clean(@"{
	""array"": [
		0,
		1,
		2
	]
}");
			var obj = new { array = new[] { 0, 1, 2 } };

			// act
			var actual = obj.ToJson();

			// assert 
			Assert.AreEqual(expected, actual);
		}
	}
}
