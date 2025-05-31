using System;

namespace Bau.Libraries.LibSystem.Processes
{
	/// <summary>
	///		Procesador de comandos (MS-Dos)
	/// </summary>
	public class CmdProcessor
	{
		/// <summary>
		///		Ejecuta un lote de MSDos
		/// </summary>
		public void ExecuteMsDos(string command, TimeSpan timeSpan, out string error)
		{
			string fileName = System.IO.Path.GetTempFileName() + ".bat";

				// Inicializa los argumentos de salida
				error = string.Empty;
				// Ejecuta el comando
				try
				{
					SystemProcessHelper processor = new SystemProcessHelper();

						// Graba el texto en el archivo
						Files.HelperFiles.SaveTextFile(fileName, command, System.Text.Encoding.ASCII);
						// Ejecuta el proceso
						processor.ExecuteApplication("cmd.exe", "/c \"" + fileName + "\"", 
												     true, TimeSpan.FromMinutes(Math.Max(timeSpan.TotalMinutes, 1)));
						// Recoge los errores
						if (!string.IsNullOrEmpty(processor.ExecutionError))
							error = $"Shell error: {processor.ExecutionError}";
						else if (!string.IsNullOrEmpty(processor.ProcessErrorOutput))
							error = $"Shell command error: {processor.ProcessErrorOutput}";
				}
				catch (Exception exception)
				{
					error = $"Error when execute command MS-DOS {exception.Message}";
				}
				// Elimina los archivos intermedios
				Files.HelperFiles.KillFile(fileName);
		}
	}
}
