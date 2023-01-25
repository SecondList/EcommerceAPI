using AutoMapper;
using EcommerceAPI.Dto;
using EcommerceAPI.Models;

namespace EcommerceAPI.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<User, UserDto>();
            CreateMap<User, UserDetailDto>();
            CreateMap<User, UserLoginDto>();
            CreateMap<User, UserRegisterDto>();
            CreateMap<UserDto, User>();
            CreateMap<UserDetailDto, User>();
            CreateMap<UserLoginDto, User>();
            CreateMap<UserRegisterDto, User>();
            CreateMap<Product, ProductDto>();
            CreateMap<Product, ProductDetailDto>();
            CreateMap<Product, ProductCreateDto>();
            CreateMap<ProductDto, Product>();
            CreateMap<ProductDetailDto, Product>();
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductCategory, ProductCategoryDto>();
            CreateMap<ProductCategory, ProductCategoryCreateDto>();
            CreateMap<ProductCategory, ProductCategoryDetailDto>();
            CreateMap<ProductCategoryDto, ProductCategory>();
            CreateMap<ProductCategoryCreateDto, ProductCategory>();
            CreateMap<ProductCategoryDetailDto, ProductCategory>();
            CreateMap<Cart, CartDto>();
            CreateMap<Cart, CartDeleteDto>();
            CreateMap<Cart, CartDetailDto>();
            CreateMap<CartDto, Cart>();
            CreateMap<CartDeleteDto, Cart>();
            CreateMap<CartDetailDto, Cart>();
            CreateMap<Order, OrderDto>();
            CreateMap<Order, Payment>().ForMember(p => p.Amount, opt => opt.MapFrom(o => o.TotalPrice));
            CreateMap<OrderDto, Order>();
            CreateMap<OrderDetail, OrderDetailDto>();
            CreateMap<OrderDetailDto, OrderDetail>();
            CreateMap<Payment, PaymentDto>();
            CreateMap<Payment, Order>();
            CreateMap<PaymentDto, Payment>();
            CreateMap<Shipment, ShipmentDto>();
            CreateMap<ShipmentDto, Shipment>();
            CreateMap<CheckOutDto, Order>();
            CreateMap<CheckOutDto, OrderDetail>();
            CreateMap<CheckOutDto, Payment>();
            CreateMap<CheckOutDto, Shipment>();
            CreateMap<CheckOutDto, StripePaymentDto>().ForMember(sp => sp.RecipientName, opt => opt.MapFrom(co => $"{co.FirstName} {co.LastName}"));
            CreateMap<Cart, OrderDetail>().ForMember(od => od.Price, opt => opt.MapFrom(c => c.Product.Price));
            CreateMap<UserToken, TokenRequestDto>();
            CreateMap<TokenRequestDto, UserToken>();
        }
    }
}
