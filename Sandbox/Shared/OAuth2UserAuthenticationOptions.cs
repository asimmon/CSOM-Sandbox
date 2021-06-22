using System;
using System.Security;

namespace Sandbox.Shared
{
    public abstract class OAuth2AuthenticationOptions
    {
        protected OAuth2AuthenticationOptions(string resourceId)
        {
            this.ResourceId = resourceId ?? throw new ArgumentNullException(nameof(resourceId));
            this.IsCachingEnabled = true;
        }

        public string ResourceId { get; }
        public bool IsCachingEnabled { get; set; }
    }

    public class OAuth2UserAuthenticationOptions : OAuth2AuthenticationOptions
    {
        public OAuth2UserAuthenticationOptions(string username, string password, string resourceId) : base(resourceId)
        {
            this.Username = username ?? throw new ArgumentNullException(nameof(username));
            this.Password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public string Username { get; }
        public string Password { get; }

        public SecureString SecuredPassword
        {
            get => this.Password.ToSecureString();
        }
    }

    public class OAuth2AppAuthenticationOptions : OAuth2AuthenticationOptions
    {
        public OAuth2AppAuthenticationOptions(string tenantName, string resourceId) : base(resourceId)
        {
            this.TenantName = tenantName ?? throw new ArgumentNullException(nameof(tenantName));
        }

        public string TenantName { get; }
    }
}