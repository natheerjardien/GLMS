using System.Threading.Tasks;

namespace PROG7311_GLMS_API.Services
{
    public class PricingService : IPricingService
    {
        private readonly ICurrencyConversionStrategy _currencyStrategy;

        public PricingService(ICurrencyConversionStrategy currencyStrategy)
        {
            _currencyStrategy = currencyStrategy;
        }

        public async Task<decimal> GetTotalCostInUsdAsync(string packageSizeCategory, string slaType)
        {
            decimal usdPrice = packageSizeCategory switch // this switch determines the base price in USD based on the package size category
            {
                "1-5kg" => 5.00m,
                "5-10kg" => 10.00m,
                "10+kg" => 20.00m,
                _ => 0.00m
            };

            // auto calculates express delivery price (1.5x for Express)
            if (slaType == "Express")
            {
                usdPrice *= 1.5m;
            }

            return await _currencyStrategy.ConvertAsync(usdPrice);
        }
    }
}