using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace TECAir.API.Middlewares
{
    /// <summary>
    /// Intercepts unhandled exceptions and returns consistent problem responses.
    /// </summary>
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (status, message) = ex switch
            {
                // Postgres unique constraint violation (23505)
                PostgresException { SqlState: "23505" } pg => (
                    StatusCodes.Status409Conflict,
                    ResolveUniqueViolation(pg.ConstraintName)
                ),

                // Postgres foreign key violation (23503)
                PostgresException { SqlState: "23503" } => (
                    StatusCodes.Status409Conflict,
                    "Operation violates a foreign key constraint."
                ),

                // Generic not found
                KeyNotFoundException => (
                    StatusCodes.Status404NotFound,
                    ex.Message
                ),

                // Validation / business rule failures
                InvalidOperationException => (
                    StatusCodes.Status400BadRequest,
                    ex.Message
                ),

                // Fallback
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred."
                )
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";

            var problem = new ProblemDetails
            {
                Status = status,
                Title = message
            };

            await context.Response.WriteAsJsonAsync(problem);
        }

        /// <summary>Maps constraint names to human-readable messages.</summary>
        private static string ResolveUniqueViolation(string? constraintName) =>
            constraintName switch
            {
                "users_email_key" => "Email already registered.",
                "users_phone_number_key" => "Phone number already registered.",
                "users_college_id_number_key" => "College ID already registered.",
                _ => "A unique field is already in use."
            };
    }
}
