using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Exceptions
{
    public class AppExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not AppException appException)
            return false;

        context.Response.StatusCode = appException.StatusCode;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = appException.StatusCode,
            Title = appException.Title,
            Detail = appException.Message
        }, ct);

        return true;
    }
}
}