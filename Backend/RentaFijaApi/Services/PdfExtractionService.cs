using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Text.RegularExpressions;
using RentaFijaApi.DTOs;
using Utilities.Common;
using System.Linq;

namespace RentaFijaApi.Services
{
    public class PdfExtractionService : IPdfExtractionService
    {
        public PdfExtractionService()
        {
            // Puedes inyectar dependencias aquí si fueran necesarias para la extracción
        }
        private readonly Dictionary<string, (double minX, double maxX)> columnXRanges = new Dictionary<string, (double minX, double maxX)>
        {
            { "NombreCompleto", (17.0, 110.0) },
            { "CodigoTicker", (150.0, 180.0) },
            { "Vencimiento", (195.0, 240.0) },
            { "Cotizacion", (465.0, 500.0) },
            { "FechaUltimaCotizacion", (520.0, 560.0) },
            { "TirAnual", (835.0, 870.0) },
            { "Paridad", (790.0, 825.0) }
        };
        // CAMBIO CLAVE AQUÍ: 'lineTolerance' vuelve a ser double
        private const double lineTolerance = 10.0; // Definir la tolerancia

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
                    int[] pagesToProcess = { 3, 5 };
                    foreach (int pageNumber in pagesToProcess)
                    {
                        // Me aseguro de que el número de página sea válido para el documento
                        if (pageNumber > 0 && pageNumber <= document.NumberOfPages)
                        {
                            Page page = document.GetPage(pageNumber);
                            Console.WriteLine($"\n--- Procesando Página {page.Number} ---");

                            // Obtenemos todas las palabras de la página, ordenadas para un procesamiento más fácil
                            var allWordsOnPage = page.GetWords()
                                .OrderBy(w => w.BoundingBox.Bottom)
                                .ThenBy(w => w.BoundingBox.Left)
                                .ToList();

                            // Estos logs son útiles para depurar el posicionamiento de las columnas
                            //Console.WriteLine("\n--- Palabras y Coordenadas en Página 3 ---");
                            //foreach (var word in allWordsOnPage)
                            //{
                            //    Console.WriteLine($"'{word.Text}' [X:{word.BoundingBox.Left:F2}, Y:{word.BoundingBox.Bottom:F2}, W:{word.BoundingBox.Width:F2}, H:{word.BoundingBox.Height:F2}]");
                            //}
                            //Console.WriteLine("-------------------------------------------");

                            // Agrupamos las palabras en "líneas" basándonos en su coordenada Y inferior
                            // Utilizar una tolerancia mayor aquí es CRÍTICO para agrupar palabras que están
                            // en la misma "fila visual" pero con pequeñas diferencias en su Y.
                            var groupedLines = allWordsOnPage
                                .GroupBy(w => (int)Math.Round(w.BoundingBox.Bottom / lineTolerance))
                                .OrderByDescending(g => g.Key) // Procesar de abajo hacia arriba o como prefieras
                                .ToList();

                            RentaFijaActivo currentAsset = null;
                            foreach (var lineGroup in groupedLines)
                            {
                                var wordsInCurrentLogicalLine = lineGroup.OrderBy(w => w.BoundingBox.Left).ToList();

                                // Intentar identificar el Ticker para esta línea lógica
                                var tickerWord = wordsInCurrentLogicalLine.FirstOrDefault(w =>
                                    w.BoundingBox.Left >= columnXRanges["CodigoTicker"].minX &&
                                    w.BoundingBox.Left <= columnXRanges["CodigoTicker"].maxX &&
                                    Regex.IsMatch(w.Text, @"^[A-Z]{1,5}[A-Z0-9]+$", RegexOptions.IgnoreCase));

                                if (tickerWord != null)
                                {
                                    // Si encontramos un Ticker, significa que hemos encontrado el inicio de una nueva fila de datos
                                    // Terminamos el activo anterior si lo hay
                                    if (currentAsset != null)
                                    {
                                        assets.Add(currentAsset);
                                    }

                                    // Creamos un nuevo activo
                                    currentAsset = new RentaFijaActivo();
                                    currentAsset.CodigoTicker = tickerWord.Text.Trim();

                                    // Define el rango vertical (minY, maxY) para buscar las propiedades de ESTA fila
                                    // Basamos el rango en la coordenada Bottom del ticker (o cualquier otra palabra clave de la línea)
                                    // Ampliamos el rango para asegurarnos de capturar todas las palabras de la fila lógica
                                    // CAMBIO CLAVE AQUÍ: rowMinY y rowMaxY ahora son double
                                    double rowMinY = tickerWord.BoundingBox.Bottom - lineTolerance;
                                    double rowMaxY = tickerWord.BoundingBox.Bottom + lineTolerance;

                                    Console.WriteLine($"[DEBUG] --- Nuevo Activo: {currentAsset.CodigoTicker} (MinY Fila: {rowMinY:F2}, MaxY Fila: {rowMaxY:F2}) ---");

                                    // Extraer NombreCompleto
                                    var nombreCompletoWords = allWordsOnPage
                                        .Where(w => w.BoundingBox.Left >= columnXRanges["NombreCompleto"].minX &&
                                                    w.BoundingBox.Left <= columnXRanges["NombreCompleto"].maxX &&
                                                    w.BoundingBox.Bottom >= rowMinY && w.BoundingBox.Bottom <= rowMaxY && // Filtra por la Y de la fila actual
                                                    w.Text != currentAsset.CodigoTicker)
                                        .OrderBy(w => w.BoundingBox.Left)
                                        .Select(w => w.Text)
                                        .ToList();
                                    currentAsset.NombreCompleto = string.Join(" ", nombreCompletoWords).Trim();
                                    Console.WriteLine($"[DEBUG] NombreCompleto extraído para {currentAsset.CodigoTicker}: '{currentAsset.NombreCompleto}'");

                                    // Determinar TipoActivo (tu lógica existente)
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
                                    else if (currentAsset.NombreCompleto.Contains("O.N.", StringComparison.OrdinalIgnoreCase) ||
                                             currentAsset.NombreCompleto.Contains("O.N", StringComparison.OrdinalIgnoreCase))
                                    {
                                        currentAsset.TipoActivo = "OBLIGACION NEGOCIABLE";
                                    }
                                    else if (currentAsset.NombreCompleto.Contains("BOPREAL", StringComparison.OrdinalIgnoreCase))
                                    {
                                        currentAsset.TipoActivo = "BOPREAL";
                                    }
                                    else
                                    {
                                        currentAsset.TipoActivo = "Otro";
                                    }

                                    // Ahora extraemos el resto de los campos utilizando la función GetColumnStringValue
                                    // y el rango Y específico de la fila
                                    string cotizacionString = GetColumnStringValue("Cotizacion", allWordsOnPage, rowMinY, rowMaxY);
                                    currentAsset.Cotizacion = ValueParsers.ParseDouble(cotizacionString);
                                    Console.WriteLine($"[DEBUG] {currentAsset.CodigoTicker} - Cotizacion String: '{cotizacionString}' -> Parsed: {currentAsset.Cotizacion}");

                                    string tirAnualString = GetColumnStringValue("TirAnual", allWordsOnPage, rowMinY, rowMaxY);
                                    currentAsset.TirAnual = ValueParsers.ParsePercentage(tirAnualString);
                                    Console.WriteLine($"[DEBUG] {currentAsset.CodigoTicker} - TirAnual String: '{tirAnualString}' -> Parsed: {currentAsset.TirAnual}");

                                    string paridadString = GetColumnStringValue("Paridad", allWordsOnPage, rowMinY, rowMaxY);
                                    currentAsset.Paridad = ValueParsers.ParsePercentage(paridadString);
                                    Console.WriteLine($"[DEBUG] {currentAsset.CodigoTicker} - Paridad String: '{paridadString}' -> Parsed: {currentAsset.Paridad}");

                                    string vencimientoString = GetColumnStringValue("Vencimiento", allWordsOnPage, rowMinY, rowMaxY);
                                    currentAsset.Vencimiento = ValueParsers.ParseDate(vencimientoString);
                                    Console.WriteLine($"[DEBUG] {currentAsset.CodigoTicker} - Vencimiento String: '{vencimientoString}' -> Parsed: {currentAsset.Vencimiento}");

                                    string fechaUltimaCotizacionString = GetColumnStringValue("FechaUltimaCotizacion", allWordsOnPage, rowMinY, rowMaxY);
                                    currentAsset.FechaUltimaCotizacion = ValueParsers.ParseDate(fechaUltimaCotizacionString);
                                    Console.WriteLine($"[DEBUG] {currentAsset.CodigoTicker} - FechaUltimaCotizacion String: '{fechaUltimaCotizacionString}' -> Parsed: {currentAsset.FechaUltimaCotizacion}");
                                }
                            }

                            // Agrega el último activo procesado si existe
                            if (currentAsset != null)
                            {
                                assets.Add(currentAsset);
                                currentAsset = null;
                            }
                        }
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
                a.TipoActivo == "OBLIGACION NEGOCIABLE" ||
                a.TipoActivo == "BOPREAL"
            ).ToList();

            Console.WriteLine($"[DEBUG] Activos filtrados encontrados: {filteredAssets.Count}");
            return filteredAssets;
        }

        // CAMBIO CLAVE AQUÍ: minYForCurrentRow y maxYForCurrentRow ahora son double
        private string GetColumnStringValue(string columnName, List<Word> allWordsOnPage, double minYForCurrentRow, double maxYForCurrentRow)
        {
            var minX = columnXRanges[columnName].minX;
            var maxX = columnXRanges[columnName].maxX;

            // Filtramos las palabras que están dentro del rango X de la columna
            // Y también dentro del rango Y de la fila actual
            var relevantWords = allWordsOnPage
                .Where(w => w.BoundingBox.Centroid.X >= minX && w.BoundingBox.Centroid.X <= maxX)
                .Where(w => w.BoundingBox.Centroid.Y >= minYForCurrentRow && w.BoundingBox.Centroid.Y <= maxYForCurrentRow)
                .OrderBy(w => w.BoundingBox.Centroid.X)
                .ToList();

            if (!relevantWords.Any())
            {
                // Console.WriteLine($"[DEBUG] Columna '{columnName}': No se encontraron palabras en el rango X (Fila Y: {minYForCurrentRow:F2}-{maxYForCurrentRow:F2}).");
                return null;
            }

            string extractedText = string.Join(" ", relevantWords.Select(w => w.Text)).Trim();
            // Console.WriteLine($"[DEBUG] Columna '{columnName}': Texto extraído antes de parsear: '{extractedText}' (BoundingBox: {string.Join(", ", relevantWords.Select(w => w.BoundingBox.ToString()))})");
            return extractedText;
        }
    }
}