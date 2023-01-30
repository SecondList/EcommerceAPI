using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class UserRole
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
