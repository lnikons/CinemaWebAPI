using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CinemaWebAPI.Exceptions
{
    public class ApiExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            (int status, string title) = exception switch
            {
                BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request"),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                BusinessRuleException => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (0, "")
            };

            if (status == 0)
                return false;

            ProblemDetails problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json", cancellationToken);
            return true;
        }
    }
}
