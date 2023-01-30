using EcommerceAPI.Exceptions;
using System.Net;
using System.Text.Json;

namespace EcommerceAPI.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode status;
            var stackTrace = String.Empty;
            string exMessage;
            var exceptionType = exception.GetType();

            if (exceptionType == typeof(BadRequestException))
            {
                exMessage = exception.Message;
                status = HttpStatusCode.BadRequest;
                stackTrace = exception.StackTrace;
            }
            else if (exceptionType == typeof(NotFoundException))
            {
                exMessage = exception.Message;
                status = HttpStatusCode.NotFound;
                stackTrace = exception.StackTrace;
            }
            else if (exceptionType == typeof(Exceptions.NotImplementedException))
            {
                status = HttpStatusCode.NotImplemented;
                exMessage = exception.Message;
                stackTrace = exception.StackTrace;
            }
            else if (exceptionType == typeof(Exceptions.UnauthorizedAccessException))
            {
                status = HttpStatusCode.Unauthorized;
                exMessage = exception.Message;
                stackTrace = exception.StackTrace;
            }
            else if (exceptionType == typeof(Exceptions.KeyNotFoundException))
            {
                status = HttpStatusCode.Unauthorized;
                exMessage = exception.Message;
                stackTrace = exception.StackTrace;
            }
            else
            {
                status = HttpStatusCode.InternalServerError;
                exMessage = exception.Message;
                stackTrace = exception.StackTrace;
            }

            var exceptionResult = JsonSerializer.Serialize(new
            {
                Message = ($"{exceptionType.Name} error from the custom middleware."),
                Errors = (exception.InnerException == null? exMessage : exception.InnerException.Message),
                StackTrace = stackTrace
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;
            return context.Response.WriteAsync(exceptionResult);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class GlobalErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalErrorHandlingMiddleware>();
        }
    }
}
