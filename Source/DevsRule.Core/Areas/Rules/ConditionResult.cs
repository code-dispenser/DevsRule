using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Seeds;
using DevsRule.Core.Common.Validation;

namespace DevsRule.Core.Common.Models;
/// <summary>
/// A completed condition evaluation result, containing all information regarding the evaluation, such as failure messages, exceptions, evaluation timings
/// and the owning conditions sets, SetValue, to be passed up the chain for the rule result if successful. Each ConditionResult contains the previous condition
/// result (or null if it was the first) forming a chain from the first evaluation (end of chain) to the last evaluation (front of chain).
/// </summary>
public class ConditionResult
{
    public string           ContextType         { get; }
    public string           SetName             { get; }
    public string           TenantID            { get; }
    public string           ConditionName       { get; }
    public string           SetValue            { get; } 
    public ConditionResult? EvaluationChain     { get; set; }
    public Exception?       Exception           { get; }
    public string           FailureMessage      { get; } 
    public bool             IsSuccess           { get; }
    public int              ConditionSetIndex   { get; }
    public string           ToEvaluate          { get; } = default!;
    public object?          EvaluationData      { get; }
    public string           EvaluatedBy         { get; } = default!;
    public Int64            EvalMicroseconds    { get; }
    public Int64            TotalMicroseconds   { get; }

    public ConditionResult(string setName, string? setValue, string conditionName, int conditionSetIndex, string contextType, string toEvaluate, 
                           object? evaluationData, string evaluatedBy, bool isSuccess, string failureMessage, Int64 evalMicroseconds, Int64 totalMicroseconds, string tenantID, Exception? exception = null)
    {
        SetName             = setName       ?? String.Empty;
        ConditionName       = conditionName ?? String.Empty;  
        ContextType         = contextType   ?? String.Empty;
        ToEvaluate          = toEvaluate    ?? String.Empty;
        EvaluatedBy         = evaluatedBy   ?? String.Empty;
        TenantID            = String.IsNullOrWhiteSpace(tenantID) ? "N/A" : tenantID;
        IsSuccess           = isSuccess;
        FailureMessage      = failureMessage ?? String.Empty;
        EvaluationData      = evaluationData;
        SetValue            = setValue ?? String.Empty;

        ConditionSetIndex   = conditionSetIndex;
        EvalMicroseconds    = evalMicroseconds;
        TotalMicroseconds   = totalMicroseconds;
        Exception           = exception;
    }
}
