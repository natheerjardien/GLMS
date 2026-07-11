using System;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Services
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