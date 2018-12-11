using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BloggingApp.Models;
using BloggingApp.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BloggingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BloggingContext _context;

        public AuthController(BloggingContext context)
        {
            _context = context;
        }

        // POST: api/Users
        [HttpPost("signup")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Users.Any(u => u.Email == user.Email))
                return Conflict("User with this e-mail already exists!");

            if (_context.Users.Any(u => u.Username == user.Username))
                return Conflict("User with this username already exists!");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Created($"api/resource/{user.Id}", user);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> LogIn([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var targetUser = _context.Users.FirstOrDefault(t => t.Email == user.Email);

            if (targetUser == null)
            {
                return NotFound();
            }

            if (targetUser.Password != user.Password)
            {
                return Unauthorized();
            }

            var identity = GetIdentity(targetUser);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return Ok($"You have logged in as {targetUser.Username}");
        }

        private ClaimsIdentity GetIdentity(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email)
            };
            ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return claimsIdentity;
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok("You have logged out successfully");
        }
    }
}