namespace EcommerceAPI.Services.User
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetUserId()
        {
            int result = -1;

            if (_httpContextAccessor.HttpContext != null)
            {
                result = int.Parse(_httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == "UserId").FirstOrDefault()?.Value);
            }

            return result;
        }
    }
}
