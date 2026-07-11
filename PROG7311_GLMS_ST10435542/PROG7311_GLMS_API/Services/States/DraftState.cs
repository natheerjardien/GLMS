using System;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Services
{
    public class DraftState : IContractState
    {
        public void Activate (Contract contract)
        {
            contract.Status = "Active";
        }

        public void Expire (Contract contract)
        {
            throw new InvalidOperationException("You cannot expire a contract that is still in Draft mode. Please activate the contract first before it can be expired.");
        }

        public void PutOnHold(Contract contract)
        {
            throw new InvalidOperationException("This contract is already On Hold.");
        }
    }
}