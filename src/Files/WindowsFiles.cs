using System;

namespace Bau.Libraries.LibSystem.Files
{
	/// <summary>
	///		Clase de ayuda para tratamiento de procesos especiales con archivos en Windows
	/// </summary>
	public static class WindowsFiles
	{
		/// <summary>
		///		Elimina el archivo y lo guarda en la papelera de reciclaje
		/// </summary>
		public static bool KillFileForRecycle(string fileName)
		{ 
			try
			{ 
				API.ShellFileOperationApi.SHFILEOPSTRUCT shf = new API.ShellFileOperationApi.SHFILEOPSTRUCT();

					// Inicializa las propiedades
					shf.wFunc = API.ShellFileOperationApi.FO_DELETE;
					shf.fFlags = API.ShellFileOperationApi.FOF_ALLOWUNDO | API.ShellFileOperationApi.FOF_NOCONFIRMATION;
					shf.pFrom = fileName;
					// Ejecuta el método
					API.ShellFileOperationApi.SHFileOperation(ref shf);
					// Indica que se ha borrado correctamente
					return true;
			}
			catch
			{ 
				return false;
			}
		}

		/// <summary>
		///		Abre un documento utilizando el shell
		/// </summary>
		public static void OpenDocumentShell(string executable, string fileName)
		{ 
			new Processes.SystemProcessHelper().ExecuteApplication(executable, fileName);
		}
		
		/// <summary>
		///		Abre el documento utilizando el shell
		/// </summary>
		public static void OpenDocumentShell(string fileName)
		{	
			OpenDocumentShell(null, fileName);
		}

		/// <summary>
		///		Abre el explorador con un archivo
		/// </summary>
		public static void OpenBrowser(string url)
		{ 
			OpenDocumentShell("iexplore.exe", url);
		}

		/// <summary>
		///		Ejecuta una aplicación
		/// </summary>
		public static void ExecuteApplication(string executable)
		{ 
			OpenDocumentShell(executable, null);
		}

		/// <summary>
		///		Ejecuta una aplicación
		/// </summary>
		public static void ExecuteApplication(string executable, string arguments) 
		{ 
			OpenDocumentShell(executable, arguments);
		}
	}
}
