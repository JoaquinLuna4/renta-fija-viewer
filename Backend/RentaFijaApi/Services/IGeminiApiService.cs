using RentaFijaApi.DTOs;

namespace RentaFijaApi.Services
{
    public interface IGeminiApiService
    {
        Task<List<RentaFijaActivo>> ExtractRentaFijaDataFromTextAsync(string reportTextContent);
    }
}
