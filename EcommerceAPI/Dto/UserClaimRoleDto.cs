using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Dto
{
    public class UserClaimRoleDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "User Id must be positive value.")]
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Role Id must be positive value.")]
        public int RoleId { get; set; }
    }
}
