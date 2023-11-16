namespace DevsRule.Core.Areas.Events;

public interface IEvent
{
    string TenantID     { get; }
    bool IsSuccessEvent { get; }
    string SenderName   { get; }

    List<Exception> ExecutionExceptions { get; }
}
