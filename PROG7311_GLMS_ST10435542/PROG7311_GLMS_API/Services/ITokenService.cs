using Microsoft.AspNetCore.Identity;

namespace PROG7311_GLMS_API.Services
{
    public interface ITokenService
    {
        // Builds a signed JWT containing the user's id, email and role claims
        (string Token, DateTime Expires) CreateToken(IdentityUser user, IList<string> roles);
    }
}
