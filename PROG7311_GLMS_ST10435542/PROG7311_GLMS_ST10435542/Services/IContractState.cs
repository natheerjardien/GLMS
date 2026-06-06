using PROG7311_GLMS_ST10435542.Models;

namespace PROG7311_GLMS_ST10435542.Services
{
// According to GeeksForGeeks (2026), the State design pattern is a behavioral design pattern that allows an object to change its behavior when its internal state changes.
    public interface IContractState // this interface acts as a master list of actions that can happen to a contract
    {
        void Activate (Contract contract); // this method will be called when we want to turn a contract 'active'
        void Expire (Contract contract);
        void PutOnHold (Contract contract); // New method for putting a contract on hold
    }
}

/* Reference List:
 
    GeeksForGeeks. (2026). Gang of Four (GOF) Design Patterns. [online]
    Available at: <https://www.geeksforgeeks.org/system-design/gang-of-four-gof-design-patterns/>
    [Accessed 16 April 2026].

 */