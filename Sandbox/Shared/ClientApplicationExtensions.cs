using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Sandbox.Shared
{
    public static class ClientApplicationExtensions
    {
        private static readonly StorageCreationProperties CacheStorageCreationProperties = new StorageCreationPropertiesBuilder(OAuth2CacheSettings.CacheFileName, OAuth2CacheSettings.CacheDir)
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

        public static async Task<T> SetupSecuredCache<T>(this T app, Func<T, ITokenCache> cacheAccessor) where T : IClientApplicationBase
        {
            var cache = cacheAccessor(app);
            var cacheHelper = await MsalCacheHelper.CreateAsync(CacheStorageCreationProperties).ConfigureAwait(false);
            cacheHelper.RegisterCache(cache);
            return app;
        }
    }
}