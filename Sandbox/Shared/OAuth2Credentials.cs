using System;
using System.Security;

namespace Sandbox.Shared
{
    public class OAuth2Credentials
    {
        public OAuth2Credentials(string username, string password, string resourceId)
        {
            this.Username = username ?? throw new ArgumentNullException(nameof(username));
            this.Password = password ?? throw new ArgumentNullException(nameof(password));
            this.ResourceId = resourceId ?? throw new ArgumentNullException(nameof(resourceId));
            this.IsCachingEnabled = true;
        }

        public string Username { get; }
        public string Password { get; }
        public string ResourceId { get; }
        public bool IsCachingEnabled { get; set; }

        public SecureString SecuredPassword
        {
            get => this.Password.ToSecureString();
        }
    }
}