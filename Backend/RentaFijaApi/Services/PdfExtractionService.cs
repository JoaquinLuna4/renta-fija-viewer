using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Globalization;
using System.Text.RegularExpressions;
using RentaFijaApi.DTOs; // Asegúrate de que tu DTO esté en este namespace o ajusta.

namespace RentaFijaApi.Services
{
    public class PdfExtractionService : IPdfExtractionService
    {
        public PdfExtractionService()
        {
            // Puedes inyectar dependencias aquí si fueran necesarias para la extracción
        }

        private DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null;
            }

            string[] formats = { "dd-MMM-yy", "dd-MMM-yyyy", "d-MMM-yy" };
            CultureInfo culture = CultureInfo.InvariantCulture; // InvariantCulture no espera puntos en las abreviaciones de los meses

            if (DateTime.TryParseExact(dateString, formats, culture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate;
            }
            return null;
        }

        public List<RentaFijaActivo> ExtractRentaFijaData(string pdfFilePath)
        {
            var assets = new List<RentaFijaActivo>();

            if (!File.Exists(pdfFilePath))
            {
                Console.WriteLine("El archivo PDF no existe en la ruta especificada.");
                return assets;
            }

            try
            {
                using (PdfDocument document = PdfDocument.Open(pdfFilePath))
                {
                    Page page = document.GetPage(3); 
                    Console.WriteLine($"\n--- Procesando Página {page.Number} ---");

                    var words = page.GetWords().OrderBy(w => w.BoundingBox.Bottom)
                                                .ThenBy(w => w.BoundingBox.Left)
                                                .ToList();

                    
                    
                    
                    //// Estos logs son útiles para depurar el posicionamiento de las columnas
                    //Console.WriteLine("\n--- Palabras y Coordenadas en Página 3 ---");
                    //foreach (var word in words)
                    //{
                    //    Console.WriteLine($"'{word.Text}' [X:{word.BoundingBox.Left:F2}, Y:{word.BoundingBox.Bottom:F2}, W:{word.BoundingBox.Width:F2}, H:{word.BoundingBox.Height:F2}]");
                    //}
                    //Console.WriteLine("-------------------------------------------");





                    var columnXRanges = new Dictionary<string, (double minX, double maxX)>
                    {
                        { "NombreCompleto", (17.0, 110.0) },
                        { "CodigoTicker", (150.0, 180.0) },
                        { "Vencimiento", (195.0, 240.0) },
                        { "Cotizacion", (465.0, 500.0) },
                        { "FechaUltimaCotizacion", (520.0, 560.0) },
                        { "TirAnual", (835.0, 870.0) },
                        { "Paridad", (790.0, 825.0) }
                    };

                    double lineTolerance = 5;
                    var linesOfWords = words.GroupBy(w => (int)(w.BoundingBox.Bottom / lineTolerance))
                                            .OrderByDescending(g => g.Key)
                                            .ToList();

                    RentaFijaActivo currentAsset = null;
                    foreach (var lineGroup in linesOfWords)
                    {
                        var lineWords = lineGroup.OrderBy(w => w.BoundingBox.Left).ToList();

                        var tickerWord = lineWords.FirstOrDefault(w => w.BoundingBox.Left >= columnXRanges["CodigoTicker"].minX &&
                                                              w.BoundingBox.Left <= columnXRanges["CodigoTicker"].maxX &&
                                                              Regex.IsMatch(w.Text, @"^[A-Z]{1,5}[A-Z0-9]+$", RegexOptions.IgnoreCase));

                        if (tickerWord != null)
                        {
                            // Si encontramos un nuevo ticker, agregamos el activo anterior (si existe) y creamos uno nuevo.
                            if (currentAsset != null)
                            {
                                assets.Add(currentAsset);
                            }
                            currentAsset = new RentaFijaActivo();
                            currentAsset.CodigoTicker = tickerWord.Text.Trim();

                            // Extraer NombreCompleto
                            var nombreCompletoWords = lineWords.Where(w => w.BoundingBox.Left >= columnXRanges["NombreCompleto"].minX &&
                                                                           w.BoundingBox.Left <= columnXRanges["NombreCompleto"].maxX &&
                                                                           w.Text != currentAsset.CodigoTicker)
                                                                .Select(w => w.Text)
                                                                .ToList();
                            currentAsset.NombreCompleto = string.Join(" ", nombreCompletoWords).Trim();

                            // Determinar TipoActivo (basado en el código original)
                            if (currentAsset.CodigoTicker.StartsWith("TX", StringComparison.OrdinalIgnoreCase) || currentAsset.NombreCompleto.Contains("BONTE", StringComparison.OrdinalIgnoreCase))
                            {
                                currentAsset.TipoActivo = "BONTE";
                            }
                            else if (currentAsset.CodigoTicker.StartsWith("AL", StringComparison.OrdinalIgnoreCase) ||
                                     currentAsset.CodigoTicker.StartsWith("GD", StringComparison.OrdinalIgnoreCase) ||
                                     currentAsset.NombreCompleto.Contains("BONAR", StringComparison.OrdinalIgnoreCase) ||
                                     currentAsset.NombreCompleto.Contains("GLOBAL", StringComparison.OrdinalIgnoreCase))
                            {
                                currentAsset.TipoActivo = "BONAR";
                            }
                            else if (currentAsset.NombreCompleto.Contains("BONOS DE CONSOLIDACION", StringComparison.OrdinalIgnoreCase))
                            {
                                currentAsset.TipoActivo = "BONOS DE CONSOLIDACIÓN";
                            }
                            // Se verifica por el prefijo del ticker (TZX) o por palabras clave en el NombreCompleto
                            else if (currentAsset.CodigoTicker.StartsWith("TZX", StringComparison.OrdinalIgnoreCase) ||
                                     (currentAsset.NombreCompleto.Contains("BONO TES. NAC.", StringComparison.OrdinalIgnoreCase) &&
                                      currentAsset.NombreCompleto.Contains("CER", StringComparison.OrdinalIgnoreCase)))
                            {
                                currentAsset.TipoActivo = "BONO TES. NAC. CER"; 
                            }
                            else if (currentAsset.CodigoTicker.StartsWith("T", StringComparison.OrdinalIgnoreCase) &&
                            currentAsset.NombreCompleto.Contains("BONO CAP", StringComparison.OrdinalIgnoreCase))
                            {
                                currentAsset.TipoActivo = "BONOCAP";
                            }
                            else if (currentAsset.CodigoTicker.StartsWith("S", StringComparison.OrdinalIgnoreCase) && currentAsset.NombreCompleto.Contains("LECAP", StringComparison.OrdinalIgnoreCase))
                            {
                                currentAsset.TipoActivo = "LECAP";
                            }
                            else if (currentAsset.NombreCompleto.Contains("BOPREAL", StringComparison.OrdinalIgnoreCase))
                            {
                                currentAsset.TipoActivo = "BOPREAL";
                            }
                            else
                            {
                                currentAsset.TipoActivo = "Otro";
                            }
                        }

                        // Si tenemos un activo en curso (ya sea recién creado o de líneas anteriores)
                        if (currentAsset != null)
                        {
                            // --- Lógica para Vencimiento ---
                            var vencimientoCandidateWords = lineWords.Where(w => w.BoundingBox.Left >= columnXRanges["Vencimiento"].minX && w.BoundingBox.Left <= columnXRanges["Vencimiento"].maxX)
                                                                     .Select(w => w.Text);
                            string vencimientoString = string.Join(" ", vencimientoCandidateWords).Trim();
                            Console.WriteLine($"[DEBUG] Procesando {currentAsset.CodigoTicker ?? "Nuevo Activo"}: Vencimiento String: '{vencimientoString}'");

                            // Solo asigna Vencimiento si aún no está establecido y se puede parsear correctamente
                            if (currentAsset.Vencimiento == null)
                            {
                                DateTime? parsedVencimientoDate = ParseDate(vencimientoString);
                                if (parsedVencimientoDate.HasValue)
                                {
                                    currentAsset.Vencimiento = parsedVencimientoDate.Value.ToString("dd-MMM-yy");
                                }
                            }

                            // --- Lógica para Cotizacion ---
                            var cotizacionCandidateWords = lineWords.Where(w => w.BoundingBox.Left >= columnXRanges["Cotizacion"].minX && w.BoundingBox.Left <= columnXRanges["Cotizacion"].maxX)
                                                                    .Select(w => w.Text);
                            string cotizacionString = string.Join("", cotizacionCandidateWords).Trim().Replace(",", ".");
                            if (decimal.TryParse(cotizacionString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedCotizacion))
                            {
                                currentAsset.Cotizacion = parsedCotizacion;
                            }

                            // --- Lógica para TirAnual ---
                            var tirAnualCandidateWords = lineWords.Where(w => w.BoundingBox.Left >= columnXRanges["TirAnual"].minX && w.BoundingBox.Left <= columnXRanges["TirAnual"].maxX)
                                                                  .Select(w => w.Text);
                            string tirAnualString = string.Join("", tirAnualCandidateWords).Trim().Replace("%", "").Replace(",", ".");
                            if (decimal.TryParse(tirAnualString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedTirAnual))
                            {
                                currentAsset.TirAnual = parsedTirAnual;
                            }

                            // --- Lógica para Paridad ---
                            var paridadCandidateWords = lineWords.Where(w => w.BoundingBox.Left >= columnXRanges["Paridad"].minX && w.BoundingBox.Left <= columnXRanges["Paridad"].maxX)
                                                                 .Select(w => w.Text);
                            string paridadString = string.Join("", paridadCandidateWords).Trim().Replace("%", "").Replace(",", ".");
                            if (decimal.TryParse(paridadString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedParidad))
                            {
                                currentAsset.Paridad = parsedParidad;
                            }

                            // --- Lógica para FechaUltimaCotizacion ---
                            var fechaUltimaCotizacionCandidateWords = lineWords.Where(w => w.BoundingBox.Left >= columnXRanges["FechaUltimaCotizacion"].minX && w.BoundingBox.Left <= columnXRanges["FechaUltimaCotizacion"].maxX)
                                                                             .Select(w => w.Text);
                            string fechaUltimaCotizacionString = string.Join(" ", fechaUltimaCotizacionCandidateWords).Trim();
                            Console.WriteLine($"[DEBUG] Procesando {currentAsset.CodigoTicker ?? "Nuevo Activo"}: FechaUltimaCotizacion String: '{fechaUltimaCotizacionString}'");

                            // Solo asigna FechaUltimaCotizacion si aún no está establecida y se puede parsear correctamente
                            if (currentAsset.FechaUltimaCotizacion == null)
                            {
                                DateTime? parsedFechaCotizacionDate = ParseDate(fechaUltimaCotizacionString);
                                if (parsedFechaCotizacionDate.HasValue)
                                {
                                    currentAsset.FechaUltimaCotizacion = parsedFechaCotizacionDate;
                                }
                            }
                        }
                    }
                    if (currentAsset != null)
                    {
                        assets.Add(currentAsset);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general al extraer datos del PDF: {ex.Message}");
            }

            var filteredAssets = assets.Where(a =>
                    a.TipoActivo == "BONTE" ||
                    a.TipoActivo == "BONO TES. NAC. CER" ||
                    a.TipoActivo == "BONAR" ||
                    a.TipoActivo == "BONOS DE CONSOLIDACIÓN" ||
                    a.TipoActivo == "LECAP" ||
                    a.TipoActivo == "BONOCAP" ||
                    a.TipoActivo == "BOPREAL"
                ).ToList();

            Console.WriteLine($"[DEBUG] Activos filtrados encontrados: {filteredAssets.Count}");
            return filteredAssets;
        }
    }
}