using Auth.API.DTO;
using AuthClassLibray.DAL.Repositories;
using AuthClassLibray.Data;
using AuthClassLibray.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        public AuthController(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _userRepository.UserExistsAsync(dto.Username.ToLower()))
                return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();

            var user = new User
            {
                Username = dto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)),
                PasswordSalt = hmac.Key,
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role
            };

            await _userRepository.AddUserAsync(user);

            return Ok(new { message = "User registered successfully" });
        }
        [HttpPost("login")]
       
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userRepository.GetByUsernameAsync(dto.Username.ToLower());
            if (user == null) return Unauthorized("Invalid username or password");

            // Validate salt length before using it
            if (user.PasswordSalt == null || user.PasswordSalt.Length < 64)
                return Unauthorized("Invalid username or password"); // or handle as you prefer

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                    return Unauthorized("Invalid username or password");
            }

            var token = _tokenService.CreateToken(user);
            return Ok(new { token });
        }

        // Test role-based auth
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnly()
        {
            return Ok("Hello Admin!");
        }
    }

}
