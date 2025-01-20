using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UTAPI.Utils;

namespace UTAPI.Middleware
{
    public class InternalServerErrorExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<InternalServerErrorExceptionHandler> _logger;

        public InternalServerErrorExceptionHandler(ILogger<InternalServerErrorExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not InternalServerErrorException internalServerErrorException)
            {
                return false;
            }

            _logger.LogError(
                internalServerErrorException,
                "Exception occurred: {Message}",
                internalServerErrorException.Message);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = internalServerErrorException.Message
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
