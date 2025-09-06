using System.Text.Json.Serialization;
using System;
using RentaFijaApi.Utilities; // Importar el namespace de los conversores

public class LetrasTesoroDto
{
    // Nombre del activo, como "Lecap" o "Lecer"
    [JsonPropertyName("Especie")]
    public string? Especie { get; set; }

    [JsonPropertyName("Fecha de Emisión")]
    public string? FechaEmision { get; set; }

    [JsonPropertyName("Fecha de Pago")]
    public string? FechaPago { get; set; }

    [JsonPropertyName("Plazo al Vto(Días)")]
    [JsonConverter(typeof(StringToIntConverter))]
    public int? PlazoAlVencimientoDias { get; set; }

    [JsonPropertyName("Monto al Vto")]
    [JsonConverter(typeof(StringToDoubleConverter))]
    public double? MontoAlVencimiento { get; set; }

    [JsonPropertyName("Tasa de licitación")]
    [JsonConverter(typeof(StringToDoubleConverter))]
    public double? TasaLicitacion { get; set; }

    [JsonPropertyName("FechaCierre")]
    public string? FechaCierre { get; set; }

    [JsonPropertyName("FechaLiquidacion")]
    public string? FechaLiquidacion { get; set; }

    [JsonPropertyName("Precio ARS c/VN 100")]
    [JsonConverter(typeof(StringToDoubleConverter))]
    public double? PrecioArsCvn100 { get; set; }

    [JsonPropertyName("Rendimiento del Período")]
    [JsonConverter(typeof(StringToDoubleConverter))]
    public double? RendimientoDelPeriodo { get; set; }

    // Campos de tasas
    [JsonPropertyName("TNA")]
    [JsonConverter(typeof(StringToDoubleConverter))]
    public double? Tna { get; set; }

    [JsonPropertyName("TEA")]
    [JsonConverter(typeof(StringToDoubleConverter))]
    public double? Tea { get; set; }

    [JsonPropertyName("TEM")]
    [JsonConverter(typeof(StringToDoubleConverter))]
    public double? Tem { get; set; }

    // Duración de Macaulay
    [JsonPropertyName("DM")]
    [JsonConverter(typeof(StringToDoubleConverter))]
    public double? Dm { get; set; }
}