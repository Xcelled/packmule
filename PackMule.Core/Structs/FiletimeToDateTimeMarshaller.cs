using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PackMule.Core.Structs
{
	class FiletimeToDateTimeMarshaler : ICustomMarshaler
	{
		/// <summary>
		/// Converts the unmanaged data to managed data.
		/// </summary>
		/// <returns>
		/// An object that represents the managed view of the COM data.
		/// </returns>
		/// <param name="pNativeData">A pointer to the unmanaged data to be wrapped. </param>
		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
			return DateTime.FromFileTime(Marshal.ReadInt64(pNativeData));
		}

		/// <summary>
		/// Converts the managed data to unmanaged data.
		/// </summary>
		/// <returns>
		/// A pointer to the COM view of the managed object.
		/// </returns>
		/// <param name="managedObj">The managed object to be converted. </param>
		public IntPtr MarshalManagedToNative(object managedObj)
		{
			var dt = (DateTime)managedObj;

			var pNativeData = Marshal.AllocHGlobal(sizeof(long));

			Marshal.WriteInt64(pNativeData, dt.ToFileTime());

			return pNativeData;
		}

		/// <summary>
		/// Performs necessary cleanup of the unmanaged data when it is no longer needed.
		/// </summary>
		/// <param name="pNativeData">A pointer to the unmanaged data to be destroyed. </param>
		public void CleanUpNativeData(IntPtr pNativeData)
		{
			Marshal.FreeHGlobal(pNativeData);
		}

		/// <summary>
		/// Performs necessary cleanup of the managed data when it is no longer needed.
		/// </summary>
		/// <param name="managedObj">The managed object to be destroyed. </param>
		public void CleanUpManagedData(object managedObj)
		{
			
		}

		/// <summary>
		/// Returns the size of the native data to be marshaled.
		/// </summary>
		/// <returns>
		/// The size, in bytes, of the native data.
		/// </returns>
		public int GetNativeDataSize()
		{
			return sizeof(long);
		}
	}
}
