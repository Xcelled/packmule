using System.Runtime.InteropServices;

namespace PackMule.Core.Structs
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct PackageItemInfo
	{
		public int Seed;
		public int Zero;
		[MarshalAs(UnmanagedType.U4)]
		public uint Offset;
		public int CompressedSize;
		public int DecompressedSize;
		[MarshalAs(UnmanagedType.Bool)]
		public bool IsCompressed;
		public long CreationTime;
		public long CreationTime2;
		public long LastAccessTime;
		public long ModifiedTime;
		public long ModifiedTime2;
	}
}
