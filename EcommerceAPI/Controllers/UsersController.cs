using AutoMapper;
using EcommerceAPI.Configuration;
using EcommerceAPI.Data;
using EcommerceAPI.Dto;
using EcommerceAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EcommerceContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public UsersController(EcommerceContext context, IMapper mapper, IConfiguration configuration, TokenValidationParameters tokenValidationParameters)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _tokenValidationParameters = tokenValidationParameters;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();

            var usersDetailDto = _mapper.Map<List<UserDetailDto>>(users);

            return Ok(new { resultCount = usersDetailDto.Count, users = usersDetailDto });
        }

        // GET: api/Users/Email/user@example.com
        [HttpGet("Email/{email}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

            var tokenString = await GenerateJwtToken(user);

            return Ok(new { message = "Your account has been successfully created.", token = tokenString });
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

            var tokenString = await GenerateJwtToken(user);

            return Ok(new { message = "Logging in your account.", token = tokenString });
        }

        // POST: api/Users/RefreshToken
        [HttpPost("RefreshToken")]
        public async Task<ActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequest)
        {
            var result = await VerifyAndGenerateToken(tokenRequest);

            if (result == null)
            {
                return BadRequest(new { message = "Invalid tokens." });
            }


            return Ok(new { message = "Token Refreshed.", token = result });
        }

        // GET : api/Users/1/Cart
        [HttpGet("{userId}/Cart")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetUserCart(int userId)
        {
            var users = await _context.Users
                                 .Include(u => u.Carts)
                                     .ThenInclude(c => c.Product)
                                 .Where(u => u.UserId == userId).ToListAsync();

            var userDto = _mapper.Map<List<UserDto>>(users);

            return Ok(new { users = userDto });
        }

        // GET : api/Users/1/Order
        [HttpGet("{userId}/Order")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetUserOrder(int userId, [FromQuery(Name = "PageSize")] int pageSize = 5, [FromQuery(Name = "Page")] int page = 1)
        {
            var users = await _context.Users
                                 .Include(u => u.Orders)
                                     .ThenInclude(o => o.OrderDetails)
                                        .ThenInclude(od => od.Product)
                                 .Include(u => u.Orders)
                                     .ThenInclude(o => o.Payment)
                                 .Include(u => u.Orders)
                                     .ThenInclude(o => o.Shipment)
                                 .Where(u => u.UserId == userId).ToListAsync();

            var userDto = _mapper.Map<List<UserDto>>(users);
            var orders = userDto[0].Orders;

            var pageCount = Math.Ceiling(orders.Count() / (float)pageSize);
            var pagedOrders = orders.Skip((page - 1) * pageSize).Take(pageSize);

            return Ok(new { resultCount = pagedOrders.Count(), orders = pagedOrders, pageSize = pageSize, page = page, pageCount = pageCount });
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

        private async Task<TokenRequestDto> GenerateJwtToken(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection(key: "JwtConfig:Secret").Value);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("Email", user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, value:user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                }),
                Expires = DateTime.Now.Add(TimeSpan.Parse(_configuration.GetSection(key: "JwtConfig:ExpiryTimeFrame").Value)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var userToken = new UserToken()
            {
                JwtId = token.Id,
                Token = RandomStringGeneration(24),
                AddedDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddMonths(1),
                IsRevoked = false,
                IsUsed = false,
                UserId = user.UserId
            };

            _context.UserTokens.Add(userToken);
            await _context.SaveChangesAsync();

            var tokenRequest = new TokenRequestDto
            {
                Token = jwtToken,
                RefreshToken = userToken.Token
            };

            return tokenRequest;
        }

        private async Task<TokenRequestDto> VerifyAndGenerateToken(TokenRequestDto tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            _tokenValidationParameters.ValidateLifetime = false;

            var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                if (result == false)
                {
                    return null;
                }
            }

            var utcExpiryDate = Int64.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

            if (expiryDate > DateTime.Now)
            {
                return null;
            }

            var storedToken = await _context.UserTokens.FirstOrDefaultAsync(ut => ut.Token == tokenRequest.RefreshToken);

            if (storedToken == null)
            {
                return null;
            }

            if (storedToken.IsUsed)
            {
                return null;
            }

            if (storedToken.IsRevoked)
            {
                return null;
            }

            var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            if (storedToken.JwtId != jti)
            {
                return null;
            }

            if (storedToken.ExpiryDate < DateTime.Now)
            {
                return null;
            }

            storedToken.IsUsed = true;

            _context.UserTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == storedToken.UserId);

            return await GenerateJwtToken(user);
        }

        private string RandomStringGeneration(int length)
        {
            var random = new Random();
            var chars = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm_";

            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private DateTime UnixTimeStampToDateTime(Int64 unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();

            return dateTimeVal;
        }
    }
}
