namespace EmployeeRecords.Custom.Middlewares;

public class NotFoundPageMiddleware
{
    private readonly RequestDelegate _next;

    public NotFoundPageMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        var responseStatusCode = context.Response.StatusCode;
        if (responseStatusCode != StatusCodes.Status404NotFound)
            return;

        if (context.Items.ContainsKey("statusSetManually"))
            return;

        context.Request.Path = "/notfound";
        await _next(context);
    }
}