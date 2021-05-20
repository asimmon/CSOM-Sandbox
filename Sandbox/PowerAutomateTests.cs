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
    public class PowerAutomateTests
    {
        private static readonly Uri PowerAutomateApiBaseUrl = new Uri("https://management.azure.com/providers/Microsoft.ProcessSimple/", UriKind.Absolute);

        private static readonly Lazy<Task<HttpClient>> LazyHttpClientAsync = new Lazy<Task<HttpClient>>(async () =>
        {
            const string username = "username@mytenantname.onmicrosoft.com";
            const string password = "password";

            var credentials = new OAuth2Credentials(username, password, KnownResourceIds.AzureManagement);
            var authenticationResult = await OAuth2Helper.AuthenticateAsync(credentials).ConfigureAwait(false);

            var httpClient = new HttpClient();

            httpClient.BaseAddress = PowerAutomateApiBaseUrl;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

            return httpClient;
        });

        private readonly ITestOutputHelper _output;

        public PowerAutomateTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public async Task DisplayEnvironments()
        {
            var httpClient = await LazyHttpClientAsync.Value;

            var environmentsJson = await httpClient.GetStringAsync("environments?api-version=2016-11-01");
            var environments = JsonSerializer.Deserialize<PagedResults<FlowEnvironment>>(environmentsJson);
            Assert.NotNull(environments);
            Assert.NotEmpty(environments.Values);
            this._output.WriteLine("Fetched {0} power automate environment(s)", environments.Values.Count);

            var environment = environments.Values[0];
            this._output.WriteLine("Retrieving flows for environment: {0}", environment.Name);

            // Fetch my personal flows
            var myFlows = await GetPagedFlows(httpClient, $"environments/{environment.Name}/flows?api-version=2016-11-01").ToListAsync();
            this._output.WriteLine("Fetched {0} personal flow(s) owned by the current user", myFlows.Count);

            // As an administrator, fetch all users personal flows and flows stored in solutions
            var allFlows = await GetPagedFlows(httpClient, $"scopes/admin/environments/{environment.Name}/flows?api-version=2016-11-01&$").ToListAsync();
            this._output.WriteLine("Fetched {0} personal and solution flow(s)", allFlows.Count);
        }

        private static async IAsyncEnumerable<Flow> GetPagedFlows(HttpClient httpClient, string apiUrlStr)
        {
            while (true)
            {
                var myFlowsJson = await httpClient.GetStringAsync(apiUrlStr).ConfigureAwait(false);
                var myFlowsPage = JsonSerializer.Deserialize<PagedResults<Flow>>(myFlowsJson);
                Assert.NotNull(myFlowsPage);

                foreach (var flow in myFlowsPage.Values)
                {
                    yield return flow;
                }

                if (myFlowsPage.NextLink == null)
                {
                    yield break;
                }

                var apiUrlBuilder = new UriBuilder(myFlowsPage.NextLink);

                apiUrlBuilder.Host = PowerAutomateApiBaseUrl.Host;
                apiUrlBuilder.Scheme = PowerAutomateApiBaseUrl.Scheme;
                apiUrlBuilder.Port = PowerAutomateApiBaseUrl.Port;

                apiUrlStr = apiUrlBuilder.ToString();
            }
        }

        private class PagedResults<T> where T : class
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