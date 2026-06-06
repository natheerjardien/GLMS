using System.Threading.Tasks;

namespace PROG7311_GLMS_ST10435542.Services
{
    public class UsdToZarStrategy : ICurrencyConversionStrategy // this class is a simple version of the money converter that doesnt need the internet
    {
        private readonly decimal _exchangeRate = 18.50m; // hardcoded the approximate exchnage rate for USD to ZAR

        public Task<decimal> ConvertAsync (decimal amount) // this method does the math to change usd into rand
        {
            return Task.FromResult(amount * _exchangeRate); // task.fromresult is used because the system expects an async result even though this is instant
        }
    }
}

/* Reference List:
 
    Slupik, A.. (2022). Async gotcha: returning Task.FromResult or Task.CompletedTask. [online]
    Available at: <https://dev.to/asik/dont-return-taskfromresult-or-taskcompletedtask-4gcp>
    [Accessed 20 April 2026].

 */