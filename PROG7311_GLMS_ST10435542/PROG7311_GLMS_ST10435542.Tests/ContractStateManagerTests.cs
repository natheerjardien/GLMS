using PROG7311_GLMS_API.Services;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_ST10435542.Tests
{
// 
    public class ContractStateManagerTests // this class is used to run automated tests on the contract state logic
    {
        private readonly ContractStateManager _manager = new ContractStateManager();

        [Fact]
        public void ActivateContract_FromDraft_ChangesToActive() // tests if activating a contract from the Draft state changes it to Active
        {
            var contract = new Contract { Status = "Draft" }; // start with a contract that is just a draft

            _manager.ActivateContract(contract); // run the activate command
            
            Assert.Equal("Active", contract.Status); // check if the status is now officially 'active'
        }

        [Fact]
        public void ActivateContract_FromOnHold_ChangesToActive() // tests if activating a contract from the On Hold state changes it to Active
        {
            var contract = new Contract { Status = "On Hold" }; // starts with a contract that was paused on hold

            _manager.ActivateContract(contract);

            Assert.Equal("Active", contract.Status); // it should move back to active status
        }

        [Fact]
        public void ActivateContract_AlreadyActive_ThrowsException() // tests if activating a contract that is already Active throws an exception
        {
            var contract = new Contract { Status = "Active" };

            // this test passes if the system throws an error
            // you cannot activate something that is already active
            Assert.Throws<InvalidOperationException>(() => _manager.ActivateContract(contract));
        }

        [Fact]
        public void ActivateContract_Expired_ThrowsException() // tests if activating a contract that is Expired throws an exception
        {
            var contract = new Contract { Status = "Expired" };

            Assert.Throws<InvalidOperationException>(() => _manager.ActivateContract(contract)); // this checks that the system blocks us from activating an expired contract
        }

        [Fact]
        public void PutOnHold_FromActive_ChangesToOnHold() // tests if putting a contract on hold from the Active state changes it to On Hold
        {
            var contract = new Contract { Status = "Active" };

            _manager.PutOnHoldContract(contract); // tries to put it on hold

            Assert.Equal("On Hold", contract.Status); // it should update to 'on hold'
        }

        [Fact]
        public void PutOnHold_FromDraft_ThrowsException() // tests if putting a contract on hold from the Draft state throws an exception
        {
            var contract = new Contract { Status = "Draft" };

            Assert.Throws<InvalidOperationException>(() => _manager.PutOnHoldContract(contract)); // the system should throw an error because you can only hold an active contract
        }

        [Fact]
        public void ExpireContract_FromActive_ChangesToExpired() // tests if expiring a contract from the Active state changes it to Expired
        {
            var contract = new Contract { Status = "Active" };

            _manager.ExpireContract(contract);

            Assert.Equal("Expired", contract.Status); // it should successfully change to 'expired'
        }

        [Fact]
        public void ExpireContract_FromOnHold_ChangesToExpired() // tests if expiring a contract from the On Hold state changes it to Expired
        {
            var contract = new Contract { Status = "On Hold" };

            _manager.ExpireContract(contract);

            Assert.Equal("Expired", contract.Status); // it should change to 'expired'
        }

        [Fact]
        public void ExpireContract_AlreadyExpired_ThrowsException() // tests if expiring a contract that is already Expired throws an exception
        {
            var contract = new Contract { Status = "Expired" };

            Assert.Throws<InvalidOperationException>(() => _manager.ExpireContract(contract)); // this checks that we cannot expire the same contract twice
        }
    }
}

/* Reference List:
 
    Microsoft. (n.d.). Unit testing C# in .NET using dotnet test and xUnit. [online]
    Available at: <https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit>
    [Accessed 22 April 2026].

 */