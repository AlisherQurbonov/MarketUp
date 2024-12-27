using MarketUp.Dtos;
using MarketUp.Helpers;
using MarketUp.Rest;
using Newtonsoft.Json;

namespace MarketUp.Services
{
    public class HandbookService : ApiClient
    {
        private readonly AppConfig _config;

        public HandbookService(AppConfig config) : base(config.HandbookApi)
        {
            _config = config;
        }

        public async Task<IEnumerable<CultureDto>> GetCultures()
        {
            try
            {               
                var result = await Get<IEnumerable<CultureDto>>($"/Culture/GetAll");

                return result;
                       
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return new CultureDto[0];                
            }
        }
    }
}
