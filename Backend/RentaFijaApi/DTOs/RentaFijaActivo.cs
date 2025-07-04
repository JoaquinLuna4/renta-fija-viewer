using System;
using System.Text.Json.Serialization; // ¡Este using es crucial!

namespace RentaFijaApi.DTOs // Verifica que este sea el namespace correcto
{
    public class RentaFijaActivo
    {
        public string? NombreCompleto { get; set; }
        [JsonPropertyName("Ticker")]
        public string? CodigoTicker { get; set; } // O simplemente 'Ticker' si prefieres ese nombre en C#

        [JsonPropertyName("Vencimiento")]
        public string? Vencimiento { get; set; } // Considera usar DateTime? si el formato de fecha es consistente

        [JsonPropertyName("Cotización")] // Mapea el campo del JSON con tilde
        public double? Cotizacion { get; set; } // Nombre en C# sin tilde, más estándar

        [JsonPropertyName("TIR Anual")] // Mapea el campo del JSON con espacio
        public double? TirAnual { get; set; } // Nombre en C# en camelCase, más estándar

        [JsonPropertyName("Paridad")]
        public double? Paridad { get; set; }

        // Estos campos pueden permanecer, pero seguirán siendo null si Gemini no los envía
        public string? TipoActivo { get; set; }
        [JsonPropertyName("Fecha Ultima Cotizacion")]
        public string? FechaUltimaCotizacion { get; set; }
    }
}