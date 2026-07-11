using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Services
{
    public interface IContractFactory // this interface acts as a set of rules that any contract factory must follow
    {
        Contract CreateContract(int clientId, string serviceLevel, DateTime startDate, DateTime endDate); // defines the specific way to create a contract
    }
}