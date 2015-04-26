using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PackMule.Core.Structs
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FileHeader
	{
		[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
		public byte[] Signature;
		public uint Revision;
		public int EntryCount;
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FiletimeToDateTimeMarshaler))]
		public long Ft1;
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(FiletimeToDateTimeMarshaler))]
		public long Ft2;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 480)]
		public string Path;
	}
}
