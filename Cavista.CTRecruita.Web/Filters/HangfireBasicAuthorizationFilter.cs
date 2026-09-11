using Hangfire.Dashboard;

namespace Cavista.CTRecruita.Web.Filters
{
    public class HangfireCustomBasicAuthenticationFilter : IDashboardAuthorizationFilter
    {
        public string User { get; set; }
        public string Pass { get; set; }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Check if basic authentication header is present
            if (httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = httpContext.Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                {
                    var credentials = authHeader.Substring("Basic ".Length).Trim();
                    var decodedCredentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(credentials));
                    var parts = decodedCredentials.Split(':', 2);

                    if (parts.Length == 2)
                    {
                        var username = parts[0];
                        var password = parts[1];

                        // Validate credentials against configured values
                        if (username == User && password == Pass)
                        {
                            return true;
                        }
                    }
                }
            }
            // If authentication fails, set a 401 Unauthorized response
            httpContext.Response.StatusCode = 401;
            httpContext.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
            return false;
        }
    }
}