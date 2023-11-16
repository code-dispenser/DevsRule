using System.Text.Json;

namespace DevsRule.Core.Areas.Events;

/// <summary>
/// Base class used for events that are raised by conditions.
/// </summary>
public abstract class ConditionEventBase : IEvent
{
    private readonly string? _jsonContextData;
    public string TenantID      { get; }
    public Type ContextType     { get; }
    public bool IsSuccessEvent  { get; }
    public string SenderName    { get; }
    public List<Exception>  ExecutionExceptions { get; }
    public Exception?       SerializationException { get; private set; }

    public ConditionEventBase(string senderName, bool isSuccessEvent, Type contextType, string jsonContextData, string tenantID, List<Exception> executionExceptions, Exception? serializationException = null)
    {
        SenderName = senderName;
        IsSuccessEvent = isSuccessEvent;
        ContextType = contextType;
        _jsonContextData = jsonContextData;
        TenantID = tenantID;
        ExecutionExceptions = executionExceptions ?? new();
        SerializationException = serializationException;
    }

    /// <summary>
    /// TryGetData allows for the retrieval of data via an out parameter.
    /// </summary>
    /// <param name="contextData">A cloned copy of the data that was supplied to the condition for evaluataion.</param>
    /// <returns>true if the context data is available, without serializition issues otherwise false.</returns>
    public bool TryGetData(out object contextData)
    {
        contextData = default!;

        if (_jsonContextData != null && SerializationException == null)
        {
            contextData = JsonSerializer.Deserialize(_jsonContextData, ContextType)!;
            return true;
        }

        return false;

    }

}
