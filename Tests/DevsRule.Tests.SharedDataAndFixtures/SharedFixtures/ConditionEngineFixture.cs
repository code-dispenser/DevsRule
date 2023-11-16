using DevsRule.Core.Areas.Engine;
using Xunit;

namespace DevsRule.Tests.SharedDataAndFixtures.SharedFixtures
{
    public class ConditionEngineFixture : IAsyncLifetime
    {
        public ConditionEngine ConditionEngine { get; } = new ConditionEngine();

        public ConditionEngineFixture()
        {
            
        }

        public async Task InitializeAsync()

            => await Task.CompletedTask;

        public async Task DisposeAsync()

            => await Task.CompletedTask;
 
    }
}
