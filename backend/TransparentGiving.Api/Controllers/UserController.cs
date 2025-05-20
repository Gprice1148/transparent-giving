using Microsoft.AspNetCore.Mvc;
using TransparentGiving.Api.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TransparentGiving.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        private static readonly List<User> _users = new();
        
        public UserController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (_users.Any(u => u.Email == request.Email))
            {
                return BadRequest("User with that email already exists.");
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password)
            };

            _users.Add(user);

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            var hashed = HashPassword(request.Password);
            if (user.PasswordHash != hashed)
                return Unauthorized("Invalid email or password.");

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful",
                token
            });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = _users.FirstOrDefault(u => u.Id.ToString() == userId);
            if (user == null)
                return NotFound("User not found.");

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email
            });
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name)
                },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public class RegisterRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}
