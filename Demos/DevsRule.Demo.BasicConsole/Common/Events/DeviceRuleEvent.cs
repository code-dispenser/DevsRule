using DevsRule.Core.Areas.Events;

namespace DevsRule.Demo.BasicConsole.Common.Events
{
    public class DeviceRuleEvent : RuleEventBase
    {
        public DeviceRuleEvent(string senderName, bool successEvent, string successValue, string failureValue, string tenantID, List<Exception> executionExceptions) 
            
            : base(senderName, successEvent, successValue, failureValue, tenantID, executionExceptions) { }
    }
}
