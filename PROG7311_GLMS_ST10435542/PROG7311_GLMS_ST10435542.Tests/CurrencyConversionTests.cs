using Moq;
using Moq.Protected;
using System.Net;
using PROG7311_GLMS_ST10435542.Services;

namespace PROG7311_GLMS_ST10435542.Tests
{
    public class CurrencyConversionTests // this class tests if the currency converter correctly handles api data and errors
    {
// Followed the implementation of Rebhi (2024) for mocking the HttpClient to test the LiveExchangeRateStrategy without making real API calls
        // this is a helper method that creates a fake internet browser for testing
        // it allows us to tell the app exactly what text and status code the "web" should return
        private HttpClient CreateMockHttpClient(string jsonResponse, HttpStatusCode statusCode)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode, Content = new StringContent(jsonResponse) });

            return new HttpClient(handlerMock.Object);
        }

        [Fact]
        public async Task ConvertAsync_ValidAmount_UsesApiRate() // tests if the conversion uses the API rate when the API call is successful
        {
            var httpClient = CreateMockHttpClient("{\"rates\": {\"ZAR\": 19.00}}", HttpStatusCode.OK); // created a fake api response that says 1 dollar = 19 rand
            var strategy = new LiveExchangeRateStrategy(httpClient);

            // test if 100 dollars becomes 1900 rand
            var result = await strategy.ConvertAsync(100m); // 100 dollars * 19.00 rand

            Assert.Equal(1900m, result); // checks if the math matches the fake api rate
        }

        [Fact]
        public async Task ConvertAsync_ZeroAmount_ReturnsZero() // tests if converting zero dollars returns zero rand
        {
            var httpClient = CreateMockHttpClient("{\"rates\": {\"ZAR\": 19.00}}", HttpStatusCode.OK);
            var strategy = new LiveExchangeRateStrategy(httpClient);

            var result = await strategy.ConvertAsync(0m); // checks if zero dollars correctly returns zero rand

            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task ConvertAsync_ApiFails_UsesHardcodedFallback() // tests if the conversion uses the hardcoded fallback rate when the API call fails
        {
            var httpClient = CreateMockHttpClient("", HttpStatusCode.InternalServerError); // simulates a 500 error from the API
            var strategy = new LiveExchangeRateStrategy(httpClient);

            var result = await strategy.ConvertAsync(100m); // should use the hardcoded 18.50 fallback
            
            Assert.Equal(1850m, result);
        }

        [Fact]
        public async Task ConvertAsync_NegativeAmount_UsesApiRate() // tests if converting a negative amount uses the API rate correctly (possibly refunds)
        {
            var httpClient = CreateMockHttpClient("{\"rates\": {\"ZAR\": 19.00}}", HttpStatusCode.OK);
            var strategy = new LiveExchangeRateStrategy(httpClient);

            var result = await strategy.ConvertAsync(-50m); // checks if negative numbers are still multiplied correctly by the rate

            Assert.Equal(-950m, result);
        }
    }
}

/* Reference List:
 
    Microsoft. (n.d.). Unit testing C# in .NET using dotnet test and xUnit. [online]
    Available at: <https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit>
    [Accessed 22 April 2026].

    Rebhi, Z.. (2024). The Ultimate Guide to Unit Testing in .NET (C#) Using xUnit and Moq (Without the Boring Examples). [online]
    Available at: <https://ziedrebhi.medium.com/the-ultimate-guide-to-unit-testing-in-c-using-xunit-and-moq-without-the-boring-examples-e53624e4db2b>
    [Accessed 22 April 2026].

 */