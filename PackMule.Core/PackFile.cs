using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zlib;
using PackMule.Core.Crypto;
using PackMule.Core.Structs;

namespace PackMule.Core
{
	/// <summary>
	/// Represents a packfile.
	/// This class supports reading and modification of metadata, but NOT content.
	/// </summary>
	public class PackFile : ICollection<PackEntry>, IDisposable
	{
		/// <summary>
		/// The underlying stream of the pack file
		/// </summary>
		private readonly Stream _source;
		/// <summary>
		/// The start of the data section
		/// </summary>
		private long _dataStart;
		/// <summary>
		/// The internal entries container
		/// </summary>
		private Dictionary<string, PackEntry> _entries;

		/// <summary>
		/// Gets the normalized names.
		/// </summary>
		/// <value>The normalized names.</value>
		public IEnumerable<string> NormalizedNames { get { return _entries.Keys; } }

		/// <summary>
		/// Gets or sets the revision of the packfile.
		/// </summary>
		/// <value>The revision.</value>
		public long Revision { get; set; }
		/// <summary>
		/// Gets or sets the root of the packfile.
		/// </summary>
		/// <value>The root.</value>
		public string Root { get; set; }
		/// <summary>
		/// Gets or sets the created date of the packfile.
		/// </summary>
		/// <value>The created.</value>
		public DateTime Created { get; set; }
		/// <summary>
		/// Gets or sets the modified date of the packfile.
		/// </summary>
		/// <value>The modified.</value>
		public DateTime Modified { get; set; }

		public string Name { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PackFile"/> class.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="name">Name of the packfile</param>
		/// <exception cref="System.ArgumentException">Source must be seekable</exception>
		public PackFile(Stream source, string name)
		{
			if (!source.CanSeek)
				throw new ArgumentException("Source must be seekable");

			_source = source;
			Name = name;

			Read();
		}

		/// <summary>
		/// Reads this instance.
		/// </summary>
		private void Read()
		{
			_source.Position = 0;
			_entries = new Dictionary<string, PackEntry>();

			var fileHeader = ReadFileHeader();
			var packageHeader = ReadPackageHeader();

			Revision = fileHeader.Revision;
			Root = fileHeader.Path;
			Created = DateTime.FromFileTime(fileHeader.Ft1);
			Modified = DateTime.FromFileTime(fileHeader.Ft2);

			ReadPackageInfos(packageHeader);

			_dataStart = _source.Position;
		}

		/// <summary>
		/// Reads the package infos.
		/// </summary>
		/// <param name="packageHeader">The package header.</param>
		/// <exception cref="System.Exception">Entry ' + name + ' is corrupted!</exception>
		private void ReadPackageInfos(PackageHeader packageHeader)
		{
			var infoHeader = new byte[packageHeader.InfoHeaderSize];

			_source.Read(infoHeader, 0, infoHeader.Length);

			var ptr = 0;

			for (var i = 0; i < packageHeader.EntryCount; i++)
			{
				int nameSectionLength;
				if (infoHeader[ptr] < 4)
					nameSectionLength = 0x10 * (infoHeader[ptr] + 1);
				else if (infoHeader[ptr] == 4)
					nameSectionLength = 0x60;
				else
					nameSectionLength = BitConverter.ToInt32(infoHeader, ptr + 1) + 5;

				var lengthSize = (infoHeader[ptr] > 4 ? 5 : 1);

				var nameStart = ptr + lengthSize;
				var maxNameLength = nameSectionLength - lengthSize - 1; // 1 for the trailing \0, which we don't care about.

				var name = Encoding.UTF8.GetString(infoHeader, nameStart, maxNameLength).TrimEnd('\0');

				ptr += nameSectionLength;

				var info = infoHeader.GetStruct<PackageItemInfo>(ptr);

				if (info.Zero != 0)
					throw new Exception("Entry '" + name + "' is corrupted!");

				_entries[name.NormalizePath()] = new PackEntry(info, name, maxNameLength);

				ptr += StructUtil.SizeOf<PackageItemInfo>();
			}
		}

		/// <summary>
		/// Reads the package header.
		/// </summary>
		/// <returns>PackageHeader.</returns>
		/// <exception cref="System.IO.InvalidDataException">Pack file is corrupted!</exception>
		private PackageHeader ReadPackageHeader()
		{
			var ph = _source.ReadFromStream<PackageHeader>();
			if (!ph.Zero.SequenceEqual(Enumerable.Repeat<byte>(0, ph.Zero.Length)))
				throw new InvalidDataException("Pack file is corrupted!");

			return ph;
		}

		/// <summary>
		/// Reads the file header.
		/// </summary>
		/// <returns>FileHeader.</returns>
		/// <exception cref="System.IO.InvalidDataException">Not a valid pack file!</exception>
		private FileHeader ReadFileHeader()
		{
			var fh = _source.ReadFromStream<FileHeader>();

			if (!fh.Signature.SequenceEqual(PackCommon.Header))
				throw new InvalidDataException("Not a valid pack file!");

			return fh;
		}

		/// <summary>
		/// Gets the <see cref="PackEntry"/> with the specified path. The path is normalized.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>PackEntry.</returns>
		public PackEntry this[string path]
		{
			get { return _entries[path.NormalizePath()]; }
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		public void Add(PackEntry item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <exception cref="System.NotSupportedException"></exception>
		public void Clear()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
		public bool Contains(PackEntry item)
		{
			return _entries.ContainsValue(item);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		public void CopyTo(PackEntry[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get { return _entries.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly
		{
			get { return true; }
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		/// <exception cref="System.NotSupportedException"></exception>
		public bool Remove(PackEntry item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
		public IEnumerator<PackEntry> GetEnumerator()
		{
			return _entries.Values.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_entries.Values).GetEnumerator();
		}

		/// <summary>
		/// Extracts the specified entry.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <returns>Stream.</returns>
		public Stream Extract(PackEntry entry)
		{
			Stream s = new SegmentStream(_source, entry.DataOffset + _dataStart, entry.SizeInPack);

			s = new PackCryptoStream(s, entry.CryptoSeed);

			if (entry.IsCompressed)
				s = new ZlibStream(s, CompressionMode.Decompress);

			return s;
		}

		/// <summary>
		/// Saves this instance, aka overwrites the file header.
		/// </summary>
		public void Save()
		{
			var head = new FileHeader
			{
				Signature = PackCommon.Header,
				Revision = (uint)Revision,
				EntryCount = Count,
				Path = Root,
				Ft1 = Created.ToFileTime(),
				Ft2 = Modified.ToFileTime()
			};

			var pkgHead = new PackageHeader
			{
				EntryCount = Count,
				InfoHeaderSize = 0,
				DataSectionSize = (uint)(_source.Length - _dataStart),
				BlankSize = 0,
				Zero = new byte[16]
			};

			var infos = new List<Tuple<byte[], PackageItemInfo>>();

			foreach (var e in _entries.Values)
			{
				var info = new PackageItemInfo
				{
					CompressedSize = e.SizeInPack,
					DecompressedSize = e.DecompressedSize,
					IsCompressed = e.IsCompressed,
					Offset = (uint)e.DataOffset,
					Seed = e.CryptoSeed,
					CreationTime = e.CreatedDate.ToFileTime(),
					CreationTime2 = e.CreatedDate.ToFileTime(),
					LastAccessTime = e.AccessDate.ToFileTime(),
					ModifiedTime = e.ModifiedDate.ToFileTime(),
					ModifiedTime2 = e.ModifiedDate.ToFileTime(),
				};

				var bytes = PackCommon.EncodeName(e.Name.PadRight(e.MaxNameLength, '\0'));

				infos.Add(Tuple.Create(bytes, info));
				pkgHead.InfoHeaderSize += bytes.Length + StructUtil.SizeOf<PackageItemInfo>();
			}

			pkgHead.BlankSize = (int)(_dataStart -
			                    (StructUtil.SizeOf<FileHeader>() + StructUtil.SizeOf<PackageHeader>() +
			                     pkgHead.InfoHeaderSize));

			pkgHead.InfoHeaderSize += pkgHead.BlankSize;

			_source.Position = 0;
			head.WriteToStream(_source);
			pkgHead.WriteToStream(_source);
			foreach (var i in infos)
			{
				_source.Write(i.Item1, 0, i.Item1.Length);
				i.Item2.WriteToStream(_source);
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_source.Dispose();
		}
	}
}
