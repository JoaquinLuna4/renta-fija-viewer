namespace RentaFijaApi.DTOs
{
    public class RentaFijaReportResponse
    {
        public List<RentaFijaActivo> Activos { get; set; } = new List<RentaFijaActivo>();
        public DateTime FechaInforme { get; set; }
        public string? Mensaje { get; set; }

    }
}
