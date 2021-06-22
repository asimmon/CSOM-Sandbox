using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Sandbox.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Sandbox
{
    public class GraphTests : BaseTest
    {
        public GraphTests(ITestOutputHelper logger) : base(logger)
        {
        }

        [Fact]
        public async Task TestGetDisplayNames()
        {
            const string username = "username@mytenantname.onmicrosoft.com";
            const string password = "password";

            var graphClient = await Shared.GraphClientFactory.CreateAsUserAsync(username, password);

            var myNames = await graphClient.Me.Profile.Names.Request().WithMaxRetry(5).GetAsync();

            var myDisplayNames = myNames.Select(n => n.DisplayName);
            this.Logger.WriteLine("My display names: " + string.Join(", ", myDisplayNames));
        }

        [Fact]
        public async Task TestGetGroupsWithAppToken()
        {
            var graphClient = await Shared.GraphClientFactory.CreateAsAppAsync("<yourTenant>");

            var groups = await graphClient.Groups.Request().WithMaxRetry(5).GetAsync();

            var groupNames = groups.Select(n => n.DisplayName);
            this.Logger.WriteLine("Group names: " + string.Join(", ", groupNames));
        }
    }
}