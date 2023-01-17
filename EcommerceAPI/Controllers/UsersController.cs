using EcommerceAPI.ActionFilters;
using EcommerceAPI.Data;
using EcommerceAPI.Dto;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EcommerceContext _context;

        public UsersController(EcommerceContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Email == email);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return user;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegisterDto userRegisterRequest)
        {
            if (_context.Users.Any(user => user.Email == userRegisterRequest.Email))
            {
                return BadRequest(new { message = "This email is already in use." });
            }

            CreatePasswordHash(userRegisterRequest.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = userRegisterRequest.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Your account has been successfully created." });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDto userLoginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Email == userLoginRequest.Email);

            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }

            if (!VerifyPasswordHash(userLoginRequest.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest(new { message = "Password is incorrect." });
            }

            return Ok(new { message = "Logging in your account." });
        }

        private void CreatePasswordHash(String password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmacsha = new HMACSHA512())
            {
                passwordSalt = hmacsha.Key;
                passwordHash = hmacsha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(String password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmacsha = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmacsha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
