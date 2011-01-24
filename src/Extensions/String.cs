using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.IO;

namespace System
{
	/// <summary>
	/// 
	/// </summary>
	public static class StringExtensions
	{
		private static readonly Regex UrlReplacementExpression = new Regex(@"[^0-9a-z \-]*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
		private const string InitVector = "ManagedFusion IV";

		/// <summary>
		/// Tries the trim.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <returns></returns>
		public static string TryTrim(this string s)
		{
			if (s == null)
				return s;

			return s.Trim();
		}

		public static string Encrypt(this string content, string key)
		{
			byte[] initVectorBytes = Encoding.UTF8.GetBytes(InitVector);
			byte[] keyBytesLong;
			using (var sha = new SHA1CryptoServiceProvider())
				keyBytesLong = sha.ComputeHash(Encoding.UTF8.GetBytes(key));

			byte[] keyBytes = new byte[16];
			Array.Copy(keyBytesLong, keyBytes, 16);

			byte[] textBytes = Encoding.UTF8.GetBytes(content);
			for (int i = 0; i < 16; i++)
				textBytes[i] ^= initVectorBytes[i];

			// encrypt the string to an array of bytes
			byte[] encrypted = Encrypt(textBytes, keyBytes, initVectorBytes);
			string encoded = Convert.ToBase64String(encrypted);
			return encoded;
		}

		private static byte[] Encrypt(byte[] textBytes, byte[] key, byte[] iv)
		{
			// Declare the stream used to encrypt to an in memory
			// array of bytes and the RijndaelManaged object
			// used to encrypt the data.
			using (var msEncrypt = new MemoryStream())
			using (var aesAlg = new RijndaelManaged())
			{
				// Provide the RijndaelManaged object with the specified key and IV.
				aesAlg.Mode = CipherMode.CBC;
				aesAlg.Padding = PaddingMode.PKCS7;
				aesAlg.KeySize = 128;
				aesAlg.BlockSize = 128;
				aesAlg.Key = key;
				aesAlg.IV = iv;

				// Create an encrytor to perform the stream transform.
				var encryptor = aesAlg.CreateEncryptor();

				// Create the streams used for encryption.
				using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
				{
					csEncrypt.Write(textBytes, 0, textBytes.Length);
					csEncrypt.FlushFinalBlock();
				}

				byte[] encrypted = msEncrypt.ToArray();

				// Return the encrypted bytes from the memory stream.
				return encrypted;
			}
		}

		public static string Decrypt(this string content, string key)
		{
			byte[] initVectorBytes = Encoding.UTF8.GetBytes(InitVector);
			byte[] keyBytesLong;
			using (var sha = new SHA1CryptoServiceProvider())
				keyBytesLong = sha.ComputeHash(Encoding.UTF8.GetBytes(key));

			byte[] keyBytes = new byte[16];
			Array.Copy(keyBytesLong, keyBytes, 16);

			byte[] textBytes = Convert.FromBase64String(content);

			// decrypt the string to an array of bytes
			byte[] decrypted = Decrypt(textBytes, keyBytes, initVectorBytes);

			for (int i = 0; i < 16; i++)
				decrypted[i] ^= initVectorBytes[i];

			string decoded = Encoding.UTF8.GetString(decrypted);
			return decoded;
		}

		private static byte[] Decrypt(byte[] textBytes, byte[] key, byte[] iv)
		{
			// Declare the stream used to encrypt to an in memory
			// array of bytes and the RijndaelManaged object
			// used to encrypt the data.
			using (var msDecrypt = new MemoryStream())
			using (var aesAlg = new RijndaelManaged())
			{
				// Provide the RijndaelManaged object with the specified key and IV.
				aesAlg.Mode = CipherMode.CBC;
				aesAlg.Padding = PaddingMode.PKCS7;
				aesAlg.KeySize = 128;
				aesAlg.BlockSize = 128;
				aesAlg.Key = key;
				aesAlg.IV = iv;

				// Create an decrypter to perform the stream transform.
				var decryptor = aesAlg.CreateDecryptor();

				// Create the streams used for encryption.
				int count;
				var buffer = new byte[16 * 1024];
				using (var msEncrypt = new MemoryStream(textBytes))
				using (var csDecrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Read))
				{
					while ((count = csDecrypt.Read(buffer, 0, buffer.Length)) > 0)
						msDecrypt.Write(buffer, 0, count);
				}

				byte[] decrypted = msDecrypt.ToArray();

				// Return the decrypted bytes from the memory stream.
				return decrypted;
			}
		}

		/// <summary>
		/// Creates the hash algorithm.
		/// </summary>
		/// <param name="hashName">Name of the hash.</param>
		/// <returns></returns>
		private static HashAlgorithm CreateHashAlgorithm(string hashName)
		{
			switch (hashName.ToLower())
			{
				case "crc32":
					return new ManagedFusion.Crc32();

				default:
					return HashAlgorithm.Create(hashName);
			}
		}

		/// <summary>
		/// Toes the hash.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <returns></returns>
		public static byte[] ToHash(this string content)
		{
			return ToHash("MD5");
		}

		/// <summary>
		/// Toes the hash string.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="hashName">Name of the hash.</param>
		/// <returns></returns>
		public static byte[] ToHash(this string content, string hashName)
		{
			return ToHash(content, hashName, Encoding.Default);
		}

		/// <summary>
		/// Toes the hash string.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="hashName">Name of the hash.</param>
		/// <param name="encoding">The encoding of the string.</param>
		/// <returns></returns>
		public static byte[] ToHash(this string content, string hashName, Encoding encoding)
		{
			if (content == null)
				throw new ArgumentNullException("content");

			HashAlgorithm algorithm = CreateHashAlgorithm(hashName);
			byte[] buffer = algorithm.ComputeHash(encoding.GetBytes(content));
			return buffer;
		}

		/// <summary>
		/// Toes the hash.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <returns></returns>
		public static string ToHashString(this string content)
		{
			return ToHashString(content, "MD5");
		}

		/// <summary>
		/// Toes the hash string.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="hashName">Name of the hash.</param>
		/// <returns></returns>
		public static string ToHashString(this string content, string hashName)
		{
			return ToHashString(content, hashName, Encoding.Default);
		}

		/// <summary>
		/// Toes the hash string.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="hashName">Name of the hash.</param>
		/// <param name="encoding">The encoding of the string.</param>
		/// <returns></returns>
		public static string ToHashString(this string content, string hashName, Encoding encoding)
		{
			byte[] buffer = ToHash(content, hashName, encoding);
			StringBuilder builder = new StringBuilder(buffer.Length * 2);

			foreach (byte b in buffer)
				builder.Append(b.ToString("x2"));

			return builder.ToString();
		}

		/// <summary>
		/// Toes the hash int64.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <returns></returns>
		public static long ToHashInt64(this string content)
		{
			return ToHashInt64(content, "MD5");
		}

		/// <summary>
		/// Toes the hash int64.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="hashName">Name of the hash.</param>
		/// <returns></returns>
		public static long ToHashInt64(this string content, string hashName)
		{
			return ToHashInt64(content, hashName, Encoding.Default);
		}

		/// <summary>
		/// Toes the hash int64.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="hashName">Name of the hash.</param>
		/// <param name="encoding">The encoding of the string.</param>
		/// <returns></returns>
		public static long ToHashInt64(this string content, string hashName, Encoding encoding)
		{
			byte[] buffer = ToHash(content, hashName, encoding);
			return BitConverter.ToInt64(buffer, 0);
		}

		/// <summary>
		/// Toes the URL part.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <returns></returns>
		public static string ToUrlFormat(this string content)
		{
			return UrlReplacementExpression.Replace(content.Trim(), String.Empty).Replace(' ', '-').ToLowerInvariant();
		}

		/// <summary>
		/// Tries to create a phrase string from Pascal case text.
		/// Will place spaces before capitalized letters.
		/// 
		/// Note that this method may not work for round tripping 
		/// ToPascalCase calls, since ToPascalCase strips more characters
		/// than just spaces.
		/// </summary>
		/// <param name="camelCase"></param>
		/// <returns></returns>
		public static string FromPascalCase(this string pascalCase)
		{
			if (pascalCase == null)
				throw new ArgumentNullException("camelCase");

			StringBuilder sb = new StringBuilder(pascalCase.Length + 10);
			bool first = true;
			char lastChar = '\0';

			foreach (char ch in pascalCase)
			{
				if (!first && (Char.IsUpper(ch) || Char.IsDigit(ch) && !Char.IsDigit(lastChar)))
					sb.Append(' ');

				// append the character to the string builder
				sb.Append(ch);

				first = false;
				lastChar = ch;
			}

			return sb.ToString();
		}

		/// <summary>
		/// Takes a phrase and turns it into Pascal case text.
		/// White Space, punctuation and separators are stripped
		/// </summary>
		/// <param name="?"></param>
		public static string ToPascalCase(this string phrase)
		{
			if (phrase == null)
				return String.Empty;

			StringBuilder sb = new StringBuilder(phrase.Length);

			// First letter is always upper case
			bool nextUpper = true;

			foreach (char ch in phrase)
			{
				if (Char.IsWhiteSpace(ch) || Char.IsPunctuation(ch) || Char.IsSeparator(ch))
				{
					nextUpper = true;
					continue;
				}

				if (nextUpper)
					sb.Append(Char.ToUpper(ch));
				else
					sb.Append(Char.ToLower(ch));

				nextUpper = false;
			}

			return sb.ToString();
		}
	}
}
