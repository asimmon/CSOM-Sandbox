using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Xunit;
using Xunit.Abstractions;

using GraphClientFactory = Sandbox.Shared.GraphClientFactory;

namespace Sandbox
{
    public class GraphTests
    {
        private readonly ITestOutputHelper _output;

        public GraphTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public async Task TestGetDisplayNames()
        {
            const string username = "username@mytenantname.onmicrosoft.com";
            const string password = "password";

            var graphClient = await GraphClientFactory.CreateAsync(username, password);

            var myNames = await graphClient.Me.Profile.Names.Request().WithMaxRetry(5).GetAsync();

            var myDisplayNames = myNames.Select(n => n.DisplayName);
            this._output.WriteLine("My display names: " + string.Join(", ", myDisplayNames));
        }
    }
}