using System.Runtime.InteropServices;

namespace PackMule.Core.Structs
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct PackageHeader
	{
		public int EntryCount;
		public int InfoHeaderSize;
		public int BlankSize; // The blank is an area after the header, possibly to allow appending to the pack
		public uint DataSectionSize;
		[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
		public byte[] Zero;
	}
}
