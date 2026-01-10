using System.Net;

namespace CreditFlowAPI.Base.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // Προσπάθησε να συνεχίσεις την κλήση κανονικά...
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // ...Αν σκάσει οτιδήποτε, έλα εδώ!
                _logger.LogError($"Something went wrong: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Εδώ αποφασίζουμε τι κωδικό θα δώσουμε ανάλογα με το Exception
            var response = context.Response;
            var message = exception.Message;

            switch (exception)
            {
                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound; // 404
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401
                    break;

                case ArgumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
                    message = "Internal Server Error from the custom middleware.";
                    break;
            }

            // Φτιάχνουμε το ωραίο JSON
            var errorDetails = new ErrorDetails()
            {
                StatusCode = response.StatusCode,
                Message = message // Το μήνυμα που γράψαμε στο throw new Exception("...")
            };

            await context.Response.WriteAsync(errorDetails.ToString());
        }
    }
}
