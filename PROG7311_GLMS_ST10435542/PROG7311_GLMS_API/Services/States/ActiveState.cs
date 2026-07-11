using System;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Services
{
    public class ActiveState : IContractState
    {
        public void Activate(Contract contract) 
        { 
            throw new InvalidOperationException("This contract is already Active."); 
        }

        public void Expire(Contract contract) 
        { 
            contract.Status = "Active";
            contract.Status = "Expired"; 
        }

        public void PutOnHold(Contract contract)
        {
            contract.Status = "On Hold";
        }
    }
}