using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;
using PackMule.Core.Crypto;
using PackMule.Core.Structs;

namespace PackMule.Core
{
	/// <summary>
	/// Class PackWriter.
	/// </summary>
	public class PackWriter : IDisposable
	{
		/// <summary>
		/// The temporary storage for the body stream
		/// </summary>
		private readonly TempFileScope _tempOutput;
		/// <summary>
		/// The _body stream
		/// </summary>
		private readonly FileStream _bodyStream;
		/// <summary>
		/// The _entries
		/// </summary>
		private readonly List<PackEntry> _entries = new List<PackEntry>();

		/// <summary>
		/// Gets the revision.
		/// </summary>
		/// <value>The revision.</value>
		public int Revision { get; private set; }
		/// <summary>
		/// Gets the root.
		/// </summary>
		/// <value>The root.</value>
		public string Root { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PackWriter"/> class.
		/// </summary>
		/// <param name="revision">The revision.</param>
		/// <param name="root">The root.</param>
		public PackWriter(int revision, string root)
		{
			Root = root;
			Revision = revision;
			_tempOutput = new TempFileScope();
			_bodyStream = File.Open(_tempOutput.Filename, FileMode.Create);
		}

		/// <summary>
		/// Writes the specified stream into the packfile.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="fileName">Name of the file.</param>
		public void Write(Stream s, string fileName)
		{
			Write(s, fileName, Revision);
		}

		/// <summary>
		/// Writes the specified stream into the packfile.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="revision">The revision.</param>
		public void Write(Stream s, string fileName, int revision)
		{
			Write(s, fileName, revision, true);
		}

		/// <summary>
		/// Writes the specified stream into the packfile.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="compress">if set to <c>true</c> [compress].</param>
		public void Write(Stream s, string fileName, bool compress)
		{
			Write(s, fileName, Revision, compress);
		}

		/// <summary>
		/// Writes the specified stream into the packfile.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="revision">The revision.</param>
		/// <param name="compress">if set to <c>true</c> [compress].</param>
		public void Write(Stream s, string fileName, int revision, bool compress)
		{
			Write(s, fileName, revision, compress, DateTime.Now, DateTime.Now, DateTime.Now);
		}

		/// <summary>
		/// Writes the specified sstream into the packfile.
		/// </summary>
		/// <param name="s">The s.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="seed">The seed.</param>
		/// <param name="compress"></param>
		/// <param name="createDate"></param>
		/// <param name="modifyDate"></param>
		/// <param name="AccessDate"></param>
		public void Write(Stream s, string fileName, int seed, bool compress, DateTime createDate, DateTime modifyDate, DateTime AccessDate)
		{
			var outputStart = _bodyStream.Position;
			var inputStart = s.Position;

			using (var mc = new PackCryptoStream(new ZlibStream(s, CompressionMode.Compress), seed))
			{
				mc.CopyTo(_bodyStream);
			}

			_entries.Add(new PackEntry(fileName, seed, compress, (int)(_bodyStream.Position - outputStart),
				(int)(s.Position - inputStart), outputStart, createDate, modifyDate, AccessDate));
		}

		/// <summary>
		/// Outputs the pack file to the given stream.
		/// </summary>
		/// <param name="dst">The DST.</param>
		public void SaveTo(Stream dst)
		{
			var head = new FileHeader
			{
				Signature = PackCommon.Header,
				Revision = (uint)Revision,
				EntryCount = _entries.Count,
				Path = Root,
				Ft1 = DateTime.UtcNow.ToFileTimeUtc(),
				Ft2 = DateTime.UtcNow.ToFileTimeUtc()
			};

			var pkgHead = new PackageHeader
			{
				EntryCount = _entries.Count,
				InfoHeaderSize = 0,
				DataSectionSize = (uint)_bodyStream.Position,
				BlankSize = 0,
				Zero = new byte[16]
			};

			var infos = new List<Tuple<byte[], PackageItemInfo>>();

			foreach (var e in _entries)
			{
				var info = new PackageItemInfo()
				{
					CompressedSize = e.SizeInPack,
					DecompressedSize = e.DecompressedSize,
					IsCompressed = e.IsCompressed,
					Offset = (uint)e.DataOffset,
					Seed = e.CryptoSeed,
					CreationTime = e.CreatedDate.ToFileTime(),
					CreationTime2 = e.CreatedDate.ToFileTime(),
					ModifiedTime = e.ModifiedDate.ToFileTime(),
					ModifiedTime2 = e.ModifiedDate.ToFileTime(),
					LastAccessTime = e.AccessDate.ToFileTime()
				};

				var bytes = PackCommon.EncodeName(e.Name);

				infos.Add(Tuple.Create(bytes, info));
				pkgHead.InfoHeaderSize += bytes.Length + StructUtil.SizeOf<PackageItemInfo>();
			}

			head.WriteToStream(dst);
			pkgHead.WriteToStream(dst);
			foreach (var i in infos)
			{
				dst.Write(i.Item1, 0, i.Item1.Length);
				i.Item2.WriteToStream(dst);
			}

			_bodyStream.Position = 0;
			_bodyStream.CopyTo(dst);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
				_tempOutput.Dispose();
				_bodyStream.Dispose();
			}
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="PackWriter"/> class.
		/// </summary>
		~PackWriter()
		{
			Dispose(false);
		}
	}
}
