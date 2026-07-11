using System.Net.Http.Headers;

namespace PROG7311_GLMS_ST10435542.Services.ApiClients
{
    // When the user logged in, the JWT from the API was stored as a claim inside the auth cookie.
    // This handler quietly attaches that JWT as a Bearer header to every outgoing API call,
    // so none of the typed API clients need to know anything about authentication.
    public class BearerTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BearerTokenHandler(IHttpContextAccessor httpContextAccessor) =>
            _httpContextAccessor = httpContextAccessor;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("access_token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
