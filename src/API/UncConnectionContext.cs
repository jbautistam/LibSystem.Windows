using System;
using System.Runtime.InteropServices;

namespace Bau.Libraries.LibSystem.API
{
	/// <summary>
	/// Clase de contexto para una conección a un recurso UNC
	/// </summary>
	public class UncConnectionContext : IDisposable
	{ 
		// Constantes de estado de recurso
		private const int RESOURCE_CONNECTED = 0x00000001;
		private const int RESOURCE_GLOBALNET = 0x00000002;
		private const int RESOURCE_REMEMBERED = 0x00000003;
		// Constantes de tipo de recurso
		private const int RESOURCETYPE_ANY = 0x00000000;
		private const int RESOURCETYPE_DISK = 0x00000001;
		private const int RESOURCETYPE_PRINT = 0x00000002;
		// Constantes de tipo de visualización
		private const int RESOURCEDISPLAYTYPE_GENERIC = 0x00000000;
		private const int RESOURCEDISPLAYTYPE_DOMAIN = 0x00000001;
		private const int RESOURCEDISPLAYTYPE_SERVER = 0x00000002;
		private const int RESOURCEDISPLAYTYPE_SHARE = 0x00000003;
		private const int RESOURCEDISPLAYTYPE_FILE = 0x00000004;
		private const int RESOURCEDISPLAYTYPE_GROUP = 0x00000005;
		// Constantes de uso de recurso
		private const int RESOURCEUSAGE_CONNECTABLE = 0x00000001;
		private const int RESOURCEUSAGE_CONTAINER = 0x00000002;
		// Constantes de modo de conexión
		private const int CONNECT_INTERACTIVE = 0x00000008;
		private const int CONNECT_PROMPT = 0x00000010;
		private const int CONNECT_REDIRECT = 0x00000080;
		private const int CONNECT_UPDATE_PROFILE = 0x00000001;
		private const int CONNECT_COMMANDLINE = 0x00000800;
		private const int CONNECT_CMD_SAVECRED = 0x00001000;
		// Constante de unidad local
		private const int CONNECT_LOCALDRIVE = 0x00000100;
		// Constantes de error
		public const int NO_ERROR = 0;
		public const int ERROR_ACCESS_DENIED = 5;
		public const int ERROR_ALREADY_ASSIGNED = 85;
		public const int ERROR_BAD_DEVICE = 1200;
		public const int ERROR_BAD_NET_NAME = 67;
		public const int ERROR_BAD_PROVIDER = 1204;
		public const int ERROR_CANCELLED = 1223;
		public const int ERROR_EXTENDED_ERROR = 1208;
		public const int ERROR_INVALID_ADDRESS = 487;
		public const int ERROR_INVALID_PARAMETER = 87;
		public const int ERROR_INVALID_PASSWORD = 1216;
		public const int ERROR_MORE_DATA = 234;
		public const int ERROR_NO_MORE_ITEMS = 259;
		public const int ERROR_NO_NET_OR_BAD_PATH = 1203;
		public const int ERROR_NO_NETWORK = 1222;
		public const int ERROR_BAD_PROFILE = 1206;
		public const int ERROR_CANNOT_OPEN_PROFILE = 1205;
		public const int ERROR_DEVICE_IN_USE = 2404;
		public const int ERROR_NOT_CONNECTED = 2250;
		public const int ERROR_OPEN_FILES  = 2401;

		// Api
		/// <summary>
		///		Función para crear una conexión sobre una ruta UNC
		/// </summary>
		[DllImport("Mpr.dll")] 
		private static extern int WNetUseConnection(IntPtr howner, NETRESOURCE lpNetResource, string lpPassword, string lpUserID,
													int dwFlags, string lpAccessName, string lpBufferSize, string lpResult);

		/// <summary>
		///		Función para cerrar una conexión sobre una ruta UNC
		/// </summary>
		[DllImport("Mpr.dll")] 
		private static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);

		/// <summary>
		///		Estructura con los datos de red
		/// </summary>
		[StructLayout(LayoutKind.Sequential)] 
		private class NETRESOURCE
			{ 
				public int dwScope = 0;
				public int dwType = 0;
				public int dwDisplayType = 0;
				public int dwUsage = 0;
				public string lpLocalName = "";
				public string lpRemoteName = "";
				public string lpComment = "";
				public string lpProvider = "";
			}

		public UncConnectionContext(string remoteUnc, string userName, string password, bool showPromptUser)
		{	
			NETRESOURCE netResource = new NETRESOURCE();

				// Guarda el directorio UNC
				RemoteUnc = remoteUnc;
				// Inicializa los datos del recurso
				netResource.dwType = RESOURCETYPE_DISK;
				netResource.lpRemoteName = RemoteUnc;
				// netResource.lpLocalName = "F:";
				// Inicializa el código de error
				LastErrorCode = NO_ERROR;
				// Conecta a la unidad UNC	
				if (showPromptUser) 
					LastErrorCode = WNetUseConnection(IntPtr.Zero, netResource, "", "", 
													  CONNECT_INTERACTIVE | CONNECT_PROMPT, null, null, null);
				else 
					LastErrorCode = WNetUseConnection(IntPtr.Zero, netResource, password, userName, 0, null, null, null);
				// Si ha habido algún error, lanza la excepción
				if (HasError)
					throw new UnauthorizedAccessException($"No se puede entrar al directorio UNC. {LastErrorCode} - {LastErrorMessage}");
				// Indica que se ha conectado
				IsConnected = true;
		}

		/// <summary>
		///		Desconecta del directorio UNC
		/// </summary>
		public bool Disconnect() 
		{ 
			// Vacía el último error
			LastErrorCode = NO_ERROR;
			// Llama al API de desconexión
			if (IsConnected)
			{ 
				// Desconecta
				LastErrorCode = WNetCancelConnection2(RemoteUnc, CONNECT_UPDATE_PROFILE, false);
				// Indica si se ha desconectado
				if (!HasError)
					IsConnected = false;
			}
			// Devuelve el valor que indica si se ha desconectado correctamente
			return !HasError;
		}

		/// <summary>
		///		Libera la memoria
		/// </summary>
		protected virtual void Dispose(bool isDisposing)
		{ 
			if (!IsDisposed)
			{	
				// Desconecta la unidad
				if (isDisposing)
					Disconnect();
				// Indica que se ha liberado la memoria
				IsDisposed = true;
			}
		}

		/// <summary>
		///		Libera la memoria
		/// </summary>
		public void Dispose()
		{	
			// Libera los recusos
			Dispose(true);
			// Destruye el objeto
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///		Destructor de la clase
		/// </summary>
		~UncConnectionContext()
		{		
			Dispose(false);
		}

		/// <summary>
		///		Directorio al que se ha conectado
		/// </summary>
		public string RemoteUnc { get; }

		/// <summary>
		///		Indica si se ha conectado al directorio UNC
		/// </summary>
		public bool IsConnected { get; private set; }

		/// <summary>
		///		Indica si se ha liberado el objeto
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		///		Ultimo código de error
		/// </summary>
		public int LastErrorCode { get; private set; }

		/// <summary>
		///		Indica si ha habido algún error
		/// </summary>
		public bool HasError 
		{ get { return LastErrorCode != NO_ERROR; }
		}

		/// <summary>
		///		Ultimo mensaje de error
		/// </summary>
		public string LastErrorMessage
		{ 
			get
			{ 
				switch (LastErrorCode)
				{	
					case NO_ERROR:
						return "No error";
					case ERROR_ACCESS_DENIED:
						return "Error: Access Denied";
					case ERROR_ALREADY_ASSIGNED:
						return "Error: Already Assigned";
					case ERROR_BAD_DEVICE:
						return "Error: Bad Device";
					case ERROR_BAD_NET_NAME:
						return "Error: Bad Net Name";
					case ERROR_BAD_PROVIDER:
						return "Error: Bad Provider";
					case ERROR_CANCELLED:
						return "Error: Cancelled";
					case ERROR_EXTENDED_ERROR:
						return "Error: Extended Error";
					case ERROR_INVALID_ADDRESS:
						return "Error: Invalid Address";
					case ERROR_INVALID_PARAMETER:
						return "Error: Invalid Parameter";
					case ERROR_INVALID_PASSWORD:
						return "Error: Invalid Password";
					case ERROR_MORE_DATA:
						return "Error: More Data";
					case ERROR_NO_MORE_ITEMS:
						return "Error: No More Items";
					case ERROR_NO_NET_OR_BAD_PATH:
						return "Error: No Net Or Bad Path";
					case ERROR_NO_NETWORK:
						return "Error: No Network";
					case ERROR_BAD_PROFILE:
						return "Error: Bad Profile";
					case ERROR_CANNOT_OPEN_PROFILE:
						return "Error: Cannot Open Profile";
					case ERROR_DEVICE_IN_USE:
						return "Error: Device In Use";
					case ERROR_NOT_CONNECTED:
						return "Error: Not Connected"; 
					case ERROR_OPEN_FILES:
						return "Error: Open Files";
					default:
						return "Error: Unexpected";
				}
			}
		}
	}
}
