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
        public async Task<List<LetrasTesoroDto>> ExtractRentaFijaDataFromTextAsync(string reportTextContent)
        {
            // 2. Preparar el prompt para Gemini
            string geminiPrompt = $@"Eres un asistente experto en análisis financiero y extracción de datos. Tu tarea es analizar el siguiente texto de un informe de letras del tesoro y cauciones.

                                    **Reglas Clave de Extracción:**
                                    * Para todos los campos numéricos, extrae el valor exacto como aparece en el texto.
                                    * No realices conversiones matemáticas. Si el valor es 3.98%, debes extraerlo como 3.98. Si es 159.73, lo extraes como 159.73.
                                    * Elimina cualquier símbolo como '%', '$', o comas de miles. Usa siempre un punto como separador decimal.
                                    * Extrae la información campo por campo, asegurándote de que cada valor corresponde a su columna correcta.

                                    Para cada activo que encuentres en las tablas, extrae la siguiente información:
                                    - **Especie**: El nombre o código del activo (ej. 'LECAC', 'S30S5').
                                    - **Fecha de Emisión**: La fecha en que se emitió el activo.
                                    - **Fecha de Pago**: La fecha en que se espera el pago.
                                    - **Plazo al Vto(Días)**: El número de días hasta el vencimiento.
                                    - **Monto al Vto**: El monto total al vencimiento.
                                    - **Tasa de licitación**: La tasa de licitación.
                                    - **FechaCierre**: La fecha de cierre.
                                    - **FechaLiquidacion**: La fecha de liquidación.
                                    - **Precio ARS c/VN 100**: El precio en pesos por un valor nominal de 100.
                                    - **Rendimiento del Período**: El rendimiento del activo en el período.
                                    - **TNA**: La Tasa Nominal Anual.
                                    - **TEA**: La Tasa Efectiva Anual.
                                    - **TEM**: La Tasa Efectiva Mensual.
                                    - **DM**: La duración de Macaulay.

                                    Si un campo no se encuentra para un activo, devuélvelo como `null`.

                                    La respuesta DEBE ser SOLAMENTE un array de objetos JSON. No incluyas ningún texto introductorio, explicaciones, ni bloques de código Markdown (```).

                                    ---

                                    Texto del Informe:
                                    {reportTextContent}
                                    "; ; ;

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

                    if (!response.IsSuccessStatusCode) // Lanza excepción si la respuesta no es 2xx
                    {
                        apiResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[ERROR] La API de Gemini respondió con código de estado HTTP no exitoso: {(int)response.StatusCode} {response.ReasonPhrase}");
                        Console.WriteLine($"[ERROR] Contenido de la respuesta de error de Gemini: {apiResponse}");

                        if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) // 503
                        {
                            Console.WriteLine("[ERROR] La API de Gemini no está disponible (503 Service Unavailable).");
                            throw new ApplicationException("El servicio de Gemini no está disponible temporalmente (503). Por favor, inténtelo de nuevo más tarde.", new HttpRequestException($"Gemini 503: {apiResponse}"));
                        }

                        else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429
                        {
                            Console.WriteLine("[ERROR] Se excedió el límite de solicitudes a la API de Gemini (429 Too Many Requests).");
                            throw new ApplicationException("Se ha excedido el límite de uso para la API de Gemini (429).", new HttpRequestException($"Gemini 429: {apiResponse}"));
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden) // 401/403
                        {
                            Console.WriteLine("[ERROR] Problema de autenticación/autorización con la API de Gemini (401/403). Revise su clave API.");
                            throw new ApplicationException("Error de autenticación con la API de Gemini (401/403).", new HttpRequestException($"Gemini 401/403: {apiResponse}"));
                        }
                        else
                        {
                            // Para cualquier otro error HTTP no exitoso
                            throw new HttpRequestException($"La API de Gemini devolvió un estado no exitoso: {(int)response.StatusCode} {response.ReasonPhrase}");
                        }
                    }
                
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
                        return new List<LetrasTesoroDto>();
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
                catch (HttpRequestException ex) // Para errores de red subyacentes antes de obtener un StatusCode
                {
                    Console.WriteLine($"[ERROR] Error de red al intentar conectar con la API de Gemini: {ex.Message}");
                    throw new ApplicationException("Error de conexión al intentar comunicarse con la API de Gemini.", ex);
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


            // 4. Parsear la respuesta JSON de Gemini a tu lista de LetrasTesoro DTOs
            try
            {
                // Finalmente, deserializa la cadena JSON limpia a tu lista de DTOs
                var activos = JsonSerializer.Deserialize<List<LetrasTesoroDto>>(jsonResponse, _jsonSerializerOptions);

                if (activos != null)
                {
                    Console.WriteLine($"[DEBUG] Deserialización exitosa. Se encontraron {activos.Count} activos.");
                    return activos; // Devuelve la lista de activos deserializada
                }
                else
                {
                    // Esto puede ocurrir si el JSON de entrada es literalmente la cadena "null"
                    Console.WriteLine("[ERROR] La deserialización del JSON de activos ha resultado en null.");
                    return new List<LetrasTesoroDto>(); // Devuelve una lista vacía en este caso
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[ERROR] Error al deserializar la respuesta JSON de Gemini a DTOs: {ex.Message}");
                Console.WriteLine($"[DEBUG] JSON que causó el error: {jsonResponse}"); // Registra el JSON limpio que falló
                throw; // Vuelve a lanzar la excepción para que el llamador sepa que algo salió mal
            }
        }

        //Este es un método simulado para pruebas locales rápidas sin llamar a la API
        private async Task<string> SimulateGeminiResponse()
        {
            Console.WriteLine("[DEBUG] Devolviendo datos de simulación.");
            await Task.Delay(50); // Simular una pequeña latencia
                                  // Obtiene la ruta al directorio de ejecución de la aplicación
            string baseDirectory = AppContext.BaseDirectory;


            // Combina con la ruta relativa de tu archivo JSON
            string filePath = Path.Combine(baseDirectory, "Data", "simulated_gemini_response.json");

            if (File.Exists(filePath))
            {
                // Lee todo el contenido del archivo como una cadena
                string jsonContent = await File.ReadAllTextAsync(filePath);
                return jsonContent;
            }
            else
            {
                Console.WriteLine($"[ERROR] Archivo de simulación no encontrado: {filePath}");
                
                return "[]"; // Retorna un array JSON vacío si no se encuentra el archivo
            }
        }
      
    }
}