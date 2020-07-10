using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Sandbox.Shared
{
    public static class GraphClientFactory
    {
        private const string SandboxClientId = "0c204bb6-d207-45e6-9252-56ff4909f79e";
        private const string SandboxResourceId = "https://graph.microsoft.com";

        private static readonly string[] SandboxScopes =
        {
            SandboxResourceId + "/.default"
        };

        private static readonly IPublicClientApplication App = PublicClientApplicationBuilder.Create(SandboxClientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdMultipleOrgs)
            .Build();

        public static async Task<GraphServiceClient> Create(string username, string password)
        {
            var authResult = await GetToken(username, password).ConfigureAwait(false);

            var authProvider = new DelegateAuthenticationProvider(request =>
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                return Task.CompletedTask;
            });

            return new GraphServiceClient(authProvider);
        }

        private static async Task<AuthenticationResult> GetToken(string username, string password)
        {
            try
            {
                return await App.AcquireTokenByUsernamePassword(SandboxScopes, username, password.ToSecureString())
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            catch (MsalUiRequiredException ex)
            {
                var consentWebUrl = $"https://login.microsoftonline.com/common/oauth2/authorize?resource={SandboxResourceId}&client_id={SandboxClientId}&response_type=code&prompt=consent";
                throw new Exception($"Please consent the sandbox azure application on your tenant at {consentWebUrl}", ex);
            }
        }
    }
}
