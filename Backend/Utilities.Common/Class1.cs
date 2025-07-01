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
        public static double? ParseDouble(string valueString)
        {
            if (string.IsNullOrWhiteSpace(valueString))
            {
                return null;
            }
            // Reemplazar separador de miles (punto) y coma decimal (coma) por formato punto decimal estándar
            valueString = valueString.Replace(".", "").Replace(",", ".");
            if (double.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Parsea una cadena de porcentaje a un valor double, manejando el símbolo % y comas.
        /// </summary>
        public static double? ParsePercentage(string valueString)
        {
            if (string.IsNullOrWhiteSpace(valueString))
            {
                return null;
            }
            // Eliminar %, separador de miles y coma decimal por formato punto decimal estándar
            valueString = valueString.Replace("%", "").Replace(".", "").Replace(",", ".");
            if (double.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Parsea una cadena de fecha a un objeto DateTime?, manejando diferentes formatos.
        /// </summary>
        public static DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null;
            }

            string[] formats = { "dd-MMM-yy", "dd-MMM-yyyy", "d-MMM-yy" };
            CultureInfo culture = CultureInfo.InvariantCulture;

            if (DateTime.TryParseExact(dateString, formats, culture, DateTimeStyles.AllowWhiteSpaces, out DateTime parsedDate))
            {
                return parsedDate;
            }
            return null;
        }
    }
}
