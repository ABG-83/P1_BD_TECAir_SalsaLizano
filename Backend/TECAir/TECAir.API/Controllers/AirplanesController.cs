using Microsoft.AspNetCore.Mvc;
using TECAir.Data.Interfaces;

namespace TECAir.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AirplanesController(IAirplaneRepository airplaneRepository) : ControllerBase
    {
        private readonly IAirplaneRepository _airplaneRepository = airplaneRepository;

        // GET /api/airplanes
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var airplanes = await _airplaneRepository.GetAllAsync();
            return Ok(airplanes);
        }

        // GET /api/airplanes/{plateNumber}
        [HttpGet("{plateNumber}")]
        public async Task<IActionResult> GetByPlateNumber(string plateNumber)
        {
            var airplane = await _airplaneRepository.GetByPlateNumberAsync(plateNumber);
            if (airplane is null)
                return NotFound(new { message = $"No se encontró el avión '{plateNumber}'." });
            return Ok(airplane);
        }
    }
}
