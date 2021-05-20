using System.Collections.Generic;
using System.IO;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Sandbox.Shared
{
    public static class OAuth2CacheSettings
    {
        private static readonly string CacheFilePath = Path.Combine(MsalCacheHelper.UserRootDirectory, "sandbox.msal.cache");
        public static readonly string CacheFileName = Path.GetFileName(CacheFilePath);
        public static readonly string CacheDir = Path.GetDirectoryName(CacheFilePath);

        public const string KeyChainServiceName = "Sandbox";
        public const string KeyChainAccountName = "MsalCache";

        public const string LinuxKeyRingSchema = "com.asimmon.sandbox.tokencache";
        public const string LinuxKeyRingCollection = MsalCacheHelper.LinuxKeyRingDefaultCollection;
        public const string LinuxKeyRingLabel = "MSAL token cache for the M365 sandbox tests.";

        public static readonly KeyValuePair<string, string> LinuxKeyRingAttr1 = new KeyValuePair<string, string>("Version", "1");
        public static readonly KeyValuePair<string, string> LinuxKeyRingAttr2 = new KeyValuePair<string, string>("ProductGroup", "MyApps");
    }
}