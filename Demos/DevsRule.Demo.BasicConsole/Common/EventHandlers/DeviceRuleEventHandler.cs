using DevsRule.Core.Areas.Events;
using DevsRule.Demo.BasicConsole.Common.Events;

namespace DevsRule.Demo.BasicConsole.Common.EventHandlers
{
    public class DeviceRuleEventHandler : IEventHandler<DeviceRuleEvent>
    {
        public async Task Handle(DeviceRuleEvent theEvent, CancellationToken cancellationToken)
        {
           await Console.Out.WriteLineAsync($"The dynamic rule event for : {theEvent.SenderName} was handled, the rule evaluated to: {theEvent.IsSuccessEvent} for TenantID: {theEvent.TenantID}");
        }
    }
}
