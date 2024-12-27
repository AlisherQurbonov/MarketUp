using MarketUpApi.Handlers;

namespace MarketUpApi.Extensions
{
    public static class MiddlewareExtension
    {
        public static IApplicationBuilder UseAppException(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
