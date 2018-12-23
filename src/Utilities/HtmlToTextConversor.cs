using System;
using System.Text.RegularExpressions;

namespace Bau.Libraries.LibSystem.Utilities
{
	/// <summary>
	/// Clase para conversión de HTML a texto
	/// </summary>
	public class HtmlToTextConversor
	{
		/// <summary>
		/// Convierte texto a HTML
		/// </summary>
		public string ConvertToText(string sourceHtml)
		{ 
			string result = "";
				
				// Si realmente tiene algo para convertir
				if (!string.IsNullOrEmpty(sourceHtml))
				{ 
					//Elimina los saltos de líneas y los tabuladores
					result = sourceHtml.Replace("\r", " ");
					result = result.Replace("\n", " ");
					result = result.Replace("\t", string.Empty);
					// Elimina los espacios duplicados
					result = Replace(result, "( )+", " ");
					// Elimina la cabecera (primero se prepara para eliminar los atributos)
					result = Replace(result, "<( )*head([^>])*>", "<head>");
					result = Replace(result, "(<( )*(/)( )*head( )*>)", "</head>");
					result = Replace(result, "(<head>).*(</head>)", string.Empty);
					// Elimina los script (primero se prepara para eliminar los atributos)
					result = Replace(result, "<( )*script([^>])*>", "<script>");
					result = Replace(result, "(<( )*(/)( )*script( )*>)", "</script>");
					result = Replace(result, "(<script>).*(</script>)", string.Empty);
					// Elimina los estilos (primero se prepara para eliminar los atributos)
					result = Replace(result, "<( )*style([^>])*>", "<style>");
					result = Replace(result, "(<( )*(/)( )*style( )*>)", "</style>");
					result = Replace(result, "(<style>).*(</style>)", string.Empty);
					// Sustituye las etiquetas td por tabuladores
					result = Replace(result, "<( )*td([^>])*>", "\t");
					// Inserta salto de líneas en lugar de los <br> y <li>
					result = Replace(result, "<( )*br( )*>", "\r");
					result = Replace(result, "<( )*li( )*>", "\r");
					// Inserta saltos de línea dobles en lugar de los <p>, <div> and <tr> tags
					result = Replace(result, "<( )*div([^>])*>", "\r\r");	
					result = Replace(result, "<( )*tr([^>])*>", "\r\r");
					result = Replace(result, "<( )*p([^>])*>", "\r\r");
					// Elimina el resto de etiquetas como <a>, vínculos, imágenes, comentarios... (todo lo que esté entre < y >)
					result = Replace(result, "<[^>]*>", string.Empty);
					// Reemplaza los caracteres especiales
					result = Replace(result, " ", " ");
					result = Replace(result, "&bull;", " * ");
					result = Replace(result, "&lsaquo;", "<");
					result = Replace(result, "&rsaquo;", ">");
					result = Replace(result, "&trade;", "(tm)");
					result = Replace(result, "&frasl;", "/");
					result = Replace(result, "&lt;", "<");
					result = Replace(result, "&gt;", ">");
					result = Replace(result, "&copy;", "(c)");
					result = Replace(result, "&reg;", "(r)");
					// Elimina el resto de caracteres especiales (http://hotwired.lycos.com/webmonkey/reference/special_characters/)
					result = Replace(result, "&(.{2,6});", string.Empty);
					// Crea saltos de líneas consistentes
					result = result.Replace("\n", "\r");
					// Elimina los saltos de línea y tabuladores extras:
					//    - Reemplaza dos saltos de línea por dos y 4 tabuladores por cuatro
					//    - Primero elimina los saltos de línea que puede haber entre los caracteres de escape en los saltos de línea
					result = Replace(result, "(\r)( )+(\r)", "\r\r");
					result = Replace(result, "(\t)( )+(\t)", "\t\t");
					result = Replace(result, "(\t)( )+(\r)", "\t\r");
					result = Replace(result, "(\r)( )+(\t)", "\r\t");
					// Elimina los tabuladores redundantes
					result = Replace(result, "(\r)(\t)+(\r)", "\r\r");
					// Elimina los tabuladores múltiples que siguen a un salto de línea sustituyéndolos por un solo tabulador
					result = Replace(result, "(\r)(\t)+", "\r\t");
					// Elimina los saltos de línea y tabuladores adicionales
					result = ExtractLineBreakTabs(result);
			}
			// Devuelve el texto convertido
			return result?.Trim();
		}

		/// <summary>
		///		Remplaza un texto utilizando una expresión regular
		/// </summary>
		private string Replace(string source, string strRegEx, string strNewString)
		{ 
			if (!string.IsNullOrEmpty(source))
				return Regex.Replace(source, strRegEx, strNewString, RegexOptions.IgnoreCase);
			else
				return source;
		}

		/// <summary>
		/// Elimina los saltos de línea y tabuladores duplicados
		/// </summary>
		private string ExtractLineBreakTabs(string source)
		{ 
			// Normaliza saltos de línea y tabuladores
			if (!string.IsNullOrEmpty(source))
			{ 
				// Elimina los saltos de línea y tabuladores
				while (source.IndexOf("\r\r", StringComparison.InvariantCulture) >= 0)
					source = source.Replace("\r\r", "\r");
				while (source.IndexOf("\t\t", StringComparison.InvariantCulture) >= 0)
					source = source.Replace("\t\t", "\t");
				// Cambia los saltos de línea
				source = source.Replace("\r", Environment.NewLine);
			}
			// Devuelve la cadena final
			return source?.Trim();
		}
	}
}
