using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ManagedFusion.Tests
{
	[TestFixture]
	public class StringExtensionsTest
	{
		[Test]
		public void Encrypt_And_Decrypt()
		{
			var key = "TEST1234";
			var content = "Nothing To See Here This Is Just A Test!";

			var encrypted = content.Encrypt(key);
			var decrypted = encrypted.Decrypt(key);

			Assert.AreEqual(content, decrypted);
		}
	}
}
