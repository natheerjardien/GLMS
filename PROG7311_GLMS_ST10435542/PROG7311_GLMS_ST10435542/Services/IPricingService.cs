using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PROG7311_GLMS_ST10435542.Services
{
    public interface IPricingService
    {
       // calculates the final ZAR cost based on the package size category
        Task<decimal> GetTotalCostInUsdAsync(string packageSizeCategory, string slaType);
    }
}