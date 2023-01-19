using AutoMapper;
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
        private readonly IMapper _mapper;

        public UsersController(EcommerceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();

            var usersDetailDto = _mapper.Map<List<UserDetailDto>>(users);

            return Ok(new { resultCount = usersDetailDto.Count, users = usersDetailDto });
        }

        // GET: api/Users/Email/user@example.com
        [HttpGet("Email/{email}")]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Email == email);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var userDetailDto = _mapper.Map<UserDetailDto>(user);

            return Ok(new { resultCount = 1, users = userDetailDto }); ;
        }

        // POST: api/Users/Register
        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register([FromBody] UserRegisterDto userRegisterRequest)
        {
            if (_context.Users.Any(user => user.Email == userRegisterRequest.Email))
            {
                return BadRequest(new { message = "This email is already in use." });
            }

            CreatePasswordHash(userRegisterRequest.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = _mapper.Map<UserRegisterDto, User>(userRegisterRequest, opt =>
            {
                opt.AfterMap((userRegisterRequest, user) =>
                {
                    user.CreatedAt = DateTime.Now;
                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                });
            });

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Your account has been successfully created." });
        }

        // POST: api/Users/Login
        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] UserLoginDto userLoginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Email == userLoginRequest.Email);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (!VerifyPasswordHash(userLoginRequest.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest(new { message = "Password is incorrect." });
            }

            return Ok(new { message = "Logging in your account." });
        }

        // GET : api/Users/1/Cart
        [HttpGet("{userId}/Cart")]
        public async Task<ActionResult> GetUserCart(int userId)
        {
           var users = await _context.Users
                                .Include(u => u.Carts)
                                    .ThenInclude(c => c.Product)
                                .Where(u => u.UserId == userId).ToListAsync();

            var userDto = _mapper.Map<List<UserDto>>(users);

            return Ok(new { users = userDto });
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
