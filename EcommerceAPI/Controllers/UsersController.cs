using AutoMapper;
using EcommerceAPI.Configuration;
using EcommerceAPI.Data;
using EcommerceAPI.Dto;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Repository;
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
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public UsersController(IUserRepository userRepository, IMapper mapper, IConfiguration configuration, TokenValidationParameters tokenValidationParameters)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _configuration = configuration;
            _tokenValidationParameters = tokenValidationParameters;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
        {
            var users = await _userRepository.GetUsers();

            var usersDetailDto = _mapper.Map<List<UserDetailDto>>(users);

            return Ok(new BaseResponse { Result = usersDetailDto, ResultCount = usersDetailDto.Count });
        }

        // GET: api/Users/Email/user@example.com
        [HttpGet("Email/{email}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetUserByEmail(email);

            if (user == null)
            {
                return NotFound(new BaseResponse { Message = "User not found." });
            }

            var userDetailDto = _mapper.Map<UserDetailDto>(user);

            return Ok(new { resultCount = 1, users = userDetailDto }); ;
        }

        // POST: api/Users/Register
        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register([FromBody] UserRegisterDto userRegisterRequest)
        {
            if (_userRepository.EmailUsed(userRegisterRequest.Email))
            {
                return BadRequest(new BaseResponse { Message = "This email is already in use." });
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

            _userRepository.CreateUser(user);
            var response = await _userRepository.Save();

            if (response != true)
            {
                return BadRequest(new BaseResponse { Message = "Something went wrong." });
            }

            var tokenString = await GenerateJwtToken(user);

            return Ok(new BaseResponse { Message = "Your account has been successfully created.", Result = tokenString });
        }

        // POST: api/Users/Login
        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] UserLoginDto userLoginRequest)
        {
            var user = await _userRepository.GetUserByEmail(userLoginRequest.Email);

            if (user == null)
            {
                return NotFound(new BaseResponse { Message = "User not found." });
            }

            if (!VerifyPasswordHash(userLoginRequest.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest(new BaseResponse { Message = "Password is incorrect." });
            }

            var tokenString = await GenerateJwtToken(user);

            return Ok(new BaseResponse { Message = "Logging in your account.", Result = tokenString });
        }

        // POST: api/Users/RefreshToken
        [HttpPost("RefreshToken")]
        public async Task<ActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequest)
        {
            var result = await VerifyAndGenerateToken(tokenRequest);

            if (result == null)
            {
                return BadRequest(new BaseResponse { Message = "Invalid tokens." });
            }


            return Ok(new BaseResponse { Message = "Token Refreshed.", Result = result });
        }

        // GET : api/Users/Cart
        [HttpGet("Cart")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetUserCart()
        {
            var userId = int.Parse(User.Claims.Where(x => x.Type == "UserId").FirstOrDefault()?.Value);

            var user = await _userRepository.GetUserCart(userId);

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(new BaseResponse { Result = userDto });
        }

        // GET : api/Users/Order
        [HttpGet("Order")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetUserOrder([FromQuery(Name = "PageSize")] int pageSize = 5, [FromQuery(Name = "Page")] int page = 1)
        {
            var userId = int.Parse(User.Claims.Where(x => x.Type == "UserId").FirstOrDefault()?.Value);
            var user = await _userRepository.GetUserOrder(userId);
            
            var userDto = _mapper.Map<UserDto>(user);
            var orders = userDto.Orders;

            var pageCount = Math.Ceiling(orders.Count() / (float)pageSize);
            var pagedOrders = orders.Skip((page - 1) * pageSize).Take(pageSize);

            return Ok(new BaseResponse { ResultCount = pagedOrders.Count(), Result = pagedOrders, PageSize = pageSize, Page = page, PageCount = (int)pageCount });
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

            _userRepository.CreateToken(userToken);
            await _userRepository.Save();

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

            var storedToken = await _userRepository.GetUserToken(tokenRequest.RefreshToken);

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

            _userRepository.UpdateToken(storedToken);
            await _userRepository.Save();

            var user = await _userRepository.GetUser(storedToken.UserId);

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
