using Newtonsoft.Json;

namespace MarketUp.Rest
{
    public class ApiResponse<T>
    {
        public ApiResponse()
        {
            Success = true;
        }

        public ApiResponse(string error)
        {
            Error = error;
            Success = false;
        }

        public ApiResponse(T data)
        {
            Data = data;
            Success = true;
        }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
    }
}
