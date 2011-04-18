using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ManagedFusion.Serialization;
using System.Collections;

namespace ManagedFusion.Tests
{
	[TestFixture]
	public class JsonTest
	{
		[Test]
		public void Serialize_An_Array()
		{
			// arrange
			var expected = @"[{""X"":""test""},{""X"":""test1""},{""X"":""test2""}]";
			var body = new[] { 
				new { X = "test" },
				new { X = "test1" },
				new { X = "test2" }
			};

			// act
			var actual = body.ToJson();

			// assert 
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void Deserialize_An_Array()
		{
			// arrange
			var expected = @"[{""X"":""test""},{""X"":""test1""},{""X"":""test2""}]";

			// act
			var jsonObj = expected.FromJson();
			var actual = (ICollection)jsonObj["collection"];

			// assert 
			Assert.IsNotEmpty(actual);
			Assert.AreEqual(3, actual.Count);
		}
	}
}
