namespace DevsRule.Core.Areas.Events;


/// <summary>
/// Base class used for events that are raised by rules.
/// </summary>
public abstract class RuleEventBase : IEvent
{
    public string   TenantID        { get; }
    public string   SenderName      { get; }
    public bool     IsSuccessEvent { get; }
    public string   SuccessValue    { get; }
    public string   FailureValue    { get; }
    public List<Exception> ExecutionExceptions { get; }

    public RuleEventBase(string senderName, bool isSuccessEvent, string successValue, string failureValue, string tenantID, List<Exception> executionExceptions)
    {
        SenderName          = senderName;
        IsSuccessEvent      = isSuccessEvent;
        SuccessValue        = successValue;
        FailureValue        = failureValue;
        TenantID            = tenantID;
        ExecutionExceptions = executionExceptions ?? new();

    }
}
