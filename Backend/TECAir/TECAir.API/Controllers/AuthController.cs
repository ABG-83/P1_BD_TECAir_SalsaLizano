// =============================================================================
// File    : AuthController.cs
// Layer   : TECAir.API → Controllers
// Purpose : Exposes HTTP endpoints for auth operations.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TECAir.Core.DTOs.Auth;
using TECAir.Core.Interfaces;

namespace TECAir.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        // POST /api/auth/login
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
    }
}
