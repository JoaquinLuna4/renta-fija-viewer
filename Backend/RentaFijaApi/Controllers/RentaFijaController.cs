
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System; // Para DateTime.Today

[ApiController]
[Route("[controller]")]
public class RentaFijaController : ControllerBase
{
    private readonly RentaFijaService _rentaFijaService;

    public RentaFijaController(RentaFijaService rentaFijaService)
    {
        _rentaFijaService = rentaFijaService;
    }


    [HttpGet("data")]
    public async Task<IActionResult> GetRentaFijaData()
    {
        //DateTime targetDate = DateTime.Today.AddDays(-1);
        string dateSure = "240625";

        // Paso 1: Encontrar la URL del PDF del informe de hoy
        string pdfUrl = await _rentaFijaService.FindPdfUrlFromDailyReportPageAsync( dateSure);

        
        if (string.IsNullOrEmpty(pdfUrl))
        {
            return StatusCode(500, "No se pudo encontrar la URL del PDF para la fecha actual. La página del informe puede no existir o su estructura ha cambiado.");
        }
        else if (!pdfUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("La URL obtenida no es un PDF válido.");
        }
        else if (!Uri.IsWellFormedUriString(pdfUrl, UriKind.Absolute))
        {
            return BadRequest("La URL obtenida no es válida.");
        }

        //Validacion de modulo Find
        //else return StatusCode(200, pdfUrl);

        //// Paso 2: Descargar el PDF usando la URL encontrada
       
        
        Console.WriteLine($"[DEBUG] Intentando descargar el PDF desde: {pdfUrl}"); // Mensaje de depuración para la descarga
        
        string pdfFilePath = await _rentaFijaService.DownloadPdfAsync(pdfUrl);
        if (string.IsNullOrEmpty(pdfFilePath))
        {
            return StatusCode(500, "No se pudo descargar el PDF desde la URL obtenida.");
        }

        //Validacion de modulo download
        //return Ok($"PDF descargado con éxito en: {pdfFilePath}. Listo para extraer datos.");

        // Paso 3: Extraer los datos del PDF
        Console.WriteLine($"[DEBUG] Iniciando extracción de datos del PDF: {pdfFilePath}"); // Mensaje de depuración antes de la extracción
        var data = _rentaFijaService.ExtractRentaFijaData(pdfFilePath);

        if (data != null && data.Count > 0) // Usamos Count > 0 en lugar de Any() si no tienes System.Linq
        {
            Console.WriteLine($"[DEBUG] Extracción completada. Se encontraron {data.Count} activos.");
            return Ok(data); // Devuelve los datos extraídos como respuesta exitosa
        }
        else
        {
            Console.WriteLine("[DEBUG] No se encontraron datos de renta fija o hubo un problema al extraerlos del PDF.");
            return NotFound("No se encontraron datos de renta fija o hubo un problema al extraerlos del PDF."); // Devuelve 404 si no hay datos
        }
    }
}