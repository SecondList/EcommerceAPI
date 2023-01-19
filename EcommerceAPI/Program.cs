using EcommerceAPI.ActionFilters;
using EcommerceAPI.Data;
using EcommerceAPI.Middlewares;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;

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

builder.Services.AddDbContext<EcommerceContext>(options =>
{
    // options.UseLazyLoadingProxies();
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// This is for action and controller level.
// builder.Services.AddScoped<ValidateModelFilterAttribute>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
