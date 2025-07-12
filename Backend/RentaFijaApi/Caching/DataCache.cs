using RentaFijaApi.DTOs;

namespace RentaFijaApi.Caching
{
    public static class DataCache
    {
        // Almacenará el último informe completo extraído
        public static RentaFijaReportResponse? CachedReport { get; private set; }

        // Almacenará la fecha del día en que se cacheó el informe
        public static DateTime? LastCachedDate { get; private set; }

        // Un objeto de bloqueo para asegurar que solo una operación de extracción/cacheo ocurra a la vez
        private static readonly object _cacheLock = new object();

        /// <summary>
        /// Establece los datos en la caché.
        /// </summary>
        /// <param name="report">El informe de renta fija completo a cachear.</param>
        /// <param name="date">La fecha del informe.</param>
        public static void SetCache(RentaFijaReportResponse report, DateTime date)
        {
            lock (_cacheLock)
            {
                CachedReport = report;
                LastCachedDate = date.Date; // Almacenamos solo la fecha (sin hora)
                Console.WriteLine($"[DEBUG] Datos de informe para {date.ToShortDateString()} cacheados.");
            }
        }

        /// <summary>
        /// Intenta obtener un informe de la caché para la fecha actual.
        /// </summary>
        /// <param name="today">La fecha actual (solo día, sin hora).</param>
        /// <returns>El informe cacheado si es válido para hoy, de lo contrario, null.</returns>
        public static RentaFijaReportResponse? GetCache(DateTime today)
        {
            lock (_cacheLock)
            {
                // Comparamos solo la fecha para un "cacheo diario"
                if (CachedReport != null && LastCachedDate.HasValue && LastCachedDate.Value.Date == today.Date)
                {
                    Console.WriteLine($"[DEBUG] Datos de informe para {today.ToShortDateString()} encontrados en caché.");
                    return CachedReport;
                }
                Console.WriteLine($"[DEBUG] Datos de informe para {today.ToShortDateString()} no encontrados o desactualizados en caché.");
                return null;
            }
        }

        /// <summary>
        /// Limpia la caché.
        /// </summary>
        public static void ClearCache()
        {
            lock (_cacheLock)
            {
                CachedReport = null;
                LastCachedDate = null;
                Console.WriteLine("[DEBUG] Caché de informes de renta fija limpiada.");
            }
        }
    }
}
