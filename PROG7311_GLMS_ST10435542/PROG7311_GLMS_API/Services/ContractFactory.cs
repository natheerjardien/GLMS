using System.Net;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Services
{
// According to GeeksForGeeks (2026), the Factory design pattern is a creational design pattern that provides an interface for creating objects in a superclass, but allows subclasses to alter the type of objects that will be created.
// In this implementation, the ContractFactory class is responsible for creating new Contract objects with the necessary properties set, ensuring that all contracts are created in the same way.
    public class ContractFactory : IContractFactory // this class is a 'factory' that specializes in making new contract objects
    {
        // this method takes in the basic info and builds a full contract object
        public Contract CreateContract(int clientId, string serviceLevel, DateTime startDate, DateTime endDate)
        {
            return new Contract // returns a brand new contract filled with the info provided
            {
                ClientId = clientId,
                ServiceLevel = serviceLevel,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Draft", // default contract status when created
                SignedAgreementFilePath = null // null because theres no file attached yet
            };
        }
    }
}

/* Reference List:
 
    GeeksForGeeks. (2026). Gang of Four (GOF) Design Patterns. [online]
    Available at: <https://www.geeksforgeeks.org/system-design/gang-of-four-gof-design-patterns/>
    [Accessed 16 April 2026].

 */