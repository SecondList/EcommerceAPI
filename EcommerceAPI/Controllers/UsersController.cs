using AutoMapper;
using EcommerceAPI.Configuration;
using EcommerceAPI.Data;
using EcommerceAPI.Dto;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using EcommerceAPI.Repository;
using EcommerceAPI.Services.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Drawing.Printing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static NuGet.Packaging.PackagingConstants;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly IUserService _userService;

        public UsersController(IUserRepository userRepository, IUserRoleRepository userRoleRepository, ICartRepository cartRepository, IOrderRepository orderRepository, IMapper mapper, IConfiguration configuration, TokenValidationParameters tokenValidationParameters, IUserService userService)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _configuration = configuration;
            _tokenValidationParameters = tokenValidationParameters;
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
        {
            var users = await _userRepository.GetUsers();

            var usersDetailDto = _mapper.Map<List<UserDetailDto>>(users);

            return Ok(new BaseResponse { Result = usersDetailDto, ResultCount = usersDetailDto.Count });
        }

        // GET: api/Users/Email/user@example.com
        [HttpGet("Email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetUserByEmail(email);

            if (user == null)
            {
                return NotFound(new BaseResponse { Message = "User not found.", Errors = new[] { $"{email} not found." } });
            }

            var userDetailDto = _mapper.Map<UserDetailDto>(user);

            return Ok(new BaseResponse { ResultCount = 1, Result = userDetailDto }); ;
        }

        // POST: api/Users/Register
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Register([FromBody] UserRegisterDto userRegisterRequest)
        {
            if (_userRepository.EmailUsed(userRegisterRequest.Email))
            {
                return BadRequest(new BaseResponse { Message = "This email is already in use.", Errors = new[] { $"Duplicate email {userRegisterRequest.Email}" } });
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
                return BadRequest(new BaseResponse { Message = "Something went wrong.", Errors = new[] { "Fail to save into database." } });
            }

            return Ok(new BaseResponse { Message = "Your account has been successfully created."});
        }

        // api/Users/ClaimRole
        [HttpPut("ClaimRole")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ClaimRole(UserClaimRoleDto userClaimRole)
        {
            if (!_userRepository.IsUserExists(userClaimRole.UserId))
            {
                return NotFound(new BaseResponse { Message = "User not found." });
            }

            if (!_userRoleRepository.IsUserRoleExists(userClaimRole.RoleId))
            {
                return NotFound(new BaseResponse { Message = "User role not found." });
            }

            await _userRepository.UpdateUserRole(userClaimRole.UserId, userClaimRole.RoleId);
            await _userRepository.Save();

            return Ok(new BaseResponse { Message = "User role updated." });
        }

        // POST: api/Users/Login
        [HttpPost("Login")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
        [Authorize(Roles = "Buyer")]
        public async Task<ActionResult> GetUserCart([FromQuery(Name = "PageSize")] int pageSize = 5, [FromQuery(Name = "Page")] int page = 1)
        {
            var pageCount = Math.Ceiling(_cartRepository.CountCarts(_userService.GetUserId()) / (float)pageSize);

            var user = await _userRepository.GetUserCart(_userService.GetUserId(), page, pageSize);

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(new BaseResponse { Result = userDto.Carts, ResultCount = userDto.Carts.Count, PageSize = pageSize, Page = page, PageCount = (int)pageCount });
        }

        // GET : api/Users/Order
        [HttpGet("Order")]
        [Authorize(Roles = "Buyer")]
        public async Task<ActionResult> GetUserOrder([FromQuery(Name = "PageSize")] int pageSize = 5, [FromQuery(Name = "Page")] int page = 1)
        {
            var pageCount = Math.Ceiling(_orderRepository.CountOrders(_userService.GetUserId()) / (float)pageSize);

            var user = await _userRepository.GetUserOrder(_userService.GetUserId(), page, pageSize);

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(new BaseResponse { Result = userDto.Orders, ResultCount = userDto.Orders.Count, PageSize = pageSize, Page = page, PageCount = (int)pageCount });
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
                    new Claim(ClaimTypes.Role, user.UserRole.RoleName),
                    new Claim(JwtRegisteredClaimNames.Email, value:user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                }),
                Expires = DateTime.Now.ToUniversalTime().Add(TimeSpan.Parse(_configuration.GetSection(key: "JwtConfig:ExpiryTimeFrame").Value)),
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
