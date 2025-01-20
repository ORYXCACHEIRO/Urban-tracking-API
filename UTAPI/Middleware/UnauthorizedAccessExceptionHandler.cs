using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace UTAPI.Middleware
{
    public class UnauthorizedAccessExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<UnauthorizedAccessExceptionHandler> _logger;

        public UnauthorizedAccessExceptionHandler(ILogger<UnauthorizedAccessExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not Utils.UnauthorizedAccessException unauthorizedAccessExceptionException)
            {
                return false;
            }

            _logger.LogError(
                unauthorizedAccessExceptionException,
                "Exception occurred: {Message}",
                unauthorizedAccessExceptionException.Message);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized Access",
                Detail = unauthorizedAccessExceptionException.Message
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
