using System.ComponentModel.DataAnnotations;

namespace PROG7311_GLMS_ST10435542.Models.Auth
{
    // What the user types into the login form
    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    // What the API sends back after a successful POST /api/auth/login
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    // Payload for POST /api/auth/register-client (creates the login AND the client profile)
    public class RegisterClientRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ContactDetails { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}
