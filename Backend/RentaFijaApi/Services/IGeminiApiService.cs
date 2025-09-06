using RentaFijaApi.DTOs;

namespace RentaFijaApi.Services
{
    public interface IGeminiApiService
    {
        Task<List<LetrasTesoroDto>> ExtractRentaFijaDataFromTextAsync(string reportTextContent);
    }
}
