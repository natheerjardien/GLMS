namespace PROG7311_GLMS_API.Services
{
// According to GeeksForGeeks (2026), the strategy design pattern is a behavioral design pattern that enables selecting an algorithms behavior at runtime.
// It defines a group of similar algorithms, encapsulates each one, and makes them interchangeable.
    public interface ICurrencyConversionStrategy // this interface creates a rule for any class that handles money conversion
    {
        // this method takes an amount of money and returns the converted value
        // we use 'task' and 'async' because the app now talks to a live api over the internet
        Task<decimal> ConvertAsync (decimal amount); // updated because added API integration
    }
}

/* Reference List:
 
    GeeksForGeeks. (2026). Gang of Four (GOF) Design Patterns. [online]
    Available at: <https://www.geeksforgeeks.org/system-design/gang-of-four-gof-design-patterns/>
    [Accessed 16 April 2026].

 */