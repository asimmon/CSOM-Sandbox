using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Sandbox.Shared
{
    public static class GraphClientFactory
    {
        public static Task<GraphServiceClient> CreateAsync(string username, string password)
        {
            return CreateAsync(new OAuth2Credentials(username, password, KnownResourceIds.Graph));
        }

        public static async Task<GraphServiceClient> CreateAsync(OAuth2Credentials credentials)
        {
            var graphEnforcedCredentials = new OAuth2Credentials(credentials.Username, credentials.Password, KnownResourceIds.Graph)
            {
                IsCachingEnabled = credentials.IsCachingEnabled
            };

            var tokenResult = await OAuth2Helper.AuthenticateAsync(graphEnforcedCredentials).ConfigureAwait(false);

            var authProvider = new DelegateAuthenticationProvider(request =>
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);
                return Task.CompletedTask;
            });

            return new GraphServiceClient(authProvider);
        }
    }
}