using MarketUpApi.Rest;
using System.Net;

namespace MarketUpApi.Handlers
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {

                if (context.Response.HasStarted)
                {
                    throw;
                }

                var message = exception.InnerException?.Message ?? exception.Message;
                var error = new ApiResponse
                {
                    Error = message,
                    Success = false
                };

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = ApiResponseType.JsonResponse;

                await context.Response.WriteAsync(error.ToString());
            }
        }
    }
}
