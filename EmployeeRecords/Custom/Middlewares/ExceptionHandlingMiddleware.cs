using System.Data.Common;
using System.Net;

namespace EmployeeRecords.Custom.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "Error occurred while interacting with the database");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing the request");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
}