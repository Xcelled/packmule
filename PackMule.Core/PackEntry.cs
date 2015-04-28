using System;
using PackMule.Core.Structs;

namespace PackMule.Core
{
	/// <summary>
	/// An entry in a packfile
	/// </summary>
	public class PackEntry
	{
		private string _name;

		/// <summary>
		/// Gets the maximum allowed length of the name. Length can't be greater than this number
		/// </summary>
		/// <value>The maximum length of the name, or -1 if there is no limit</value>
		public int MaxNameLength { get; private set; }

		/// <summary>
		/// Gets the crypto seed.
		/// </summary>
		/// <value>The crypto seed.</value>
		public int CryptoSeed { get; set; }

		/// <summary>
		/// Gets a value indicating whether the data instance is compressed.
		/// </summary>
		/// <value><c>true</c> if this instance is compressed; otherwise, <c>false</c>.</value>
		public bool IsCompressed { get; set; }

		/// <summary>
		/// Gets the size in the pack file.
		/// </summary>
		/// <value>The packed size.</value>
		public int SizeInPack { get; set; }

		/// <summary>
		/// Gets the size of the decompressed data.
		/// </summary>
		/// <value>The size of the decompressed data.</value>
		public int DecompressedSize { get; set; }

		/// <summary>
		/// Gets the data offset into the pack data section.
		/// </summary>
		/// <value>The stream offset.</value>
		public long DataOffset { get; set; }

		/// <summary>
		/// Gets the name (AKA the path) for the entry.
		/// </summary>
		/// <value>The name.</value>
		/// <exception cref="System.ArgumentOutOfRangeException"></exception>
		public string Name
		{
			get { return _name; }
			set
			{
				if (MaxNameLength > 0 && value.Length > MaxNameLength)
					throw new ArgumentOutOfRangeException();
				_name = value;
			}
		}

		public DateTime CreatedDate { get; set; }
		public DateTime ModifiedDate { get; set; }
		public DateTime AccessDate { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PackEntry"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="cryptoSeed">The crypto seed.</param>
		/// <param name="isCompressed">if set to <c>true</c> [is compressed].</param>
		/// <param name="sizeInPack">The size in pack.</param>
		/// <param name="decompressedSize">Size of the decompressed.</param>
		/// <param name="dataOffset">The stream offset.</param>
		/// <param name="createdDate"></param>
		/// <param name="modifyDate"></param>
		/// <param name="accessDate"></param>
		public PackEntry(string name, int cryptoSeed, bool isCompressed, int sizeInPack, int decompressedSize, long dataOffset,
			DateTime createdDate, DateTime modifyDate, DateTime accessDate)
		{
			MaxNameLength = -1;

			DataOffset = dataOffset;
			DecompressedSize = decompressedSize;
			SizeInPack = sizeInPack;
			IsCompressed = isCompressed;
			CryptoSeed = cryptoSeed;
			Name = name;
			CreatedDate = createdDate;
			ModifiedDate = modifyDate;
			AccessDate = accessDate;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PackEntry"/> class.
		/// </summary>
		/// <param name="info">The information.</param>
		/// <param name="name">The name.</param>
		/// <param name="maxNameLength">Maximum length of the name.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">maxNameLength</exception>
		internal PackEntry(PackageItemInfo info, string name, int maxNameLength)
			: this(name, info.Seed, info.IsCompressed, info.CompressedSize, info.DecompressedSize, info.Offset,
			DateTime.FromFileTime(info.CreationTime), DateTime.FromFileTime(info.ModifiedTime2), DateTime.FromFileTime(info.LastAccessTime))
		{
			if (name.Length > maxNameLength)
				throw new ArgumentOutOfRangeException("maxNameLength");

			MaxNameLength = maxNameLength;
		}
	}
}
