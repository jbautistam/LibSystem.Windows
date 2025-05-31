using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace Bau.Libraries.LibSystem.API
{
	/// <summary>
	/// Token seguro
	/// </summary>
	public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		/// <summary>
		/// API para cerrar un handle
		/// </summary>
		[DllImport("kernel32.dll")]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[SuppressUnmanagedCodeSecurity]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseHandle(IntPtr handle);

		private SafeTokenHandle() : base(true) {}

		/// <summary>
		/// Libera el handle
		/// </summary>
		protected override bool ReleaseHandle()
		{	
			return CloseHandle(handle);
		}
	}
}