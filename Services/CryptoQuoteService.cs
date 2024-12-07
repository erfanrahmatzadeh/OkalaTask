using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OkalaTask.Interface;

namespace OkalaTask.Services
{
    public class CryptoQuoteService : ICryptoQuoteService
    {
        private readonly HttpClient _httpClient;
        private readonly string _coinMarketCapUrl;
        private readonly string _coinMarketCapApiKey;
        private readonly string _exchangeRatesUrl;
        private readonly string _exchangeRatesApiKey;

        public CryptoQuoteService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _coinMarketCapUrl = configuration["ApiSettings:CoinMarketCapUrl"];
            _coinMarketCapApiKey = configuration["ApiSettings:CoinMarketCapApiKey"];
            _exchangeRatesUrl = configuration["ApiSettings:ExchangeRatesUrl"];
            _exchangeRatesApiKey = configuration["ApiSettings:ExchangeRatesApiKey"];
        }

        public virtual async Task<Dictionary<string, decimal>> GetCryptoQuoteAsync(string cryptoCode)
        {
            // Step 1: Get Crypto Price in USD
            var cryptoPriceInUsd = await GetCryptoPriceInUsd(cryptoCode);
            if (cryptoPriceInUsd == null) return null;

            // Step 2: Get Exchange Rates
            var exchangeRates = await GetExchangeRates();
            if (exchangeRates == null) return null;

            // Step 3: Convert Prices
            var targetCurrencies = new[] { "USD", "EUR", "BRL", "GBP", "AUD" };
            var convertedRates = targetCurrencies.ToDictionary(
                currency => currency,
                currency => currency == "USD"
                        ? Math.Round(cryptoPriceInUsd.Value, 4)
                        : Math.Round(cryptoPriceInUsd.Value * exchangeRates[currency], 4)
            );

            return convertedRates;
        }

        private async Task<decimal?> GetCryptoPriceInUsd(string cryptoCode)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_coinMarketCapUrl}?symbol={cryptoCode}");
            request.Headers.Add("X-CMC_PRO_API_KEY", _coinMarketCapApiKey);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(json);
            return (decimal?)data?.data[cryptoCode]?.quote?.USD?.price;
        }

        private async Task<Dictionary<string, decimal>> GetExchangeRates()
        {
            var requestUrl = $"{_exchangeRatesUrl}?access_key={_exchangeRatesApiKey}";
            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(json);

            var ratesObject = data.rates as JObject;
            if (ratesObject == null)
            {
                return null;
            }

            // Convert rates to a dictionary
            var rates = ratesObject.Properties()
                .ToDictionary(
                    prop => prop.Name,
                    prop => prop.Value.ToObject<decimal>()
                );

            if (!rates.TryGetValue("USD", out var usdRate) || usdRate == 0)
            {
                throw new Exception("USD rate not available or is zero.");
            }

            return rates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / usdRate);
        }
    }
}
