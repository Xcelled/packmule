using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PackMule.Core.Structs
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct PackageHeader
	{
		public int EntryCount;
		public int InfoHeaderSize;
		public int BlankSize;
		public int DataSectionSize;
		[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
		public byte[] Zero;
	}
}
