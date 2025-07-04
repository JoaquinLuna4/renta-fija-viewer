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
    private readonly IGeminiApiService _geminiApiService; 
    public RentaFijaService(HttpClient httpClient, IPdfExtractionService pdfExtractionService, IGeminiApiService geminiApiService)
    {
        _httpClient = httpClient;
        _pdfExtractionService = pdfExtractionService;
        _geminiApiService = geminiApiService;
    }

    // Este método ahora se encargará de encontrar la URL del PDF dentro de la página del informe
    public async Task<string> FindPdfUrlFromDailyReportPageAsync()
    {

        // Construye la URL del informe diario basada en la fecha
        // Formato: InformeRentaFijaDDMMYY (ej. InformeRentaFija280625)
        //string datePart = date.ToString("ddMMyy");
        string dateSure = "040725";
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

    
    public async Task<List<RentaFijaActivo>> GetRentaFijaDataForTodayAsync()
    {
        Console.WriteLine($"[DEBUG] Iniciando proceso de extracción de datos ");

        // 1. Encontrar la URL del PDF
        string pdfPageUrl = await FindPdfUrlFromDailyReportPageAsync();
        if (string.IsNullOrEmpty(pdfPageUrl))
        {
            Console.WriteLine("No se pudo encontrar la URL del PDF. Abortando extracción.");
            return new List<RentaFijaActivo>();
        }

        // 2. Descargar el PDF
        string pdfFilePath = await DownloadPdfAsync(pdfPageUrl);
        if (string.IsNullOrEmpty(pdfFilePath))
        {
            Console.WriteLine("No se pudo descargar el PDF. Abortando extracción.");
            return new List<RentaFijaActivo>();
        }

        // 3. Extraer el texto completo del PDF usando PdfExtractionService
        Console.WriteLine($"[DEBUG] Extrayendo texto completo del PDF: {pdfFilePath}");
        string fullPdfText = _pdfExtractionService.ExtractFullTextFromPdf(pdfFilePath);

        // 4. Enviar el texto a Gemini para la extracción inteligente de los datos
        Console.WriteLine("[DEBUG] Enviando texto a Gemini para interpretación.");
        List<RentaFijaActivo> activos = await _geminiApiService.ExtractRentaFijaDataFromTextAsync(fullPdfText);

        // Opcional: Eliminar el archivo PDF temporal después de procesar
        try
        {
            if (File.Exists(pdfFilePath))
            {
                File.Delete(pdfFilePath);
                Console.WriteLine($"[DEBUG] Archivo PDF temporal eliminado: {pdfFilePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] No se pudo eliminar el archivo PDF temporal {pdfFilePath}: {ex.Message}");
        }

        Console.WriteLine($"[DEBUG] Proceso de extracción finalizado. Se encontraron {activos.Count} activos.");
        return activos;
    }

}

