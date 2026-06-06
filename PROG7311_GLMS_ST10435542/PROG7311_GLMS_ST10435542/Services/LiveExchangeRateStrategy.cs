using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PROG7311_GLMS_ST10435542.Services
{
    public class LiveExchangeRateStrategy : ICurrencyConversionStrategy // this class uses a live internet connection to figure out money conversions
    {
        private readonly HttpClient _httpClient;
        private readonly string _appId = "f238e4ddd513450cb160cd3186ac3ccf"; // API key from openexchangerates.org (free tier)

        public LiveExchangeRateStrategy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> ConvertAsync(decimal amount) // this is the main task that finds the latest rate and does the math
        {
            try
            {
                string url = $"https://openexchangerates.org/api/latest.json?app_id={_appId}";
                
                var response = await _httpClient.GetAsync(url); // this line actually sends the request to the website
                response.EnsureSuccessStatusCode(); // this makes sure the website actually answered us correctly

                var responseBody = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(responseBody);
                
                decimal zarRate = (decimal)doc.RootElement.GetProperty("rates").GetProperty("ZAR").GetDouble(); // the API returns as a double so we convert to decimal over here

                return amount * zarRate; // multiplies the original amount by the live rate to get the final total
            }
            catch
            {
                return amount * 18.50m; // fallback on the hardcoded exchnage if the API fails
            }
        }
    }
}

/* Reference List:
 
    OpenExchangeRates. (2026). Open Exchange Rates. [online]
    Available at: <https://openexchangerates.org/>
    [Accessed 20 April 2026].

 */