using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Core.Common.Validation;

namespace DevsRule.Core.Areas.Rules;

/// <summary>
/// The result of a rule evaluation, containing all information about the evaluation path, failure messages, exceptions, timings and the overall 
/// outcome, and return a value if specified by the rule.
/// </summary>
public class RuleResult
{
    public RuleResult?      RuleResultChain      { get; internal set; } = null; 
    public string           RuleName             { get; }
    public string           DataTenantID         { get; }
    public string           RuleTenantID         { get; }
    public string           SuccessValue         { get; }
    public string           SuccessfulSet        { get; }
    public string           FailureValue         { get; }  
    public ConditionResult? EvaluationChain      { get; }
    public bool             IsSuccess            { get; }
    public bool             RuleDisabled         { get; }
    public  List<string>    FailureMessages      { get; }
    public  List<Exception> Exceptions           { get; }
    public int              TotalEvaluations     { get; }
    public Int64            RuleTimeMicroseconds { get; }
    public Int64            RuleTimeMilliseconds { get; }

    public RuleResult(string ruleName, string failureValue, ConditionResult? evaluationChain, string ruleTenantID, List<string> failureMessages, List<Exception> exceptions, int totalEvaluations, Int64 ruleTimeMicroseconds, bool ruleDisabled = false)
    {
        RuleName                = Check.ThrowIfNullOrWhitespace(ruleName);
        DataTenantID            = String.IsNullOrWhiteSpace(evaluationChain?.TenantID) ? "N/A" : evaluationChain.TenantID;
        RuleTenantID            = String.IsNullOrWhiteSpace(ruleTenantID) ? GlobalStrings.Default_TenantID : ruleTenantID;
        SuccessValue            = evaluationChain?.SetValue  ?? string.Empty;
        SuccessfulSet           = evaluationChain?.SetName   ?? string.Empty;
        IsSuccess               = evaluationChain?.IsSuccess ?? false;

        FailureValue            = failureValue  ?? string.Empty;
        EvaluationChain         = evaluationChain;
        FailureMessages         = failureMessages   ?? new List<string>();
        Exceptions              = exceptions        ?? new List<Exception>();

        TotalEvaluations        = totalEvaluations;
        RuleTimeMicroseconds    = ruleTimeMicroseconds;
        RuleTimeMilliseconds    = RuleTimeMicroseconds == 0 ? 0 : RuleTimeMicroseconds / 1000;

        RuleDisabled            = ruleDisabled;
    }



}
