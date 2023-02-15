using EcommerceAPI.ActionFilters;
using EcommerceAPI.Configuration;
using EcommerceAPI.Data;
using EcommerceAPI.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.DependencyInjection;
using EcommerceAPI.Models;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Repository;
using EcommerceAPI.Services.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelFilterAttribute>(); // Register Action Filter Attribute.
}).ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddDbContext<EcommerceContext>(options =>
{
    // options.UseLazyLoadingProxies();
    options.UseSqlServer(builder.Configuration.GetConnectionString("someeConnection"));
});

// JWT Bearer
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(key: "JwtConfig"));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();

var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection(key: "JwtConfig:Secret").Value);
var issuer = builder.Configuration.GetSection(key: "JwtConfig:Issuer").Value;
var audience = builder.Configuration.GetSection(key: "JwtConfig:Audience").Value;
var subject = builder.Configuration.GetSection(key: "JwtConfig:Subject").Value;

var tokenValidationParameters = new TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = false, // for development
    ValidateAudience = false, // for development
    ValidAudience = audience,
    ValidIssuer = issuer,
    RequireExpirationTime = false,
    ValidateLifetime = true
};

builder.Services.AddAuthentication(configureOptions: options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwt =>
{
    jwt.SaveToken = true;
    jwt.TokenValidationParameters = tokenValidationParameters;
});

builder.Services.AddSingleton(tokenValidationParameters);

// This is for action and controller level.
// builder.Services.AddScoped<ValidateModelFilterAttribute>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddPolicy(
    name: "EcommerceOrigins",
    policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader();
    }));

// Add Stripe Infrastructure
builder.Services.AddStripeInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalErrorHandlingMiddleware();

app.UseCors("EcommerceOrigins");

app.UseHttpsRedirection();

app.UseStatusCodePages(async statusCodeContext =>
{
    // using static System.Net.Mime.MediaTypeNames;
    statusCodeContext.HttpContext.Response.ContentType = Application.Json;

    var exceptionResult = JsonSerializer.Serialize(new
    {
        message = ($"Status Code Page: {statusCodeContext.HttpContext.Response.StatusCode}"),
        error = statusCodeContext.HttpContext.Response.StatusCode
    });

    await statusCodeContext.HttpContext.Response.WriteAsync(exceptionResult);
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
