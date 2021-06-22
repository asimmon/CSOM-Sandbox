using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace Sandbox.Shared
{
    public static class ClientContextFactory
    {
        private static readonly ISet<HttpStatusCode> ThrottlingHttpStatusCodes = new HashSet<HttpStatusCode>
        {
            (HttpStatusCode) 429,
            (HttpStatusCode) 503
        };

        public static Task<ClientContext> CreateAsync(string webUrl)
        {
            return CreateAsync(webUrl, string.Empty, string.Empty);
        }

        public static async Task<ClientContext> CreateAsync(string webUrl, string username, string password)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            if (string.IsNullOrWhiteSpace(webUrl))
                throw new ArgumentNullException(nameof(webUrl));

            var context = new ClientContext(webUrl);

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                username = username.Trim();
                password = password.Trim();

                if (username.EndsWith(".onmicrosoft.com", StringComparison.OrdinalIgnoreCase))
                {
                    var resourceId = GetAssociatedResourceId(new Uri(webUrl, UriKind.Absolute));
                    var credentials = new OAuth2UserAuthenticationOptions(username, password, resourceId);
                    var tokenResult = await OAuth2Helper.AuthenticateAsUserAsync(credentials).ConfigureAwait(false);

                    context.ExecutingWebRequest += (sender, e) =>
                    {
                        e.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + tokenResult.AccessToken;
                    };
                }
                else
                {
                    context.Credentials = new NetworkCredential(username, password.ToSecureString());
                }
            }
            else
            {
                context.Credentials = CredentialCache.DefaultCredentials;
            }

            return context;
        }

        private static string GetAssociatedResourceId(Uri uri)
        {
            return uri.GetLeftPart(UriPartial.Authority);
        }

        public static async Task ExecuteQueryRetryAsync(this ClientRuntimeContext clientContext, int maxRetryCount = 10, int initialDelay = 500)
        {
            if (maxRetryCount <= 0)
                throw new ArgumentException("Provide a maximum retry count greater than zero.");

            if (initialDelay <= 0)
                throw new ArgumentException("Provide an initial delay greater than zero.");

            var retryAttempts = 0;

            ClientRequest request = null;

            while (retryAttempts < maxRetryCount)
            {
                try
                {
                    if (request == null)
                    {
                        request = clientContext.PendingRequest;
                        await clientContext.ExecuteQueryAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        await clientContext.RetryQueryAsync(request).ConfigureAwait(false);
                    }

                    return;
                }
                catch (WebException wex)
                {
                    if (wex.Response is HttpWebResponse response && ThrottlingHttpStatusCodes.Contains(response.StatusCode))
                    {
                        await Task.Delay(initialDelay).ConfigureAwait(false);

                        retryAttempts++;
                        initialDelay *= 2;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            throw new Exception($"Maximum retry attempts {maxRetryCount}, has be attempted.");
        }
    }
}