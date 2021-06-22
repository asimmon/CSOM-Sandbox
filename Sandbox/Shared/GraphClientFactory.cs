using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Sandbox.Shared
{
    public static class GraphClientFactory
    {
        public static Task<GraphServiceClient> CreateAsUserAsync(string username, string password)
        {
            return CreateAsUserAsync(new OAuth2UserAuthenticationOptions(username, password, KnownResourceIds.Graph));
        }

        public static async Task<GraphServiceClient> CreateAsUserAsync(OAuth2UserAuthenticationOptions options)
        {
            var graphEnforcedOptions = new OAuth2UserAuthenticationOptions(options.Username, options.Password, KnownResourceIds.Graph)
            {
                IsCachingEnabled = options.IsCachingEnabled
            };

            var tokenResult = await OAuth2Helper.AuthenticateAsUserAsync(graphEnforcedOptions).ConfigureAwait(false);

            var authProvider = new DelegateAuthenticationProvider(request =>
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);
                return Task.CompletedTask;
            });

            return new GraphServiceClient(authProvider);
        }

        public static async Task<GraphServiceClient> CreateAsAppAsync(string tenantName)
        {
            var graphEnforcedOptions = new OAuth2AppAuthenticationOptions(tenantName, KnownResourceIds.Graph);

            var tokenResult = await OAuth2Helper.AuthenticateAsAppAsync(graphEnforcedOptions).ConfigureAwait(false);

            var authProvider = new DelegateAuthenticationProvider(request =>
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);
                return Task.CompletedTask;
            });

            return new GraphServiceClient(authProvider);
        }
    }
}