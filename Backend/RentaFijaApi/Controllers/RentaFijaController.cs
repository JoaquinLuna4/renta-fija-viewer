﻿
using Microsoft.AspNetCore.Mvc;
using RentaFijaApi.Services;
using RentaFijaApi.DTOs;
using System.Collections.Generic;
using System;

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
        public async Task<ActionResult<List<RentaFijaActivo>>> GetRentaFijaData(
         [FromQuery]
        string? tipoActivo = null)
        {
        RentaFijaReportResponse response = await _rentaFijaService.GetRentaFijaDataForTodayAsync(tipoActivo);

            if (response == null || response.Activos.Count == 0)
            {
            return NotFound(response?.Mensaje ?? "No se encontraron datos de renta fija.");
        }

            return Ok(response);
        }
    }
