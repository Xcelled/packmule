using System;
using System.Text;

namespace PackMule.Core
{
	internal class PackCommon
	{
		/// <summary>
		/// The header sequence of all pack files
		/// </summary>
		public static readonly byte[] Header = { 0x50, 0x41, 0x43, 0x4B, 0x02, 0x01, 0x00, 0x00 }; // 'P' 'A' 'C' 'K' 0x2, 0x1, 0x00, 0x00

		/// <summary>
		/// Encodes a string for storage in a pack file. Adds null terminal.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>System.Byte[].</returns>
		public static byte[] EncodeName(string name)
		{
			var nameRaw = Encoding.UTF8.GetBytes(name);

			var requiredLength = nameRaw.Length + 1; // +1 for null terminal

			if (requiredLength <= ((0x10 * 4) - 1))
			{
				var scale = (byte)(requiredLength / 0x10);
				var buffer = new byte[(scale + 1) * 0x10];
				buffer[0] = scale;

				Buffer.BlockCopy(nameRaw, 0, buffer, 1, nameRaw.Length);

				return buffer;
			}
			else if (requiredLength <= (0x60 - 1))
			{
				var buffer = new byte[0x60];
				buffer[0] = 4;

				Buffer.BlockCopy(nameRaw, 0, buffer, 1, nameRaw.Length);

				return buffer;
			}
			else
			{
				var buffer = new byte[requiredLength + 1 + sizeof(int)];
				buffer[0] = 5;
				BitConverter.GetBytes(requiredLength).CopyTo(buffer, 1);

				Buffer.BlockCopy(nameRaw, 0, buffer, 1 + sizeof(int), nameRaw.Length);

				return buffer;
			}
		}
	}
}
