using System;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Services
{
// According to GeeksForGeeks (2026), the State design pattern is a behavioral design pattern that allows an object to change its behavior when its internal state changes.
// The object will appear to change its class.
    public class ContractStateManager : IContractStateManager // this class manages the different states a contract can be in
    {
        private IContractState GetCurrentState (string status) // this private method looks at the current status text and picks the right state class
        {
            return status switch // uses the contract's status to determine which state object to return
            {
                "Draft" => new DraftState(),
                "Active" => new ActiveState(),
                "Expired" => new ExpiredState(),
                "On Hold" => new OnHoldState(),
                _ => throw new ArgumentException("Unknown contract status")
            };
        }

        public void ActivateContract (Contract contract) // this method handles the request to turn a contract into 'active'
        {
            var state = GetCurrentState(contract.Status);
            state.Activate(contract); // calls the Activate method
        }

        public void ExpireContract (Contract contract) // this method handles the request to turn a contract into 'expired'
        {
            var state = GetCurrentState(contract.Status);
            state.Expire(contract); // calls the Expire method
        }

        public void PutOnHoldContract (Contract contract) // this method handles the request to put a contract 'on hold'
        {
            var state = GetCurrentState(contract.Status);
            state.PutOnHold(contract); // calls the PutOnHold method
        }
    }
}

/* Reference List:
 
    GeeksForGeeks. (2026). Gang of Four (GOF) Design Patterns. [online]
    Available at: <https://www.geeksforgeeks.org/system-design/gang-of-four-gof-design-patterns/>
    [Accessed 16 April 2026].

 */