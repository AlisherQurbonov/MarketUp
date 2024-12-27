using MarketUp.Helpers;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using System.Text;

namespace MarketUp.Rest
{
    /// <summary>
    /// TimeSpans are not serialized consistently depending on what properties are present. So this 
    /// serializer will ensure the format is maintained no matter what.
    /// </summary>
    public class TimespanConverter : JsonConverter<TimeSpan>
    {
        /// <summary>
        /// Format: Days.Hours:Minutes:Seconds:Milliseconds
        /// </summary>
        public const string TimeSpanFormatString = @"d\.hh\:mm\:ss\:FFF";

        public override void WriteJson(JsonWriter writer, TimeSpan value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var timespanFormatted = $"{value.ToString(TimeSpanFormatString)}";
            writer.WriteValue(timespanFormatted);
        }

        public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue,
            bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            TimeSpan parsedTimeSpan;
            TimeSpan.TryParseExact((string)reader.Value, TimeSpanFormatString, null, out parsedTimeSpan);
            return parsedTimeSpan;
        }
    }

    public class DateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ"));
        }
    }

    public abstract class ApiClient
    {
        private readonly IWebProxy _proxy;
        private readonly HttpMessageHandler _handler;

        private const int LIMIT_TIMEOUT_IN_SECONDS = 30;

        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            MaxDepth = int.MaxValue,
            Converters =
            {
                new DateTimeConverter()
            }
        };

        private HttpClient CreateHttpClient(string authToken = "")
        {
            var handler = (_proxy != null)
                ? new HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = _proxy
                }
                : (_handler ?? NetworkHelper.ConfigureClientHandler());
            var httpTimeout = TimeSpan.FromSeconds(LIMIT_TIMEOUT_IN_SECONDS);
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = httpTimeout,
            };

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ApiResponseType.JsonResponse));

            if (!string.IsNullOrWhiteSpace(authToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            }

            return client;
        }

        private static async Task<T> ConvertResponse<T>(HttpResponseMessage request)
        {
            if (!request.IsSuccessStatusCode)
                throw new Exception(request.ReasonPhrase);

            var response = await request.Content.ReadFromJsonAsync<ApiResponse<T>>(_options);

            if (!response.Success)
                throw new Exception(response.Error);

            return response.Data;
        }

        private static async Task<string> ConvertResponseAsString(HttpResponseMessage request)
        {
            if (!request.IsSuccessStatusCode)
                throw new Exception(request.ReasonPhrase);

            var response = await request.Content.ReadAsStringAsync();

            return response;
        }

        private static async Task<object> ConvertResponse(HttpResponseMessage request)
        {
            if (!request.IsSuccessStatusCode)
                throw new Exception(request.ReasonPhrase);

            var response = await request.Content.ReadFromJsonAsync<ApiResponse>(_options);

            if (!response.Success)
                throw new Exception(response.Error);

            return response.Data;
        }

        private static async Task<ApiResponse> ConvertNoResponse(HttpResponseMessage request)
        {
            if (!request.IsSuccessStatusCode)
            {
                return new ApiResponse
                {
                    Error = request.ReasonPhrase,
                    Success = false
                };
            }

            return await request.Content.ReadFromJsonAsync<ApiResponse>();
        }

        protected ApiClient(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        protected ApiClient(string baseUrl, IWebProxy proxy) : this(baseUrl)
        {
            _proxy = proxy;
        }


        protected ApiClient(string baseUrl, HttpMessageHandler handler) : this(baseUrl)
        {
            _handler = handler;
        }

        protected string BaseUrl { get; }

        protected virtual async Task<T> Post<T>(string routeUrl, object bodyData, string authToken = "")
        {
            var client = CreateHttpClient(authToken);

            var content = new StringContent(JsonConvert.SerializeObject(bodyData), Encoding.UTF8,
                ApiResponseType.JsonResponse);
            var requestTimeout = TimeSpan.FromSeconds(LIMIT_TIMEOUT_IN_SECONDS);
            var tokenSource = new CancellationTokenSource(requestTimeout);
            var request = await client.PostAsync(routeUrl, content, tokenSource.Token);

            return await ConvertResponse<T>(request);
        }

        protected virtual async Task<object> Post(string routeUrl, object bodyData, string authToken = "")
        {
            var client = CreateHttpClient(authToken);
            var content = new StringContent(JsonConvert.SerializeObject(bodyData), Encoding.UTF8,
                ApiResponseType.JsonResponse);
            var requestTimeout = TimeSpan.FromSeconds(LIMIT_TIMEOUT_IN_SECONDS);
            var tokenSource = new CancellationTokenSource(requestTimeout);
            var request = await client.PostAsync(routeUrl, content, tokenSource.Token);

            return await ConvertResponse(request);
        }

        protected virtual async Task<string> PostAsString(string routeUrl, object bodyData, string authToken = "")
        {
            var client = CreateHttpClient(authToken);
            var content = new StringContent(JsonConvert.SerializeObject(bodyData), Encoding.UTF8,
                ApiResponseType.JsonResponse);
            var requestTimeout = TimeSpan.FromSeconds(LIMIT_TIMEOUT_IN_SECONDS);
            var tokenSource = new CancellationTokenSource(requestTimeout);
            var request = await client.PostAsync(routeUrl, content, tokenSource.Token);

            return await ConvertResponseAsString(request);
        }

        protected virtual async Task<T> Get<T>(string routeUrl, string authToken = "")
        {
            var client = CreateHttpClient(authToken);
            var requestTimeout = TimeSpan.FromSeconds(LIMIT_TIMEOUT_IN_SECONDS);
            var tokenSource = new CancellationTokenSource(requestTimeout);
            var request = await client.GetAsync(routeUrl, tokenSource.Token);

            return await ConvertResponse<T>(request);
        }

        protected virtual async Task<object> Get(string routeUrl, string authToken = "")
        {
            var client = CreateHttpClient(authToken);
            var requestTimeout = TimeSpan.FromSeconds(LIMIT_TIMEOUT_IN_SECONDS);
            var tokenSource = new CancellationTokenSource(requestTimeout);
            var request = await client.GetAsync(routeUrl, tokenSource.Token);

            return await ConvertResponse(request);
        }

        protected virtual async Task<string> GetAsString(string routeUrl, string authToken = "")
        {
            var client = CreateHttpClient(authToken);
            var requestTimeout = TimeSpan.FromSeconds(LIMIT_TIMEOUT_IN_SECONDS);
            var tokenSource = new CancellationTokenSource(requestTimeout);
            var request = await client.GetAsync(routeUrl, tokenSource.Token);

            return await ConvertResponseAsString(request);
        }

        protected virtual async Task<ApiResponse> GetNoResult(string routeUrl, string authToken = "")
        {
            var client = CreateHttpClient(authToken);
            var requestTimeout = TimeSpan.FromSeconds(LIMIT_TIMEOUT_IN_SECONDS);
            var tokenSource = new CancellationTokenSource(requestTimeout);
            var request = await client.GetAsync(routeUrl, tokenSource.Token);

            return await ConvertNoResponse(request);
        }

        protected virtual async Task<ApiResponse> PostNoResult(string routeUrl, object bodyData, string authToken = "")
        {
            var client = CreateHttpClient(authToken);
            var content = new StringContent(JsonConvert.SerializeObject(bodyData), Encoding.UTF8,
                ApiResponseType.JsonResponse);
            var requestTimeout = TimeSpan.FromSeconds(LIMIT_TIMEOUT_IN_SECONDS);
            var tokenSource = new CancellationTokenSource(requestTimeout);
            var request = await client.PostAsync(routeUrl, content, tokenSource.Token);

            return await ConvertNoResponse(request);
        }

        protected virtual async Task<ApiResponse> GetNoResultWithTimeout(string routeUrl, int timeoutInMinutes, string authToken = "")
        {
            var client = CreateHttpClient(authToken);
            var requestTimeout = TimeSpan.FromMinutes(timeoutInMinutes);
            client.Timeout = requestTimeout;
            var tokenSource = new CancellationTokenSource(requestTimeout);
            var request = await client.GetAsync(routeUrl, tokenSource.Token);

            return await ConvertNoResponse(request);
        }

        protected virtual async Task<T> Get<T>(string routeUrl, int timeOut, string authToken = "")
        {
            var client = CreateHttpClient(authToken);
            var requestTimeout = TimeSpan.FromMinutes(timeOut);
            var tokenSource = new CancellationTokenSource(requestTimeout);
            var request = await client.GetAsync(routeUrl, tokenSource.Token);

            return await ConvertResponse<T>(request);
        }
    }
}
