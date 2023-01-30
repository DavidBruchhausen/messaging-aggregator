using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using MessagingAggregator.Api.Common.Responses;
using MessagingAggregator.Api.Common.Responses.Metadata;
using Serilog;

namespace MessagingAggregator.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private const string ContentType = "application/json";

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        // catch (NotFoundException)
        // {
        //     await HandleNotFoundExceptionAsync(httpContext);
        // }
        // catch (ValidationException ex)
        // {
        //     await HandleValidationExceptionAsync(httpContext, ex);
        // }
        catch (UnauthorizedAccessException)
        {
            await HandleUnauthorizedAccessExceptionAsync(httpContext);
        }
        catch (Exception ex)
        {
            httpContext.Items.Add("Exception", ex);
            await HandleExceptionAsync(httpContext);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context)
    {
        const HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        const string message = "There was a problem while processing your request.";

        var meta = new Meta(statusCode, message);
        var metaResponse = new MetaResponse<Meta>(meta);

        return WriteResponse(context, statusCode, metaResponse);
    }

    private static Task WriteResponse<TMeta>(HttpContext context, HttpStatusCode statusCode, MetaResponse<TMeta> response)
        where TMeta : Meta
    {
        ConfigureContext(context, statusCode);
        var responsePayload = JsonSerializer.Serialize<object>(response, SerializerOptions);
        return context.Response.WriteAsync(responsePayload);
    }

    private static void ConfigureContext(HttpContext context, HttpStatusCode statusCode)
    {
        context.Response.ContentType = ContentType;
        context.Response.StatusCode = (int)statusCode;
    }

    private static Task HandleNotFoundExceptionAsync(HttpContext context)
    {
        const HttpStatusCode statusCode = HttpStatusCode.NotFound;
        const string message = "The requested resource could not be found.";

        var meta = new Meta(statusCode, message);
        var metaResponse = new MetaResponse<Meta>(meta);

        return WriteResponse(context, statusCode, metaResponse);
    }

    private static Task HandleUnauthorizedAccessExceptionAsync(HttpContext context)
    {
        const HttpStatusCode statusCode = HttpStatusCode.Unauthorized;
        const string message = "Unauthorized.";

        var meta = new Meta(statusCode, message);
        var metaResponse = new MetaResponse<Meta>(meta);

        return WriteResponse(context, statusCode, metaResponse);
    }
}
