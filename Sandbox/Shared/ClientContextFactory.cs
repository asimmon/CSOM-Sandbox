using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
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

        public static ClientContext Create(string webUrl)
        {
            return Create(webUrl, string.Empty, string.Empty);
        }

        public static ClientContext Create(string webUrl, string username, string password)
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
                    context.Credentials = new SharePointOnlineCredentials(username, password.ToSecureString());
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

        public static void ExecuteQueryRetry(this ClientRuntimeContext clientContext, int maxRetryCount = 10, int initialDelay = 500)
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
                        clientContext.ExecuteQuery();
                    }
                    else
                    {
                        clientContext.RetryQuery(request);
                    }

                    return;
                }
                catch (WebException wex)
                {
                    if (wex.Response is HttpWebResponse response && ThrottlingHttpStatusCodes.Contains(response.StatusCode))
                    {
                        Thread.Sleep(initialDelay);

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