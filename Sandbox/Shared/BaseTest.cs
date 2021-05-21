using Xunit.Abstractions;

namespace Sandbox.Shared
{
    public abstract class BaseTest
    {
        protected BaseTest(ITestOutputHelper logger)
        {
            this.Logger = logger;
        }

        protected ITestOutputHelper Logger { get; }
    }
}