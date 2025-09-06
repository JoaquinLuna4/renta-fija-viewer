namespace RentaFijaApi.DTOs
{
    public class RentaFijaReportResponse
    {
        public List<LetrasTesoroDto> Activos { get; set; } = new List<LetrasTesoroDto>();
        public DateTime FechaInforme { get; set; }
        public string? Mensaje { get; set; }

    }
}
