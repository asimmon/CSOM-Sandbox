using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Sandbox.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Sandbox
{
    public class PowerAutomateTests
    {
        private static readonly Lazy<Task<HttpClient>> LazyHttpClientAsync = new Lazy<Task<HttpClient>>(async () =>
        {
            const string username = "username@mytenantname.onmicrosoft.com";
            const string password = "password";

            var credentials = new OAuth2Credentials(username, password, KnownResourceIds.AzureManagement);
            var authenticationResult = await OAuth2Helper.AuthenticateAsync(credentials).ConfigureAwait(false);

            var httpClient = new HttpClient();

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
            var jsonResponse = await httpClient.GetStringAsync("https://management.azure.com/providers/Microsoft.ProcessSimple/environments?api-version=2016-11-01");
            this._output.WriteLine(jsonResponse);
        }
    }
}