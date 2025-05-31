using System;
using System.Runtime.InteropServices;

namespace Bau.Libraries.LibSystem.API
{
	/// <summary>
	///		Api de acceso a ShellFileOperation (SHFileOperation)
	/// </summary>
	internal static class ShellFileOperationApi
	{	
		// Constantes para operaciones
		internal const int FO_DELETE = 3;
		internal const int FOF_ALLOWUNDO = 0x40;
		internal const int FOF_NOCONFIRMATION = 0x10; // No pregunta al usuario

		// Estructuras
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=1)]
		internal struct SHFILEOPSTRUCT
		{
			public IntPtr hwnd;
			[MarshalAs(UnmanagedType.U4)] public int wFunc;
			public string pFrom;
			public string pTo;
			public short fFlags;
			[MarshalAs(UnmanagedType.Bool)] public bool fAnyOperationsAborted;
			public IntPtr hNameMappings;
			public string lpszProgressTitle;
		}

		// Funciones
		[DllImport("shell32.dll", CharSet=CharSet.Auto)]
		internal static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);
	}
}
