using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PackMule.Core.Structs
{
	public static class StructExtensions
	{
		public static byte[] GetBytes<T>(this T obj) where T : struct
		{
			var size = Marshal.SizeOf(obj);
			var arr = new byte[size];
			var ptr = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(obj, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
			Marshal.FreeHGlobal(ptr);

			return arr;
		}

		public static T GetStruct<T>(this byte[] arr, int offset) where T : struct
		{
			var handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
			var stuff = (T)Marshal.PtrToStructure(IntPtr.Add(handle.AddrOfPinnedObject(), offset), typeof(T));
			handle.Free();
			return stuff;
		}

		public static T ReadFromStream<T>(this Stream file) where T : struct
		{
			var buff = new byte[Marshal.SizeOf(typeof(T))];
			file.Read(buff, 0, buff.Length);
			return GetStruct<T>(buff, 0);
		}

		public static void WriteToStream<T>(this T obj, Stream output) where T : struct
		{
			var b = GetBytes(obj);

			output.Write(b, 0, b.Length);
		}

		public static int SizeOf<T>() where T : struct
		{
			return Marshal.SizeOf(typeof(T));
		}
	}
}
