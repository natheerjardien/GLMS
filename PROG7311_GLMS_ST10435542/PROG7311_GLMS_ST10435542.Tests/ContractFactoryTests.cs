using PROG7311_GLMS_ST10435542.Services;

namespace PROG7311_GLMS_ST10435542.Tests
{
// According to Microsoft (n.d.), xUnit is a popular testing framework for .NET applications.
// It provides a simple way to write unit tests which are essential for testing the accuracy and reliability of code.
// In this class I wrote tests for the ContractFactory, which is responsible for creating new contract instances based on users input
    public class ContractFactoryTests // this class contains tests to make sure the contract factory works as it should
    {
        [Fact] // this label tells the computer that this is a specific test to run
        public void CreateContract_ValidInputs_SetsDefaultStatusToDraft() // tests if creating a contract with valid inputs sets the default status to Draft
        {
            var factory = new ContractFactory(); // first, we set up the factory and some sample dates
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddMonths(6);

            var contract = factory.CreateContract(1, "Premium", startDate, endDate); // then we try to create a contract

            Assert.Equal("Draft", contract.Status); // finally we check if the status was automatically set to 'draft'
        }

        [Fact]
        public void CreateContract_MapsInputsCorrectly() // tests if creating a contract maps the input to the contract properties correctly
        {
            var factory = new ContractFactory();
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 12, 31);

            var contract = factory.CreateContract(99, "Standard", startDate, endDate); // we create a contract with an id of 99 and the standard level

            // checks everu piece of data to make sure the factory didnt mix up the numbers or text
            Assert.Equal(99, contract.ClientId);
            Assert.Equal("Standard", contract.ServiceLevel);
            Assert.Equal(startDate, contract.StartDate);
            Assert.Equal(endDate, contract.EndDate);
        }

        [Fact]
        public void CreateContract_GeneratesEmptyAgreementPath() // tests if creating a contract generates an empty agreement file path
        {
            var factory = new ContractFactory();
            var contract = factory.CreateContract(1, "Basic", DateTime.Now, DateTime.Now.AddDays(1));

            // check that the file path is empty because no one has uploaded a file yet
            // this makes sure the factory isnt putting null data in that spot
            Assert.Null(contract.SignedAgreementFilePath);
        }
    }
}

/* Reference List:
 
    Microsoft. (n.d.). Unit testing C# in .NET using dotnet test and xUnit. [online]
    Available at: <https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit>
    [Accessed 22 April 2026].

 */