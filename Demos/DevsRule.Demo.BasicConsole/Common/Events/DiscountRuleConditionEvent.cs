using DevsRule.Core.Areas.Events;

namespace DevsRule.Demo.BasicConsole.Common.Events
{
    public class DiscountRuleConditionEvent : ConditionEventBase
    {
        public DiscountRuleConditionEvent(string senderName, bool successEvent, Type contextType, string jsonContextData, string tenantID, List<Exception> executionExceptions, Exception? serializationException = null) 
            : base(senderName, successEvent, contextType, jsonContextData, tenantID, executionExceptions, serializationException) {}

    }
}
