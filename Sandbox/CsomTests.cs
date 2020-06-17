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
        public void TestOnPremises()
        {
            const string webUrl = "http://mysharepointserver/sites/MySite/";

            using (var context = ClientContextFactory.Create(webUrl))
            {
                var list = context.Web.Lists.GetByTitle("Documents");
                context.Load(list);
                context.ExecuteQueryRetry();
                this._output.WriteLine(list.Title);
            }
        }

        [Fact]
        public void TestOffice365()
        {
            const string webUrl = "https://mytenantname.sharepoint.com/sites/MySite/";
            const string username = "username@mytenantname.onmicrosoft.com";
            const string password = "password";

            using (var context = ClientContextFactory.Create(webUrl, username, password))
            {
                var list = context.Web.Lists.GetByTitle("Documents");
                context.Load(list);
                context.ExecuteQueryRetry();
                this._output.WriteLine(list.Title);
            }
        }
    }
}