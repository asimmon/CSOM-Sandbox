using System.Security;

namespace Sandbox.Shared
{
    public static class StringExtensions
    {
        public static SecureString ToSecureString(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var result = new SecureString();
            foreach (var c in text)
                result.AppendChar(c);

            return result;
        }
    }
}