using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IO;
using System.Security.Claims;

namespace MarketUpApi.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly IWebHostEnvironment _environment;    

        public RequestLoggingMiddleware(RequestDelegate next, IWebHostEnvironment environment)
        {
            _next = next;
            _manager = new RecyclableMemoryStreamManager();
            _environment = environment;            
        }

        public async Task Invoke(HttpContext context)
        {
            await Task.Factory.StartNew(async () =>
            {
                await LoggingRequest(context);
            });

            await _next(context);
        }

        private async Task LoggingRequest(HttpContext context)
        {
            var request = context.Request;         
            {

                var ip = $"{context.Connection.RemoteIpAddress}";
                var agent = request.Headers["User-Agent"];
                var user = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var url = request.GetDisplayUrl();

                request.EnableBuffering();

                await using var requestStream = _manager.GetStream();
                await request.Body.CopyToAsync(requestStream);
                var body = ReadStreamChunks(requestStream);

                request.Body.Position = 0;

                //await _notification.PortalInfo(ip, agent, url, user, $"/{request.Method}", body);
            }
        }

        private static string ReadStreamChunks(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);
            var readChunkBufferLength = 4096;
            var readChunk = new char[readChunkBufferLength];
            var readChunkLength = 0;

            do
            {
                readChunkLength = reader.ReadBlock(readChunk, 0, readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);

            return textWriter.ToString();
        }
    }
}
