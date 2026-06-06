using System;
using PROG7311_GLMS_ST10435542.Models;

namespace PROG7311_GLMS_ST10435542.Services
{
    public class ExpiredState : IContractState
    {
        public void Activate (Contract contract)
        {
            throw new InvalidOperationException("You cannot activate a contract that has already Expired.");
        }

        public void Expire (Contract contract)
        {
            throw new InvalidOperationException("This contract is already Expired.");
        }

        public void PutOnHold(Contract contract)
        {
            throw new InvalidOperationException("This contract is already On Hold.");
        }
    }
}