using PROG7311_GLMS_ST10435542.Models;

namespace PROG7311_GLMS_ST10435542.Services
{
// According to GeeksForGeeks (2026), the structural design pattern, specifically the facade pattern, provides a simplified interface to a complex subsystem.
// In this case, the IContractStateManager serves as a facade that simplifies the interaction with the different contract states and their transitions.
    public interface IContractStateManager // this interface tells the rest of the app what the state manager is capable of doing
    {
        void ActivateContract (Contract contract); // this is the command used to try and move a contract to the active state
        void ExpireContract (Contract contract);
        void PutOnHoldContract (Contract contract);
    }
}

/* Reference List:
 
    GeeksForGeeks. (2026). Gang of Four (GOF) Design Patterns. [online]
    Available at: <https://www.geeksforgeeks.org/system-design/gang-of-four-gof-design-patterns/>
    [Accessed 16 April 2026].

 */