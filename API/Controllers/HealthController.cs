using System;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class HealthController(IUniteOfWork unit) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var spec = new HealthCheckSpec();
            var any = await unit.Repository<Product>().CountAsync(spec); 
            
            return Ok(new
                {
                    status = "Healthy",
                    db = any >= 0 ? "Connected" : "No data"
                });
            

        }
        catch (Exception ex)
        {

            return StatusCode(500, new { status = "Unhealthy", error = ex.Message });
        }
    }
}
