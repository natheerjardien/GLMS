using System;
using PROG7311_GLMS_ST10435542.Models;

namespace PROG7311_GLMS_ST10435542.Services
{
    public class OnHoldState : IContractState
    {
        public void Activate(Contract contract) // if its on hold we are allowed to activate it again
        {
            contract.Status = "Active";
        }

        public void Expire(Contract contract) // because it is possible to expire a paused contract
        {
            contract.Status = "Expired";
        }

        public void PutOnHold(Contract contract)
        {
            throw new InvalidOperationException("This contract is already On Hold.");
        }
    }
}