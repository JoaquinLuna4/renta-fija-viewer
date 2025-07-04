
using Microsoft.AspNetCore.Mvc;
using RentaFijaApi.Services;
using RentaFijaApi.DTOs;

[ApiController]
[Route("[controller]")]
public class RentaFijaController : ControllerBase
{
   private readonly RentaFijaService _rentaFijaService;

        public RentaFijaController(RentaFijaService rentaFijaService)
        {
            _rentaFijaService = rentaFijaService;
        }

        [HttpGet()]
        public async Task<ActionResult<List<RentaFijaActivo>>> GetRentaFijaData()
        {
            var activos = await _rentaFijaService.GetRentaFijaDataForTodayAsync();

            if (activos == null || activos.Count == 0)
            {
                return NotFound("No se encontraron datos de renta fija para el informe actual.");
            }

            return Ok(activos);
        }
    }
