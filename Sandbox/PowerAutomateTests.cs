using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Sandbox.Shared;
using Xunit;
using Xunit.Abstractions;
using System.Text.Json.Serialization;

namespace Sandbox
{
    public class PowerAutomateTests : BaseTest
    {
        private static readonly Uri PowerAutomateApiBaseUrl = new Uri("https://management.azure.com/providers/Microsoft.ProcessSimple/", UriKind.Absolute);

        private static readonly Lazy<Task<HttpClient>> LazyHttpClientAsync = new Lazy<Task<HttpClient>>(async () =>
        {
            const string username = "username@mytenantname.onmicrosoft.com";
            const string password = "password";

            var credentials = new OAuth2UserAuthenticationOptions(username, password, KnownResourceIds.AzureManagement);
            var authenticationResult = await OAuth2Helper.AuthenticateAsUserAsync(credentials).ConfigureAwait(false);

            var httpClient = new HttpClient();

            httpClient.BaseAddress = PowerAutomateApiBaseUrl;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

            return httpClient;
        });

        public PowerAutomateTests(ITestOutputHelper logger) : base(logger)
        {
        }

        [Fact]
        public async Task DisplayEnvironments()
        {
            var httpClient = await LazyHttpClientAsync.Value;

            // Fetch first power apps environment
            var environments = await FetchPagedItems<FlowEnvironment>(httpClient, "environments?api-version=2016-11-01").ToListAsync();
            this.Logger.WriteLine("Fetched power apps environment(s): {0}", string.Join(", ", environments.Select(x => x.Name)));
            Assert.NotEmpty(environments);
            var environment = environments[0];

            // Fetch my personal flows
            var myFlows = await FetchPagedItems<Flow>(httpClient, $"environments/{environment.Name}/flows?api-version=2016-11-01").ToListAsync();
            this.Logger.WriteLine("Fetched {0} personal flow(s) owned by the current user", myFlows.Count);

            // As an administrator, fetch all users personal flows and flows stored in solutions
            var allFlows = await FetchPagedItems<Flow>(httpClient, $"scopes/admin/environments/{environment.Name}/flows?api-version=2016-11-01&$").ToListAsync();
            this.Logger.WriteLine("Fetched {0} personal and solution flow(s)", allFlows.Count);
        }

        private static async IAsyncEnumerable<T> FetchPagedItems<T>(HttpClient httpClient, string apiUrlStr)
        {
            while (true)
            {
                var jsonItemPage = await httpClient.GetStringAsync(apiUrlStr).ConfigureAwait(false);
                var itemPage = JsonSerializer.Deserialize<PagedResults<T>>(jsonItemPage);
                Assert.NotNull(itemPage);

                foreach (var item in itemPage.Values)
                {
                    yield return item;
                }

                if (itemPage.NextLink == null)
                {
                    yield break;
                }

                var apiUrlBuilder = new UriBuilder(itemPage.NextLink);

                apiUrlBuilder.Host = PowerAutomateApiBaseUrl.Host;
                apiUrlBuilder.Scheme = PowerAutomateApiBaseUrl.Scheme;
                apiUrlBuilder.Port = PowerAutomateApiBaseUrl.Port;

                apiUrlStr = apiUrlBuilder.ToString();
            }
        }

        private class PagedResults<T>
        {
            [JsonPropertyName("value")]
            public List<T> Values { get; set; }

            [JsonPropertyName("nextLink")]
            public string NextLink { get; set; }
        }

        private class FlowEnvironment
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        private class Flow
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("properties")]
            public FlowProperties Properties { get; set; }
        }

        private class FlowProperties
        {
            [JsonPropertyName("displayName")]
            public string DisplayName { get; set; }
        }
    }
}