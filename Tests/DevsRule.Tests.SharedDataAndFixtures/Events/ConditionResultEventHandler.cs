using DevsRule.Core.Areas.Events;

namespace DevsRule.Tests.SharedDataAndFixtures.Events
{
    public class ConditionResultEventHandler : IEventHandler<ConditionResultEvent>
    {
        public bool Handled = false;

        public ConditionResultEventHandler()
        {
 
        }
        public async Task Handle(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            Handled = true;
            await Task.CompletedTask; 
           
        }
    }
}
