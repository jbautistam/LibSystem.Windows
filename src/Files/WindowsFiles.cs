using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bau.Libraries.LibSystem.Files;

/// <summary>
///		Clase de ayuda para tratamiento de procesos especiales con archivos en Windows
/// </summary>
public static class WindowsFiles
{
	/// <summary>
	///		Abre un documento utilizando el shell
	/// </summary>
	public static void OpenDocumentShell(string? executable, string? fileName)
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
		if (!string.IsNullOrWhiteSpace(url))
			try
			{
				Process.Start(url);
			}
			catch
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					Process.Start("xdg-open", url);
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					Process.Start("open", url);
			}
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
