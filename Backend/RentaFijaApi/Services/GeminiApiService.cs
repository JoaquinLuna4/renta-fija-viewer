using RentaFijaApi.DTOs;
using System.Text.Json; // Para System.Text.Json.JsonSerializer
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using static RentaFijaApi.DTOs.GeminiDTOs;
using System.Text.RegularExpressions; // Para Exception

namespace RentaFijaApi.Services
{
    // Define una interfaz para el servicio de Gemini, lo cual es opcional pero recomendable
    // para facilitar la inyección de dependencias y los tests unitarios.
   

    public class GeminiApiService : IGeminiApiService
    {
        // Si necesitas un HttpClient para llamar a la API de Gemini (lo más probable)
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey; // Para tu clave API de Gemini
        private readonly string _geminiApiUrl; // La URL base de la API de Gemini
        private readonly bool _useSimulation; // Para simular respuestas sin llamar a la API real
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        // Constructor para inyectar HttpClient y la clave API (se configura en Startup.cs o Program.cs)
        public GeminiApiService(HttpClient httpClient, string geminiApiKey, string geminiApiUrl, bool useSimulation = false)
        {
            _httpClient = httpClient;
            _geminiApiKey = geminiApiKey;
            _geminiApiUrl = geminiApiUrl;
            _useSimulation = useSimulation;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, // Permite que las propiedades en el JSON coincidan sin importar mayúsculas/minúsculas
                WriteIndented = true, // Para que el JSON de salida sea legible (opcional, solo para debug)
                
            };
        }

        // El método principal que recibe el texto y llama a Gemini
        public async Task<List<RentaFijaActivo>> ExtractRentaFijaDataFromTextAsync(string reportTextContent)
        {
            // 2. Preparar el prompt para Gemini
            string geminiPrompt = $@"Eres un asistente experto en análisis financiero y extracción de datos de informes de renta fija. Tu tarea es analizar el siguiente texto de un informe y extraer la información de los activos listados en las tablas,
                                    específicamente para la sección de 'Renta Fija'. Solo debes analizar y extraer los datos de la hoja 2.

                                    Para cada activo que encuentres, necesito los siguientes campos:
                                    - **NombreCompleto**: El nombre completo del bono, que usualmente se encuentra a la izquierda del Ticker. En el documento figura en la columna BONO.
                                    - **Ticker:** El código de identificación del activo (ej. 'T2X5', 'PNDCO').
                                    - **Vencimiento:** La fecha de vencimiento del activo en formato AAAA-MM-DD (ej. '2025-05-23').
                                    - **Cotización:** El precio de cotización del activo. Debe ser un número decimal, sin símbolos de moneda.
                                    - **TIR Anual:** La Tasa Interna de Retorno anual, **que se encuentra bajo la columna 'TIR ANUAL' en el documento**. Debe ser un número decimal, sin el símbolo '%'. Generalmente se encuentra a la derecha de Paridad.
                                    - **Paridad:** La paridad del activo, **que se encuentra bajo la columna 'PARIDAD' en el documento**. Debe ser un número decimal, sin el símbolo '%'.

                                    Si un campo no se encuentra para un activo específico, por favor, devuélvelo como `null`.
                                    Asegúrate de que todos los valores numéricos usen el punto como separador decimal.

                                    La respuesta DEBE ser SOLAMENTE el array de objetos JSON. No incluyas ningún texto introductorio, explicaciones, ni bloques de código Markdown (```). Un maximo de 50 activos.

                                    ---

                                    Texto del Informe:
                                    {reportTextContent}
                                    ";

            // 3. Llamar a la API de Gemini
            string jsonResponse = string.Empty;
            string apiResponse = string.Empty;
            if (_useSimulation)
            {
                Console.WriteLine("[DEBUG] *** Modo simulación de Gemini ACTIVO ***");
                jsonResponse = await SimulateGeminiResponse(); 
            }
            else
            {
                Console.WriteLine("[DEBUG] *** Modo REAL de Gemini ACTIVO ***");
                try
                {
                    // Construye el cuerpo de la solicitud JSON para la API de Gemini
                    var requestBody = new
                    {
                        contents = new[]
                        {
                        new
                        {
                            parts = new[]
                            {
                                new { text = geminiPrompt }
                            }
                        }
                    }
                    };

                    // Serializa el cuerpo a JSON
                    string jsonContent = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                    // Prepara la URL de la API (ej. para Gemini 1.5 Flash)
                    // Asegúrate de usar la región correcta (ej. us-central1) y tu project ID

                    //Generando la url completa
                    string fullApiUrl = $"{_geminiApiUrl}/models/gemini-1.5-flash:generateContent?key={_geminiApiKey}";

                    Console.WriteLine($"[DEBUG] Enviando solicitud a Gemini API: {fullApiUrl}");

                    HttpResponseMessage response = await _httpClient.PostAsync(fullApiUrl, content);
                    response.EnsureSuccessStatusCode(); // Lanza excepción si la respuesta no es 2xx

                    apiResponse = await response.Content.ReadAsStringAsync();

                    var geminiRootResponse = JsonSerializer.Deserialize<GeminiRootResponse>(apiResponse, _jsonSerializerOptions);

                    string rawExtractedJson = string.Empty;
                    if (geminiRootResponse?.Candidates != null && geminiRootResponse.Candidates.Any())
                    {
                        // Accede a la propiedad 'text' que contiene tu JSON deseado
                        rawExtractedJson = geminiRootResponse.Candidates.First().Content.Parts.First().Text;
                    }
                    else
                    {
                        // Si la estructura no es la esperada, registra el error y maneja
                        Console.WriteLine("[ERROR] Estructura de respuesta de Gemini inesperada. No se encontraron candidatos.");
                        return new List<RentaFijaActivo>();
                    }

                    if (rawExtractedJson != null)
                    {
                        string cleanedJsonResponse = rawExtractedJson;


                        // Ahora, limpia la cadena JSON extraída de las envolturas de Markdown

                        // ------------------- Codigo para eliminar la envoltura de la respuesta de Gemini--------------------\\

                        // Primero, elimina el inicio del bloque de código Markdown (```json) si existe
                        if (cleanedJsonResponse.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                        {
                            cleanedJsonResponse = cleanedJsonResponse.Substring("```json".Length);
                        }

                        // *** ¡NUEVA LÓGICA DE LIMPIEZA MÁS ROBUSTA PARA EL FINAL! ***
                        // Busca el último carácter ']' que indica el final del array JSON.
                        int lastBracketIndex = cleanedJsonResponse.LastIndexOf(']');
                        if (lastBracketIndex != -1)
                        {
                            // Si encuentra un ']', toma la subcadena hasta (e incluyendo) ese corchete.
                            // Esto descarta cualquier cosa después del JSON válido, incluyendo los '```' y saltos de línea.
                            cleanedJsonResponse = cleanedJsonResponse.Substring(0, lastBracketIndex + 1);
                        }

                        cleanedJsonResponse = cleanedJsonResponse.Trim(); // Elimina cualquier espacio en blanco, saltos de línea, etc. al principio o final

                        Console.WriteLine($"[DEBUG] JSON de Gemini recibido (limpio final para deserialización): {cleanedJsonResponse}");

                        jsonResponse = cleanedJsonResponse; // Asigna el JSON limpio para la deserialización final
                                                            // ------------------- FIN código para eliminar la envoltura de la respuesta ------------------- \\

                    }
                    }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error HTTP al llamar a la API de Gemini: {ex.Message}");
                    throw; // Vuelve a lanzar la excepción para propagar el error
                }
                catch (System.Text.Json.JsonException ex)
                {
                    // Este catch es CRUCIAL para el error de JSON truncado o malformado
                    Console.WriteLine($"[ERROR] Error de deserialización JSON: {ex.Message}");
                    Console.WriteLine($"[DEBUG] JSON que causó el error (posiblemente truncado):\n{jsonResponse}");
                    Console.WriteLine($"JSON recibido (problema de deserialización RAW): {apiResponse}"); // Registra el JSON original problemático
                    throw new ApplicationException("No se pudo procesar la respuesta de la IA. El formato JSON es inválido o está incompleto.", ex);
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"Error inesperado en la llamada a la API de Gemini: {ex.Message}");
                    throw;
                }


            }


            // 4. Parsear la respuesta JSON de Gemini a tu lista de RentaFijaActivo DTOs
            try
            {
                // Finalmente, deserializa la cadena JSON limpia a tu lista de DTOs
                var activos = JsonSerializer.Deserialize<List<RentaFijaActivo>>(jsonResponse, _jsonSerializerOptions);
                if (activos != null)
                {
                    Console.WriteLine($"[DEBUG] Proceso de extracción finalizado. Se encontraron {activos.Count} activos.");
                    foreach (var activo in activos)
                    {
                        AssignTipoActivo(activo); // Llama al método para asignar el tipo
                    }
                    return activos;
                }
                Console.WriteLine("[DEBUG] Deserialización a lista de activos resultó en null.");
                return new List<RentaFijaActivo>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error al deserializar la respuesta JSON de Gemini a DTOs: {ex.Message}");
                Console.WriteLine($"JSON recibido (problema de deserialización final): {jsonResponse}"); // Registra el JSON limpio que falló
                throw; // Vuelve a lanzar la excepción
            }
        }

        //Este es un método simulado para pruebas locales rápidas sin llamar a la API
        private async Task<string> SimulateGeminiResponse()
        {
            Console.WriteLine("[DEBUG] Devolviendo datos de simulación.");
            await Task.Delay(50); // Simular una pequeña latencia

            return @"[
       {
    ""NombreCompleto"": ""O.N. VISTA CLASE XXVI VTO 2031"",
    ""Ticker"": ""VSCRO"",
    ""Vencimiento"": ""2031-10-10"",
    ""Cotización"": 100.12999,
    ""TIR Anual"": 4.23,
    ""Paridad"": 101.74
  },
  {
    ""NombreCompleto"": ""YPF Cl. XXIX Vto. 2025"",
    ""Ticker"": ""YCA6O"",
    ""Vencimiento"": ""2025-07-28"",
    ""Cotización"": 100.38395,
    ""TIR Anual"": null,
    ""Paridad"": 101.443
  },
  {
    ""NombreCompleto"": ""MASTELLONE HNOS. Clase G en U$S"",
    ""Ticker"": ""MTCGO"",
    ""Vencimiento"": ""2026-06-30"",
    ""Cotización"": 100.1255,
    ""TIR Anual"": 0.93,
    ""Paridad"": 100.061
  },
    ]";
        }
        private void AssignTipoActivo(RentaFijaActivo asset)
        {
            if (asset == null || string.IsNullOrWhiteSpace(asset.CodigoTicker))
            {
                // Si no hay Ticker, no podemos clasificar bien, lo asignamos a "Otro" o lanzamos excepción
                asset.TipoActivo = "Otro";
                return;
            }

            if (asset.CodigoTicker.StartsWith("TX", StringComparison.OrdinalIgnoreCase) ||
                (asset.NombreCompleto != null && asset.NombreCompleto.Contains("BONTE", StringComparison.OrdinalIgnoreCase)))
            {
                asset.TipoActivo = "BONTE";
            }
            else if (asset.CodigoTicker.StartsWith("AL", StringComparison.OrdinalIgnoreCase) ||
                     asset.CodigoTicker.StartsWith("GD", StringComparison.OrdinalIgnoreCase) ||
                     (asset.NombreCompleto != null && (asset.NombreCompleto.Contains("BONAR", StringComparison.OrdinalIgnoreCase) ||
                                                     asset.NombreCompleto.Contains("GLOBAL", StringComparison.OrdinalIgnoreCase))))
            {
                asset.TipoActivo = "BONAR";
            }
            else if (asset.NombreCompleto != null && asset.NombreCompleto.Contains("BONOS DE CONSOLIDACION", StringComparison.OrdinalIgnoreCase))
            {
                asset.TipoActivo = "BONOS DE CONSOLIDACIÓN";
            }
            else if (asset.CodigoTicker.StartsWith("TZX", StringComparison.OrdinalIgnoreCase) ||
                     (asset.NombreCompleto != null && asset.NombreCompleto.Contains("BONO TES. NAC.", StringComparison.OrdinalIgnoreCase) &&
                                                     asset.NombreCompleto.Contains("CER", StringComparison.OrdinalIgnoreCase)))
            {
                asset.TipoActivo = "BONO TES. NAC. CER";
            }
            else if (asset.CodigoTicker.StartsWith("T", StringComparison.OrdinalIgnoreCase) &&
                     (asset.NombreCompleto != null && asset.NombreCompleto.Contains("BONO CAP", StringComparison.OrdinalIgnoreCase)))
            {
                asset.TipoActivo = "BONOCAP";
            }
            else if (asset.CodigoTicker.StartsWith("S", StringComparison.OrdinalIgnoreCase) &&
                     (asset.NombreCompleto != null && asset.NombreCompleto.Contains("LECAP", StringComparison.OrdinalIgnoreCase)))
            {
                asset.TipoActivo = "LECAP";
            }
            else if (asset.NombreCompleto != null && asset.NombreCompleto.Contains("BOPREAL", StringComparison.OrdinalIgnoreCase))
            {
                asset.TipoActivo = "BOPREAL";
            }
            else if ((asset.NombreCompleto != null && (asset.NombreCompleto.Contains("O.N.", StringComparison.OrdinalIgnoreCase) ||
                                                     asset.NombreCompleto.Contains("OBLIGACION NEGOCIABLE", StringComparison.OrdinalIgnoreCase))) ||
                     (asset.CodigoTicker != null && Regex.IsMatch(asset.CodigoTicker, @"[OV]$", RegexOptions.IgnoreCase)))
            {
                asset.TipoActivo = "OBLIGACION NEGOCIABLE";
            }
            else
            {
                asset.TipoActivo = "Otro";
            }
        }
    }
}