using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Sandbox.Shared
{
    public static class OAuth2Helper
    {
        // You can replace it with your own Azure App Registration Client ID as long as it targets multiple organizations.
        private const string SandboxClientId = "0c204bb6-d207-45e6-9252-56ff4909f79e";

        // In order to use app authentication (not delegated), you need to provide your own app client ID above and paste your client secret below.
        private const string SandboxClientSecret = "<yourAppClientSecret>";

        private static readonly Lazy<Task<IPublicClientApplication>> AsyncPublicApp = new Lazy<Task<IPublicClientApplication>>(CreatePublicAppAsync);
        private static readonly ConcurrentDictionary<string, Task<IConfidentialClientApplication>> AsyncConfidentialApps = new (StringComparer.OrdinalIgnoreCase);

        private static async Task<IPublicClientApplication> CreatePublicAppAsync()
        {
            return await PublicClientApplicationBuilder.Create(SandboxClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdMultipleOrgs)
                .Build()
                .SetupSecuredCache(x => x.UserTokenCache)
                .ConfigureAwait(false);
        }

        private static async Task<IConfidentialClientApplication> CreateConfidentialAppAsync(string tenantName)
        {
            static Task<IConfidentialClientApplication> AsyncConfidentialAppFactory(string tn)
            {
                var configuredApp = ConfidentialClientApplicationBuilder.Create(SandboxClientId)
                    .WithClientSecret(SandboxClientSecret)
                    .WithTenantId(tn + ".onmicrosoft.com")
                    .Build();

                return Task.FromResult(configuredApp);
            }

            return await AsyncConfidentialApps.GetOrAdd(tenantName, AsyncConfidentialAppFactory).ConfigureAwait(false);
        }

        public static async Task<AuthenticationResult> AuthenticateAsAppAsync(OAuth2AppAuthenticationOptions options)
        {
            var resourceWithDefaultScope = new[] { $"{options.ResourceId}/.default" };

            var app = await CreateConfidentialAppAsync(options.TenantName).ConfigureAwait(false);

            try
            {
                return await app.AcquireTokenForClient(resourceWithDefaultScope).ExecuteAsync().ConfigureAwait(false);
            }
            catch (MsalUiRequiredException ex)
            {
                var consentWebUrl = $"https://login.microsoftonline.com/common/oauth2/authorize?resource={options.ResourceId}&client_id={SandboxClientId}&response_type=code&prompt=consent";
                throw new Exception($"Please consent the sandbox azure application on your tenant at {consentWebUrl}", ex);
            }
        }

        public static Task<AuthenticationResult> AuthenticateAsUserAsync(string username, string password, string resourceId)
        {
            return AuthenticateAsUserAsync(new OAuth2UserAuthenticationOptions(username, password, resourceId));
        }

        public static async Task<AuthenticationResult> AuthenticateAsUserAsync(OAuth2UserAuthenticationOptions options)
        {
            var resourceWithDefaultScope = new[] { $"{options.ResourceId}/.default" };

            var app = await AsyncPublicApp.Value.ConfigureAwait(false);
            var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
            var account = accounts.FirstOrDefault(x => options.Username.Equals(x.Username, StringComparison.OrdinalIgnoreCase));

            if (account != null)
            {
                if (options.IsCachingEnabled)
                {
                    try
                    {
                        return await app.AcquireTokenSilent(resourceWithDefaultScope, account).ExecuteAsync().ConfigureAwait(false);
                    }
                    catch (MsalException)
                    {
                        // We'll try to acquire a token with the username and password instead
                    }
                }
                else
                {
                    await app.RemoveAsync(account).ConfigureAwait(false);
                }
            }

            try
            {
                return await app.AcquireTokenByUsernamePassword(resourceWithDefaultScope, options.Username, options.SecuredPassword)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            catch (MsalUiRequiredException ex)
            {
                var consentWebUrl = $"https://login.microsoftonline.com/common/oauth2/authorize?resource={options.ResourceId}&client_id={SandboxClientId}&response_type=code&prompt=consent";
                throw new Exception($"Please consent the sandbox azure application on your tenant at {consentWebUrl}", ex);
            }
        }
    }
}