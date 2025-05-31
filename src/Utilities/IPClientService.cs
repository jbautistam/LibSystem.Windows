using System.Net;

namespace Bau.Libraries.LibSystem.Utilities;

/// <summary>
/// Servicio para obtener las IPs de cliente
/// </summary>
public class IPClientService
{
	/// <summary>
	/// Obtiene la IP del cliente
	/// </summary>
	public string GetClientIP(HttpContext context)
	{ 
		string ip = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

			// Si se ha obtenido la IP, se obtiene la primera (puede que haya varias separadas por ,),
			// En caso contrario recogemos la IP de la variable de sesión
			if (!string.IsNullOrEmpty(ip))
				ip = ip.Split(',')[0];
			else
				ip = context.Request.ServerVariables["REMOTE_ADDR"];
			// Devuelve la IP
			return ip;
	}

	/// <summary>
	///		Obtiene la IP de la red local
	/// </summary>
	public string GetLanIP(System.Net.Sockets.AddressFamily family = System.Net.Sockets.AddressFamily.InterNetwork)
	{ 
		IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());

			// Recorre las direcciones buscando el tipo
			if (ipHost?.AddressList != null && ipHost.AddressList.Length > 0)
				foreach (IPAddress ipAddress in ipHost.AddressList)
					if (ipAddress.AddressFamily == family)
						return ipAddress.ToString();
			// Si ha llegado hasta aquí es porque no ha encontrado nada
			return null;
	}

	/// <summary>
	///		Obtiene el array de direcciones IP de cliente (todas las externas e internas)
	/// </summary>
	public List<string> GetArrayClientIP(HttpContext context, bool includeInternal)
	{ 
		List<string> clientIps = new List<string>();

			// Indica si se deben incluir las variables internas
			if (includeInternal)
			{	
				// Añade las IPs
				AddIPs(clientIps, context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]);
				// Añade las IPs del request
				AddIPs(clientIps, context.Request.UserHostAddress);
				AddIPs(clientIps, context.Request.UserHostName);
				// Añade las IPs de red local
				AddIPs(clientIps, GetLanIP());
			}
			// Añade las IPs de REMOTE_ADDR
			AddIPs(clientIps, context.Request.ServerVariables["REMOTE_ADDR"]);
			// Devuelve la dirección IP
			return clientIps;				
	}

	/// <summary>
	///		Añade una serie de IPs a la colección
	/// </summary>
	private void AddIPs(List<string> clientIps, string ip)
	{ 
		if (!string.IsNullOrWhiteSpace(ip))
		{ 
			string [] ipParts = ip.Split(',');

				foreach (string ipTarget in ipParts)
					if (!string.IsNullOrWhiteSpace(ipTarget) && !CheckExists(clientIps, ipTarget.Trim()))
						clientIps.Add(ipTarget.Trim());
		}
	}

	/// <summary>
	///		Comprueba si existe una IP en la colección
	/// </summary>
	private bool CheckExists(List<string> clientIps, string ip)
	{ 
		// Comprueba la IP en la colección
		foreach (string ipSource in clientIps)
			if (ipSource.Equals(ip, StringComparison.CurrentCultureIgnoreCase))
				return true;
		// Si ha llegado hasta aquí es porque no existe
		return false;
	}
}
