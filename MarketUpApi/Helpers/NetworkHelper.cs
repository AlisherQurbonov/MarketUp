using System.Net.NetworkInformation;
using System.Net;

namespace MarketUpApi.Helpers
{
    public class NetworkHelper
    {
        private const string LocaleDomain = "rtsb.uz";

        public static void ConfigureProxy()
        {
            ServicePointManager.ServerCertificateValidationCallback += (a, b, c, d) => true;
        }

        public static bool IsLocale()
        {

            var lan = IPGlobalProperties.GetIPGlobalProperties();
            var domain = lan.DomainName;

            return domain == LocaleDomain;
        }

        public static HttpClientHandler ConfigureClientHandler()
        {

            ConfigureProxy();

            if (IsLocale())
            {
                return new HttpClientHandler
                {
                    UseProxy = true,
                    Proxy = GetDefaultProxy(),
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };
            }

            return new HttpClientHandler();
        }

        public static IWebProxy GetDefaultProxy() => WebRequest.GetSystemWebProxy();
    }
}
