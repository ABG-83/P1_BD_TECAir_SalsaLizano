using Microsoft.AspNetCore.Mvc;
using TECAir.Core.DTOs.Users;
using TECAir.Core.Interfaces;

namespace TECAir.API.Controllers
{
    /// <summary>
    /// Exposes CRUD endpoints for user management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        // GET api/users
        /// <summary>Returns all registered users.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET api/users/{id}
        /// <summary>Returns a single user by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        // GET api/users/by-email?email=...
        /// <summary>Returns a single user by email address.</summary>
        [HttpGet("by-email")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            return user is null ? NotFound() : Ok(user);
        }

        // POST api/users
        /// <summary>Creates a new user account.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] UserRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int newId = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newId }, new { userId = newId });
        }

        // PUT api/users/{id}
        /// <summary>Updates mutable profile fields of an existing user.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UserRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool updated = await _userService.UpdateUserAsync(id, dto);
            return updated ? NoContent() : NotFound();
        }

        // DELETE api/users/{id}
        /// <summary>Permanently removes a user account.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _userService.DeleteUserAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
