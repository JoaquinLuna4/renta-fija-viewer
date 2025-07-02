using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using AngleSharp.Html.Dom; // Agrega este using
using AngleSharp.Html.Parser; // Agrega este using
using System; // Para DateTime.Today

using System.Text.RegularExpressions;
using System.Globalization;
using RentaFijaApi.DTOs;
using RentaFijaApi.Services;
public class RentaFijaService
{
    private readonly HttpClient _httpClient;
    private readonly IPdfExtractionService _pdfExtractionService;
    private readonly string _iamcReportsBaseUrl = "https://www.iamc.com.ar/Informe/";
    private readonly string _pdfFileName = "InformeDiarioIAMC.pdf"; // Nombre de archivo temporal

    public RentaFijaService(HttpClient httpClient, IPdfExtractionService pdfExtractionService)
    {
        _httpClient = httpClient;
        _pdfExtractionService = pdfExtractionService;
    }

    // Este método ahora se encargará de encontrar la URL del PDF dentro de la página del informe
    public async Task<string> FindPdfUrlFromDailyReportPageAsync(string date)
    {

        // Construye la URL del informe diario basada en la fecha
        // Formato: InformeRentaFijaDDMMYY (ej. InformeRentaFija280625)
        //string datePart = date.ToString("ddMMyy");
        string dateSure = "240625";
        string reportUrl = $"{_iamcReportsBaseUrl}InformeRentaFija{dateSure}/";


        Console.WriteLine($"[DEBUG] Intentando acceder a la página del informe: {reportUrl}");

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(reportUrl);
            Console.WriteLine($"Respuesta del servidor: {response.StatusCode}");
            response.EnsureSuccessStatusCode(); // Lanza excepción si la respuesta no es 2xx

            string htmlContent = await response.Content.ReadAsStringAsync();

            // *** LÍNEA DE DEPURACIÓN: Guarda el HTML a un archivo temporal ***
            //string tempHtmlFilePath = Path.Combine(Path.GetTempPath(), $"IAMC_Report_{datePart}.html");
            //await File.WriteAllTextAsync(tempHtmlFilePath, htmlContent);
            //Console.WriteLine($"[DEBUG] HTML de la página guardado en: {tempHtmlFilePath}");
            // ***************************************************************


            // Usar AngleSharp para parsear el HTML
            var parser = new HtmlParser();
            IHtmlDocument document = await parser.ParseDocumentAsync(htmlContent);

            // Buscar el div con id "visualizadorpdf" y luego la etiqueta <object>
            // La etiqueta <object> está directamente dentro de un div con class "pdfVisualizador"
            var pdfObjectElement = document.QuerySelector("div.pdfVisualizador object");



            if (pdfObjectElement == null)
            {
                Console.WriteLine("[DEBUG] El selector 'div.pdfVisualizador object' no encontró ningún elemento. Revisa el HTML de la página y el selector.");
                return null;
            }

            string pdfUrl = pdfObjectElement.GetAttribute("data");
            if (!string.IsNullOrEmpty(pdfUrl))
            {
                Console.WriteLine($"[DEBUG] URL del PDF encontrada: {pdfUrl}");
                return pdfUrl;
            }
            else
            {
                Console.WriteLine("[DEBUG] Se encontró el elemento <object>, pero su atributo 'data' es nulo o vacío.");
                return null;
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error HTTP al acceder a la página del informe ({reportUrl}): {e.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al parsear la página del informe ({reportUrl}): {ex.Message}");
            return null;
        }
    }


    public async Task<string> DownloadPdfAsync(string pdfUrl)
    {
        Console.WriteLine($"[DEBUG] Àccediendo a la class DownloadPdf");

        if (string.IsNullOrEmpty(pdfUrl))
        {
            Console.WriteLine("URL del PDF vacía o nula. No se puede descargar.");
            return null;
        }

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(pdfUrl);
            response.EnsureSuccessStatusCode();

            string filePath = Path.Combine(Path.GetTempPath(), _pdfFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(fileStream);
            }
            Console.WriteLine($"PDF descargado en: {filePath}");
            return filePath;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error al descargar el PDF de {pdfUrl}: {e.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado al descargar el PDF: {ex.Message}");
            return null;
        }
    }

    public List<RentaFijaActivo> ExtractRentaFijaData(string pdfFilePath)
    {
        // Esta línea ahora debería funcionar porque _pdfExtractionService ya no será null
        return _pdfExtractionService.ExtractRentaFijaData(pdfFilePath);
    }

}




//    public List<RentaFijaActivo> ExtractRentaFijaData(string pdfFilePath)
//    {
//        var data = new List<Dictionary<string, string>>();
//        var assets = new List<RentaFijaActivo>();

//        if (!File.Exists(pdfFilePath))
//        {
//            Console.WriteLine("El archivo PDF no existe en la ruta especificada.");
//            return assets;
//        }

//        try
//        {
//            using (PdfDocument document = PdfDocument.Open(pdfFilePath))
//            {
//                Page page = document.GetPage(3); //Obtenemos la pagina 3
//                Console.WriteLine($"\n--- Procesando Página {page.Number} ---");

//                var words = page.GetWords().OrderBy(w => w.BoundingBox.Bottom)
//                                                  .ThenBy(w => w.BoundingBox.Left)
//                                                  .ToList();





//                // TEMPORAL: IMPRIME LAS PALABRAS CON SUS COORDENADAS PARA LA DEPURACIÓN
//                Console.WriteLine("\n--- Palabras y Coordenadas en Página 3 ---");
//                foreach (var word in words)
//                {
//                    Console.WriteLine($"'{word.Text}' [X:{word.BoundingBox.Left:F2}, Y:{word.BoundingBox.Bottom:F2}, W:{word.BoundingBox.Width:F2}, H:{word.BoundingBox.Height:F2}]");
//                }
//                Console.WriteLine("-------------------------------------------");





//                var columnXRanges = new Dictionary<string, (double minX, double maxX)>
//{
//    { "NombreCompleto", (17.0, 110.0) },
//    { "CodigoTicker", (150.0, 180.0) },
//    { "Vencimiento", (195.0, 240.0) },
//    { "Cotizacion", (465.0, 500.0) },
//    { "FechaUltimaCotizacion", (520.0, 560.0) }, // ¡NUEVO! Rango para la fecha de última cotización
//    { "TirAnual", (835.0, 870.0) },
//    { "Paridad", (790.0, 825.0) }
//};

//                // Agrupamos palabras por líneas, con una tolerancia para palabras ligeramente desalineadas en Y
//                // Puedes ajustar el 'lineTolerance' si las palabras de la misma línea tienen Y muy diferentes.
//                double lineTolerance = 5; // Pixels
//                var linesOfWords = words.GroupBy(w => (int)(w.BoundingBox.Bottom / lineTolerance))
//                                        .OrderByDescending(g => g.Key) // Ordenar por Y de abajo hacia arriba (líneas de datos primero)
//                                        .ToList();


//                RentaFijaActivo currentAsset = null;
//                foreach (var lineGroup in linesOfWords)
//                {
//                    // Ordena las palabras dentro de cada línea por su posición X
//                    var lineWords = lineGroup.OrderBy(w => w.BoundingBox.Left).ToList();

//                    // Intentar identificar un nuevo activo basándose en el ticker o el nombre.
//                    // Busca el ticker en una posición X esperada.
//                    var tickerWord = lineWords.FirstOrDefault(w => w.BoundingBox.Left >= columnXRanges["CodigoTicker"].minX &&
//                                                                   w.BoundingBox.Left <= columnXRanges["CodigoTicker"].maxX &&
//                                                                   Regex.IsMatch(w.Text, @"^[A-Z]{2,5}\d{2,3}D?$", RegexOptions.IgnoreCase)); // Regex para un ticker típico

//                    if (tickerWord != null)
//                    {
//                        if (currentAsset != null)
//                        {
//                            assets.Add(currentAsset);
//                        }
//                        currentAsset = new RentaFijaActivo();
//                        currentAsset.CodigoTicker = tickerWord.Text.Trim();

//                        // Intentar capturar el NombreCompleto antes o en la misma línea
//                        var nombreCompletoWords = lineWords.Where(w => w.BoundingBox.Left >= columnXRanges["NombreCompleto"].minX &&
//                                                                       w.BoundingBox.Left <= columnXRanges["NombreCompleto"].maxX &&
//                                                                       w.Text != currentAsset.CodigoTicker) // Excluir el propio ticker
//                                                          .Select(w => w.Text)
//                                                          .ToList();
//                        currentAsset.NombreCompleto = string.Join(" ", nombreCompletoWords).Trim();

//                        // Asignar TipoActivo basado en el nombre o ticker para el filtro
//                        if (currentAsset.CodigoTicker.StartsWith("TX", StringComparison.OrdinalIgnoreCase) || currentAsset.NombreCompleto.Contains("BONTE", StringComparison.OrdinalIgnoreCase))
//                        {
//                            currentAsset.TipoActivo = "BONTE";
//                        }
//                        else if (currentAsset.CodigoTicker.StartsWith("AL", StringComparison.OrdinalIgnoreCase) ||
//                                 currentAsset.CodigoTicker.StartsWith("GD", StringComparison.OrdinalIgnoreCase) ||
//                                 currentAsset.NombreCompleto.Contains("BONAR", StringComparison.OrdinalIgnoreCase) ||
//                                 currentAsset.NombreCompleto.Contains("GLOBAL", StringComparison.OrdinalIgnoreCase))
//                        {
//                            currentAsset.TipoActivo = "BONAR";
//                        }
//                        else if (currentAsset.NombreCompleto.Contains("BONOS DE CONSOLIDACION", StringComparison.OrdinalIgnoreCase))
//                        {
//                            currentAsset.TipoActivo = "BONOS DE CONSOLIDACIÓN";
//                        }
//                        else if (currentAsset.CodigoTicker.StartsWith("S", StringComparison.OrdinalIgnoreCase) && currentAsset.NombreCompleto.Contains("LECAP", StringComparison.OrdinalIgnoreCase))
//                        {
//                            currentAsset.TipoActivo = "LECAP";
//                        }
//                        else if (currentAsset.NombreCompleto.Contains("BOPREAL", StringComparison.OrdinalIgnoreCase)) // Añadido para BOPREAL
//                        {
//                            currentAsset.TipoActivo = "BOPREAL";
//                        }
//                        else
//                        {
//                            currentAsset.TipoActivo = "Otro";
//                        }

//                        // Debug: para ver qué tipo de activo se está identificando
//                        // Console.WriteLine($"[DEBUG] Activo detectado: {currentAsset.NombreCompleto} ({currentAsset.CodigoTicker}) - Tipo: {currentAsset.TipoActivo}");
//                    }

//                    if (currentAsset != null)
//                    {
//                        // Llenar propiedades de currentAsset para Vencimiento, Cotizacion, TIR Anual, Paridad
//                        // Recolectar todas las palabras en el rango de X para cada columna
//                        var vencimientoWords = lineWords.Where(w => w.BoundingBox.Left >= columnXRanges["Vencimiento"].minX && w.BoundingBox.Left <= columnXRanges["Vencimiento"].maxX)
//                                                        .Select(w => w.Text);
//                        currentAsset.Vencimiento = string.Join(" ", vencimientoWords).Trim(); // Captura todas las palabras de la columna de vencimiento


//                        // Para valores numéricos, recolecta y luego intenta parsear
//                        var cotizacionCandidateWords = lineWords.Where(w => w.BoundingBox.Left >= columnXRanges["Cotizacion"].minX && w.BoundingBox.Left <= columnXRanges["Cotizacion"].maxX)
//                                                                .Select(w => w.Text);
//                        string cotizacionString = string.Join("", cotizacionCandidateWords).Trim(); // Une las palabras para el parseo
//                        decimal parsedCotizacion;
//                        // Usamos NumberStyles.Any para flexibilizar la lectura, e InvariantCulture para evitar problemas con comas/puntos.
//                        // Reemplazamos coma por punto para el parseo si es necesario
//                        cotizacionString = cotizacionString.Replace(",", ".");
//                        if (decimal.TryParse(cotizacionString, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedCotizacion))
//                        {
//                            currentAsset.Cotizacion = parsedCotizacion;
//                        }
//                        var tirAnualCandidateWords = lineWords.Where(w => w.BoundingBox.Left >= columnXRanges["TirAnual"].minX && w.BoundingBox.Left <= columnXRanges["TirAnual"].maxX)
//                                                              .Select(w => w.Text);
//                        string tirAnualString = string.Join("", tirAnualCandidateWords).Trim().Replace("%", "");
//                        decimal parsedTirAnual;
//                        tirAnualString = tirAnualString.Replace(",", ".");
//                        if (decimal.TryParse(tirAnualString, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedTirAnual))
//                        {
//                            currentAsset.TirAnual = parsedTirAnual;
//                        }

//                        var paridadCandidateWords = lineWords.Where(w => w.BoundingBox.Left >= columnXRanges["Paridad"].minX && w.BoundingBox.Left <= columnXRanges["Paridad"].maxX)
//                                                             .Select(w => w.Text);
//                        string paridadString = string.Join("", paridadCandidateWords).Trim().Replace("%", "");
//                        decimal parsedParidad;
//                        paridadString = paridadString.Replace(",", ".");
//                        if (decimal.TryParse(paridadString, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedParidad))
//                        {
//                            currentAsset.Paridad = parsedParidad;
//                        }
//                    }
//                }
//                // Agrega el último activo procesado de la página
//                if (currentAsset != null)
//                {
//                    assets.Add(currentAsset);
//                }
//            } // Fin del using (PdfDocument document)

//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Error general al extraer datos del PDF: {ex.Message}");
//        }
//        // --- FILTRADO FINAL DESPUÉS DE LA EXTRACCIÓN ---
//        var filteredAssets = assets.Where(a =>
//                a.TipoActivo == "BONTE" ||
//                a.TipoActivo == "BONAR" ||
//                a.TipoActivo == "BONOS DE CONSOLIDACIÓN"
//            ).ToList();

//        Console.WriteLine($"[DEBUG] Activos filtrados encontrados: {filteredAssets.Count}");
//        return filteredAssets;
//    }
//}
