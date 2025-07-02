using System.Globalization;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Utilities.Common
{
    public static class ValueParsers
    {
        /// <summary>
        /// Parsea una cadena a un valor double, manejando comas como decimales.
        /// </summary>
        public static decimal? ParseDouble(string valueString)
        {
            if (string.IsNullOrWhiteSpace(valueString))
            {
                return null;
            }
            // Eliminar comas (separador de miles) si existen
            valueString = valueString.Replace(",", "");
            // Intentar parsear usando CultureInfo.InvariantCulture, que usa punto como decimal.
            // Ojo: NumberStyles.Any permite espacios, comas (que ya eliminamos), signos, etc.
            if (decimal.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            Console.WriteLine($"[DEBUG] Fallo al parsear decimal para la cadena: '{valueString}'");
            return null;
        }

        /// <summary>
        /// Parsea una cadena de porcentaje a un valor double, manejando el símbolo % y comas.
        /// </summary>
        public static decimal? ParsePercentage(string valueString)
        {
            if (string.IsNullOrWhiteSpace(valueString))
            {
                return null;
            }

            // Eliminar % y comas (separador de miles) si existen
            valueString = valueString.Replace("%", "").Replace(",", "");

            // Intentar parsear usando CultureInfo.InvariantCulture, que usa punto como decimal.
            if (decimal.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            Console.WriteLine($"[DEBUG] Fallo al parsear porcentaje para la cadena: '{valueString}'");
            return null;
        }
        /// <summary>
        /// Parsea una cadena de fecha a un objeto DateTime?, manejando diferentes formatos y culturas.
        /// </summary>
        public static DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null;
            }

            dateString = dateString.Trim(); // Asegurarse de que no haya espacios extra

            // Formatos comunes, incluyendo los que ya tenías y algunos numéricos/variaciones
            // se ajustó para que los formatos de mes con abreviaturas (MMM) sean sensibles a la cultura
            string[] commonFormats = {
                "dd-MMM-yy", "dd-MMM-yyyy", "d-MMM-yy", // Ej: 10-Dec-35, 1-Jul-2026
                "dd/MM/yy", "dd/MM/yyyy", "d/M/yy", "d/M/yyyy", // Ej: 10/12/35, 1/7/2026
                "yyyy-MM-dd", "yy-MM-dd" // Formatos ISO u otros comunes
            };

            // Intentar parsear con CultureInfo.InvariantCulture (para abreviaturas de mes en inglés como "Dec", "Jul")
            if (DateTime.TryParseExact(dateString, commonFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime parsedDateInvariant))
            {
                return parsedDateInvariant;
            }

            // Intentar parsear con cultura española de España (para abreviaturas de mes en español como "Ene", "Abr")
            CultureInfo esCulture = new CultureInfo("es-ES");
            if (DateTime.TryParseExact(dateString, commonFormats, esCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime parsedDateEs))
            {
                return parsedDateEs;
            }

            // Intentar parsear con cultura española de Argentina (si hay alguna diferencia sutil)
            CultureInfo arCulture = new CultureInfo("es-AR");
            if (DateTime.TryParseExact(dateString, commonFormats, arCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime parsedDateAr))
            {
                return parsedDateAr;
            }

            // Si llegamos aquí, ningún formato conocido pudo parsear la fecha
            Console.WriteLine($"[DEBUG] Fallo al parsear fecha para la cadena: '{dateString}'");
            return null;
        }
    }
}
