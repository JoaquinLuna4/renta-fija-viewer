namespace RentaFijaApi.DTOs
{
    public class RentaFijaActivo
    {
        public string TipoActivo { get; set; } // Ej: "BONTE", "BONAR", "LECAP", "BOPREAL", "ON", "BONOS DE CONSOLIDACIÓN"
        public string NombreCompleto { get; set; } // Nombre largo del activo, ej. "BONAR STEP-UP USD Ley Arg 2029"
        public string CodigoTicker { get; set; } // El código corto, ej. "AL29", "GD30", "TX26"
        public DateTime? Vencimiento { get; set; } // Podría ser DateTime si lo parseas
        //public string Moneda { get; set; } // ARS, USD, etc. (Si lo extraes)
        //public string Clase { get; set; } // Ley Arg, Ley NY, etc. (Si lo extraes)
        public decimal? Cotizacion { get; set; } // Usamos decimal? para permitir nulos si no siempre está presente
        public decimal? TirAnual { get; set; } // Rendimiento Anual
        public decimal? Paridad { get; set; } // Paridad
        public DateTime? FechaUltimaCotizacion { get; set; } // Ultima cotizacion

        // Puedes agregar más campos según lo que necesites del PDF
        // Por ejemplo: Duracion, Tasa, FrecuenciaPago, etc.

        // Constructor vacío, útil para la deserialización y creación de objetos
        public RentaFijaActivo() { }

        // Opcional: Un método ToString() para facilitar la depuración y ver el objeto en consola
        public override string ToString()
        {
            return $"Tipo: {TipoActivo ?? "N/A"}, Ticker: {CodigoTicker ?? "N/A"}, Nombre: {NombreCompleto ?? "N/A"}, Vto: {Vencimiento?.ToString("dd-MMM-yy") ?? "N/A"}, Cotiz: {Cotizacion?.ToString("F2") ?? "N/A"}, TIR: {TirAnual?.ToString("F2") ?? "N/A"}%, Paridad: {Paridad?.ToString("F2") ?? "N/A"}%";
        }
    }
}
