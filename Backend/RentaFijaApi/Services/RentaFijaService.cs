
using AngleSharp.Html.Dom; // Agrega este using
using AngleSharp.Html.Parser; // Agrega este using
using System; // Para DateTime.Today
using RentaFijaApi.Caching; 
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
    public RentaFijaService(HttpClient httpClient, IPdfExtractionService pdfExtractionService, IGeminiApiService geminiApiService, bool useSimulation = false)
    {
        _httpClient = httpClient;
        _pdfExtractionService = pdfExtractionService;
        _geminiApiService = geminiApiService;
        
    }

    // Este método ahora se encargará de encontrar la URL del PDF dentro de la página del informe
    public async Task<PdfReportInfo?> FindPdfUrlFromDailyReportPageAsync()
    {
        int maxDaysToLookBack = 10; // Mantener un límite razonable
        DateTime currentDate = DateTime.Today; // Empezamos desde hoy

        for (int i = 0; i < maxDaysToLookBack; i++)
        {
            // Calcular la fecha a probar
            DateTime dateToTry = currentDate.AddDays(-i);

            // --- OPTIMIZACIÓN: Saltar fines de semana ---
            // Si el día es sábado o domingo, salta más rápido al viernes anterior.
            // Esto solo es una optimización, la lógica actual ya manejaría el 404 si no se salta.
            if (dateToTry.DayOfWeek == DayOfWeek.Saturday)
            {
                dateToTry = dateToTry.AddDays(-1); // Retrocede al viernes
                i++; // Incrementa i para no contar el sábado como un intento fallido "real"
            }
            else if (dateToTry.DayOfWeek == DayOfWeek.Sunday) 
            {
                dateToTry = dateToTry.AddDays(-2); // Retrocede al viernes
                i += 2; // Incrementa i para no contar el domingo ni el sábado como intentos fallidos
            }
            // Asegurarse de que no nos pasamos del límite con esta resta adicional
            if (i >= maxDaysToLookBack) // Si al ajustar la fecha, la 'i' se pasa del límite, salir del bucle
            {
                break;
            }

            // Formatea la fecha al formato "DDMMYY".
            // Esta variable debe estar dentro del bucle para que se actualice en cada iteración.
            string dateSure = dateToTry.ToString("ddMMyy");

            // Construye la URL del informe diario basada en la fecha.
            // Esta variable también debe estar dentro del bucle.
            string reportUrl = $"{_iamcReportsBaseUrl}InformeRentaFija{dateSure}/";

            Console.WriteLine($"[DEBUG] Intentando acceder a la página del informe para la fecha: {dateSure} ({reportUrl})");

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(reportUrl);
                Console.WriteLine($"Respuesta del servidor para {dateSure}: {response.StatusCode}");

            
                // IMPORTANTE: En lugar de EnsureSuccessStatusCode(), verifica si es exitoso.
                // Si no es exitoso (ej. 404), no lanzamos excepción, sino que continuamos al día anterior.
                if (response.IsSuccessStatusCode)
                {
                    string htmlContent = await response.Content.ReadAsStringAsync();

                    // *** LÍNEA DE DEPURACIÓN: Guarda el HTML a un archivo temporal ***
                    //string tempHtmlFilePath = Path.Combine(Path.GetTempPath(), $"IAMC_Report_{dateSure}.html"); // Usar dateSure para el nombre
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
                        Console.WriteLine($"[DEBUG] El selector 'div.pdfVisualizador object' no encontró ningún elemento en la página para {dateSure}. Revisa el HTML o el selector. Saltando al día anterior.");
                        // Si no se encuentra el elemento PDF, esta fecha no sirve, continuamos con el siguiente día.
                        continue;
                    }

                    string pdfUrl = pdfObjectElement.GetAttribute("data");
                    if (!string.IsNullOrEmpty(pdfUrl))
                    {
                        Console.WriteLine($"[DEBUG] URL del PDF encontrada para {dateSure}: {pdfUrl}");
                        return new PdfReportInfo { PdfUrl = pdfUrl, ReportDate = dateToTry }; // ¡Éxito! Retorna la URL del PDF y termina la función.
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] Se encontró el elemento <object> para {dateSure}, pero su atributo 'data' es nulo o vacío. Saltando al día anterior.");
                        // Si el atributo 'data' está vacío, la URL del PDF no es válida, continuamos.
                        continue;
                    }
                }
                else
                {
                    // Si la respuesta no fue exitosa (ej. 404 Not Found), no hay informe para esta fecha.
                    Console.WriteLine($"[DEBUG] No se encontró informe para la fecha: {dateSure}. Estado HTTP: {response.StatusCode}. Intentando fecha anterior.");
                    continue; // Pasa a la siguiente iteración (día anterior).
                }

            }
            
            catch (HttpRequestException e)
            {
                Console.WriteLine($"[DEBUG] Error HTTP al acceder a la página del informe para {dateSure}: {e.Message}. Intentando fecha anterior.");
                continue; // Si hay un error de red o de solicitud, continúa con el día anterior.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error general al parsear la página del informe para {dateSure}: {ex.Message}. Intentando fecha anterior.");
                continue; // Si hay cualquier otro error, continúa con el día anterior.
            }
        }

        // Si el bucle termina después de revisar todos los días y no se encontró ningún PDF,
        // significa que no hay informes válidos en el rango.
        Console.WriteLine($"[ERROR] No se pudo encontrar un informe de renta fija válido en los últimos {maxDaysToLookBack} días.");
        return null; // La función debe retornar null si no encuentra nada.
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

    
    public async Task<RentaFijaReportResponse> GetRentaFijaDataForTodayAsync(string? tipoActivo = null)
    {
       Console.WriteLine($"[DEBUG] Iniciando proceso de extracción de datos. Parámetro tipoActivo recibido: '{tipoActivo ?? "null o vacío"}'");

        DateTime today = DateTime.Today; // Fecha de hoy (solo día, sin hora) para la comparación
      // DateTime today = new DateTime(2025, 7, 11); // Fecha de manual solo para testing


        // --- Lógica de Cacheo ---
       
        // 1. Primero, intentar encontrar el último informe disponible (sin importar si es de hoy o ayer).
          PdfReportInfo? latestReportInfo = await FindPdfUrlFromDailyReportPageAsync();
          DateTime actualReportDate;

        if (latestReportInfo == null || string.IsNullOrEmpty(latestReportInfo.PdfUrl))
        {
            Console.WriteLine("[DEBUG] No se pudo encontrar la URL de ningún PDF de informe en el rango de búsqueda. Abortando.");
            // Si no se encuentra ningún informe válido después de buscar 10 días,
            // devolvemos una respuesta de fallo.
            actualReportDate = DateTime.Today;
            return new RentaFijaReportResponse
            {
                FechaInforme = actualReportDate, // Fallback si no se encontró NADA
                Mensaje = "No se pudo encontrar la URL de ningún informe de renta fija en el rango de búsqueda."
            };
        }
        else
        {
            //Asignamos la fecha encontrada a actualReportDate
            actualReportDate = latestReportInfo.ReportDate.Date;
            Console.WriteLine($"[DEBUG] FindPdfUrlFromDailyReportPageAsync encontró: URL={latestReportInfo.PdfUrl} y Fecha={actualReportDate.ToShortDateString()}");
        }
        Console.WriteLine($"[DEBUG] el valor de actualReportDate después de encontrar la url: {actualReportDate}");


            // 2. comparamos si el informe ya esta en la cache.

            RentaFijaReportResponse? cachedResponse = DataCache.GetCache(latestReportInfo.ReportDate);


        if (cachedResponse != null)
        {
            Console.WriteLine("[DEBUG] Sirviendo datos desde la caché para la fecha del informe: " + cachedResponse.FechaInforme.ToShortDateString());
            // Si los datos están en caché y son para hoy, aplica el filtro y devuelve
            if (!string.IsNullOrEmpty(tipoActivo))
            {
                List<RentaFijaActivo> filteredActivos = cachedResponse.Activos
                    .Where(a => a.TipoActivo != null &&
                                string.Equals(a.TipoActivo, tipoActivo, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                return new RentaFijaReportResponse
                {
                    Activos = filteredActivos,
                    FechaInforme = cachedResponse.FechaInforme,
                    Mensaje = filteredActivos.Any() ? "Datos cacheados y filtrados con éxito." : "No se encontraron activos con el filtro en la caché."
                };
            }
            return cachedResponse; // Devuelve el informe completo desde la caché
        }

        // --- Fin Lógica de Cacheo ---

        // Si los datos no están en caché o no son de hoy, procede con la extracción completa
        Console.WriteLine("[DEBUG] Caché vacía o desactualizada. Procediendo a extraer nuevo informe.");

       
        // 2. Descargar el PDF
        string pdfFilePath = await DownloadPdfAsync(latestReportInfo.PdfUrl);
        if (string.IsNullOrEmpty(pdfFilePath))
        {
            Console.WriteLine("No se pudo descargar el PDF. Abortando extracción.");
            return new RentaFijaReportResponse
            {
                FechaInforme = actualReportDate, // Usamos la fecha que sí encontramos
                Mensaje = "No se pudo descargar el PDF."
            };
        }



        // 3. Extraer el texto completo del PDF usando PdfExtractionService
        Console.WriteLine($"[DEBUG] Extrayendo texto completo del PDF: {pdfFilePath}");
        string fullPdfText = _pdfExtractionService.ExtractFullTextFromPdf(pdfFilePath);

        // 4. Enviar el texto a Gemini para la extracción inteligente de los datos
        Console.WriteLine("[DEBUG] Enviando texto a Gemini para interpretación.");


        List<RentaFijaActivo> activos = 
             await _geminiApiService.ExtractRentaFijaDataFromTextAsync(fullPdfText);


        //---OPTIMIZACIÓN: Filtrar por tipo de activo si se proporciona;
        if (activos != null && activos.Any() && !string.IsNullOrEmpty(tipoActivo))
        {
            Console.WriteLine($"[DEBUG] Aplicando filtro por tipo de activo: '{tipoActivo}'");
            // Filtra la lista de activos
            activos = activos.Where(a =>
                a.TipoActivo != null &&
                string.Equals(a.TipoActivo, tipoActivo, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            Console.WriteLine($"[DEBUG] Después del filtro, se encontraron {activos.Count} activos.");
        }
        //------FIN DE OPTIMIZACIÓN


        // Eliminar el archivo PDF temporal después de procesar
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
        RentaFijaReportResponse finalResponse = new RentaFijaReportResponse
        {
            Activos = activos ?? new List<RentaFijaActivo>(),
            FechaInforme = actualReportDate,
            Mensaje = activos != null && activos.Any() ? "Datos extraídos con éxito." : "No se encontraron activos o la extracción falló."
        };

       // crear un DateTimeOffset con la fecha deseada a las 00:00:00 UTC
    // y un offset de cero.
    DateTimeOffset actualReportDateOffset = new DateTimeOffset(
        latestReportInfo.ReportDate.Year,
        latestReportInfo.ReportDate.Month,
        latestReportInfo.ReportDate.Day,
        0, 0, 0, // Hora 00:00:00
        TimeSpan.Zero // Offset de 0, lo que lo hace UTC
    );


        // --- Lógica de Cacheo (Guardar el informe completo) ---
        // Guardamos la respuesta completa (sin el filtro de tipoActivo aplicado, si lo hubo)
        // Esto permite que futuras peticiones con diferentes filtros usen la misma base cacheados.
        DataCache.SetCache(finalResponse, actualReportDateOffset);
        // --- Fin Lógica de Cacheo ---



        return finalResponse;
    }

}

