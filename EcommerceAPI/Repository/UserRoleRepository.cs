using EcommerceAPI.Data;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Repository
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly EcommerceContext _context;

        public UserRoleRepository(EcommerceContext context)
        {
            _context = context;
        }

        public bool IsUserRoleExists(int roleId)
        {
            return _context.UserRoles.Any(ur => ur.RoleId == roleId);
        }
    }
}
