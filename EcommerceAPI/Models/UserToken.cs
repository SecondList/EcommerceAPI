using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models
{
    public class UserToken
    {
        public int Id { get; set; }

        [ForeignKey("Users")]
        public int UserId { get; set; }

        public string Token { get; set; }

        public string JwtId { get; set; }

        public bool IsUsed { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime AddedDate { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}
