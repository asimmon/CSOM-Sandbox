using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Sandbox.Shared
{
    public static class OAuth2Helper
    {
        // You can replace it with your own Azure App Registration Client ID as long as it targets multiple organizations.
        private const string SandboxClientId = "0c204bb6-d207-45e6-9252-56ff4909f79e";

        private static readonly Lazy<Task<IPublicClientApplication>> AsyncApp = new Lazy<Task<IPublicClientApplication>>(CreateMsalAppWithCaching);

        private static async Task<IPublicClientApplication> CreateMsalAppWithCaching()
        {
            var app = PublicClientApplicationBuilder.Create(SandboxClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdMultipleOrgs)
                .Build();

            var storageProperties = new StorageCreationPropertiesBuilder(OAuth2CacheSettings.CacheFileName, OAuth2CacheSettings.CacheDir)
                .WithLinuxKeyring(
                    OAuth2CacheSettings.LinuxKeyRingSchema,
                    OAuth2CacheSettings.LinuxKeyRingCollection,
                    OAuth2CacheSettings.LinuxKeyRingLabel,
                    OAuth2CacheSettings.LinuxKeyRingAttr1,
                    OAuth2CacheSettings.LinuxKeyRingAttr2)
                .WithMacKeyChain(
                    OAuth2CacheSettings.KeyChainServiceName,
                    OAuth2CacheSettings.KeyChainAccountName)
                .Build();

            var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);
            cacheHelper.RegisterCache(app.UserTokenCache);

            return app;
        }

        public static Task<AuthenticationResult> AuthenticateAsync(string username, string password, string resourceId)
        {
            return AuthenticateAsync(new OAuth2Credentials(username, password, resourceId));
        }

        public static async Task<AuthenticationResult> AuthenticateAsync(OAuth2Credentials credentials)
        {
            var resourceWithDefaultScope = new[] { $"{credentials.ResourceId}/.default" };

            var app = await AsyncApp.Value.ConfigureAwait(false);
            var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
            var account = accounts.FirstOrDefault(x => credentials.Username.Equals(x.Username, StringComparison.OrdinalIgnoreCase));

            if (account != null)
            {
                if (credentials.IsCachingEnabled)
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
                return await app.AcquireTokenByUsernamePassword(resourceWithDefaultScope, credentials.Username, credentials.SecuredPassword)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            catch (MsalUiRequiredException ex)
            {
                var consentWebUrl = $"https://login.microsoftonline.com/common/oauth2/authorize?resource={credentials.ResourceId}&client_id={SandboxClientId}&response_type=code&prompt=consent";
                throw new Exception($"Please consent the sandbox azure application on your tenant at {consentWebUrl}", ex);
            }
        }
    }
}