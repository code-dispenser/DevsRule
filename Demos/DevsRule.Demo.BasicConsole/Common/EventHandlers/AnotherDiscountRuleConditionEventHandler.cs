using DevsRule.Core.Areas.Events;
using DevsRule.Demo.BasicConsole.Common.Events;
using DevsRule.Demo.BasicConsole.Common.Models;

namespace DevsRule.Demo.BasicConsole.Common.EventHandlers;

public class AnotherDiscountRuleConditionEventHandler : IEventHandler<AnotherDiscountRuleConditionEvent>
{
    private readonly AppSettings _appSettings;
    public AnotherDiscountRuleConditionEventHandler(AppSettings appSettings)
    
        => _appSettings = appSettings;

    public async Task Handle(AnotherDiscountRuleConditionEvent theEvent, CancellationToken cancellationToken)
    {
        await Console.Out.WriteLineAsync($"The event handler {nameof(AnotherDiscountRuleConditionEventHandler)} recieved the event {nameof(AnotherDiscountRuleConditionEvent)} which evaluated to {theEvent.IsSuccessEvent}");
        await Console.Out.WriteLineAsync($"Using the contructor injected appsettings an email could have been sent to {_appSettings.EmailAddress}");
    }
}
