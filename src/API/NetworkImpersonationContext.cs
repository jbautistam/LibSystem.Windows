using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Bau.Libraries.LibSystem.API
{
	/// <summary>
	/// Clase para impersonar sobre un recurso de Windows
	/// </summary>
	public class NetworkImpersonationContext : IDisposable
	{ 
		// Constantes privadas
		private const int LOGON32_PROVIDER_DEFAULT = 0;
		private const int LOGON32_LOGON_INTERACTIVE = 2; // Este parámetro indica que LogonUser cree un token principal
		// Variables privadas
		private readonly WindowsImpersonationContext impersonationContext;
		private readonly SafeTokenHandle tokenHnd;

		/// <summary>
		/// Api para conectar con un usuario a un dominio
		/// </summary>
		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword,
											 int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

		public NetworkImpersonationContext(string domain, string userName, string password)
		{	
			if (LogonUser(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out tokenHnd))
				using (WindowsIdentity windowIdentity = new WindowsIdentity(tokenHnd.DangerousGetHandle()))
					{ impersonationContext = windowIdentity.Impersonate();
					}
			else
				throw new AccessViolationException($"No se puede entrar al dominio (error de Windows {Marshal.GetLastWin32Error()})");
		}

		/// <summary>
		///		Libera los recursos
		/// </summary>
		protected virtual void Dispose(bool isDisposing)
		{	
			if (!IsDisposed)
			{ 
				// Libera los recursos
				if (isDisposing)
					impersonationContext?.Dispose();
				// Libera el handle en cualquier caso
				tokenHnd?.Dispose();
				// Indica que se ha liberado
				IsDisposed = true;
			}
		}

		/// <summary>
		///		Libera la memoria
		/// </summary>
		public void Dispose()
		{	
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///		Indica si se ha liberado el objeto
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		///		Destructor
		/// </summary>
		~NetworkImpersonationContext()
		{	
			Dispose(false);
		}
	}
}