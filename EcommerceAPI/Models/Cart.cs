﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set;}

        [ForeignKey("Product")]
        [Range(1, int.MaxValue, ErrorMessage = "Product Id must be positive value.")]
        public int ProductId { get; set; }

        [ForeignKey("User")]
        [Range(1, int.MaxValue, ErrorMessage = "User Id must be positive value.")]
        public int UserId { get; set; }
        
        [Range(1, 99999, ErrorMessage = "Quantity must be a positive value and cannot order more than 99999 items")]
        [DisplayName("Order Quantity")]
        public int OrderQty { get; set; }
        public DateTime CreatedAt { get; set;}
        public DateTime ModifiedAt { get; set; }
        public User User { get; set; } = null!;
        public Product Product { get; set; } = null!;

    }
}
