﻿using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Dto
{
    public class CartDeleteDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "User Id must be positive value.")]
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Product Id must be positive value.")]
        public int ProductId { get; set; }
    }
}
