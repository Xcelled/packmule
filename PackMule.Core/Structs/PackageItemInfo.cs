using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PackMule.Core.Structs
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct PackageItemInfo
	{
		public int Seed;
		public int Zero;
		[MarshalAs(UnmanagedType.U4)]
		public int Offset;
		public int CompressedSize;
		public int DecompressedSize;
		public int IsCompressed;
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(FiletimeToDateTimeMarshaler))]
		public long Ft1;
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(FiletimeToDateTimeMarshaler))]
		public long Ft2;
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(FiletimeToDateTimeMarshaler))]
		public long Ft3;
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(FiletimeToDateTimeMarshaler))]
		public long Ft4;
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(FiletimeToDateTimeMarshaler))]
		public long Ft5;
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(FiletimeToDateTimeMarshaler))]
		public long Ft6;
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(FiletimeToDateTimeMarshaler))]
		public long Ft7;
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(FiletimeToDateTimeMarshaler))]
		public long Ft8;
	}
}
