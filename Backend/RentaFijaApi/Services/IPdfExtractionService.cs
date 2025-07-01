using RentaFijaApi.DTOs;

namespace RentaFijaApi.Services // Ajusta tu namespace si es diferente
{
    public interface IPdfExtractionService
    {
        List<RentaFijaActivo> ExtractRentaFijaData(string pdfFilePath);
    }
}