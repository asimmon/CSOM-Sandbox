using System.Threading.Tasks;
using Sandbox.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Sandbox
{
    public class CsomTests
    {
        private readonly ITestOutputHelper _output;

        public CsomTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public async Task TestOnPremises()
        {
            const string webUrl = "https://mysharepointserver/sites/MySite/";

            using (var context = await ClientContextFactory.CreateAsync(webUrl))
            {
                var list = context.Web.Lists.GetByTitle("Documents");
                context.Load(list);
                await context.ExecuteQueryRetryAsync();
                this._output.WriteLine(list.Title);
            }
        }

        [Fact]
        public async Task TestOffice365()
        {
            const string webUrl = "https://mytenantname.sharepoint.com/sites/MySite/";
            const string username = "username@mytenantname.onmicrosoft.com";
            const string password = "password";

            using (var context = await ClientContextFactory.CreateAsync(webUrl, username, password))
            {
                var list = context.Web.Lists.GetByTitle("Documents");
                context.Load(list);
                await context.ExecuteQueryRetryAsync();
                this._output.WriteLine(list.Title);
            }
        }
    }
}