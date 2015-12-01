using System.Runtime.InteropServices;

namespace PackMule.Core.Structs
{
	/// <summary>
	/// Struct FileHeader
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct FileHeader
	{
		/// <summary>
		/// The signature of the pack file
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
		public byte[] Signature;
		/// <summary>
		/// The revision
		/// </summary>
		public uint Revision;
		/// <summary>
		/// The entry count
		/// </summary>
		public int EntryCount;
		/// <summary>
		/// The creation date?
		/// </summary>
		public long Ft1;
		/// <summary>
		/// The modified date?
		/// </summary>
		public long Ft2;
		/// <summary>
		/// The path that the file is "rooted" at
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 480)]
		public string Path;
	}
}
