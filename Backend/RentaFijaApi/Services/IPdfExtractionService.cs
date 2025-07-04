using RentaFijaApi.DTOs;

namespace RentaFijaApi.Services{
    public interface IPdfExtractionService
    {
        string ExtractFullTextFromPdf(string pdfFilePath);
    }
}