using DevsRule.Core.Areas.Events;

namespace DevsRule.Tests.SharedDataAndFixtures.Events;

public class ConditionResultEvent : ConditionEventBase
{
    public ConditionResultEvent(string senderName, bool successEvent, Type contextType, string jsonContextData, string tenantID,List<Exception> executionExceptions, Exception? serializationException = null)
    : base(senderName, successEvent, contextType, jsonContextData, tenantID, executionExceptions, serializationException) { }
}

public class RuleResultEvent : RuleEventBase
{
    public RuleResultEvent(string senderName, bool successEvent, string successValue, string failureValue, string tenantID, List<Exception> executionExceptions)
    : base(senderName, successEvent, successValue,failureValue,  tenantID, executionExceptions) { }
}