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
	public class JsonObjectTest
	{
		[Test, Ignore]
		public void Complex()
		{
			// arrange
			var json = Properties.Resources.ComplexJson;

			// act 
			dynamic obj = JsonObject.Parse(json);

			// assert
			Assert.IsInstanceOf<ICollection>(obj.d);
		}

		[Test]
		public void Json_Array_Should_Be_ICollection()
		{
			// arrange
			var json = "{ \"array\": [ 1, 2, 3 ] }";

			// act
			dynamic obj = JsonObject.Parse(json);

			// assert
			Assert.IsInstanceOf<ICollection>(obj.array);
		}

		[Test]
		public void Json_Empty_Array_Should_Be_ICollection()
		{
			// arrange
			var json = "{ \"array\": [ ] }";

			// act
			dynamic obj = JsonObject.Parse(json);

			// assert
			Assert.IsInstanceOf<ICollection>(obj.array);
		}

		[Test]
		public void Dictionary_Array_Should_Be_ICollection()
		{
			// arrange
			var json = new Dictionary<string, object> {
				{ "array", new[] { 1, 2, 3 } }
			};

			// act
			dynamic obj = JsonObject.Parse(json);

			// assert
			Assert.IsInstanceOf<ICollection>(obj.array);
		}

		[Test]
		public void Dictionary_Empty_Array_Should_Be_ICollection()
		{
			// arrange
			var json = new Dictionary<string, object> {
				{ "array", new int[0] }
			};

			// act
			dynamic obj = JsonObject.Parse(json);

			// assert
			Assert.IsInstanceOf<ICollection>(obj.array);
		}
	}
}
