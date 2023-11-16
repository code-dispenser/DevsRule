using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;

namespace DevsRule.Core.Common.Extensions;


public static class RuleResultExtensions
{


    public static RuleResult OnSuccess(this RuleResult thisRuleresult, Action<RuleResult> onSuccess)
    {
        if (thisRuleresult.IsSuccess) onSuccess(thisRuleresult); 

        return thisRuleresult;
    }
    public static RuleResult OnFailure(this RuleResult thisRuleresult, Action<RuleResult> onFailure)
    {
        if (false == thisRuleresult.IsSuccess) onFailure(thisRuleresult);

        return thisRuleresult;
    }

    public static async Task<RuleResult> OnSuccess(this Task<RuleResult> thisRuleResult, string ruleName, ConditionEngine conditionEngine, RuleData contexts)
    {
        var currentResult = await thisRuleResult;

        if (currentResult.IsSuccess)
        {
            var ruleResult = await conditionEngine.EvaluateRule(ruleName, contexts).ConfigureAwait(false);
            ruleResult.RuleResultChain = currentResult;
            return ruleResult;
        }

        return currentResult;
    }
    public static async Task<RuleResult> OnSuccess(this Task<RuleResult> thisRuleResult, Rule nextRule, ConditionEvaluatorResolver evaluatorResolver, RuleData contexts, EventPublisher eventPublisher)
    {
        var currentResult = await thisRuleResult;

        if (currentResult.IsSuccess)
        {
            var ruleResult = await nextRule.Evaluate(evaluatorResolver, contexts, eventPublisher).ConfigureAwait(false);
            ruleResult.RuleResultChain = currentResult;
            return ruleResult;
        }

        return currentResult;
    }
    public static async Task<RuleResult> OnSuccess(this Task<RuleResult> thisRuleResult, Action<RuleResult> act_onSuccess)
    {
        var currentResult = await thisRuleResult.ConfigureAwait(false);

        if (currentResult.IsSuccess) act_onSuccess(currentResult); 

        return currentResult;
    }

    public static async Task<RuleResult> OnSuccess(this Task<RuleResult> thisRuleResult, Func<Task<RuleResult>> onSuccess)
    {
        var currentResult = await thisRuleResult.ConfigureAwait(false);

        if (currentResult.IsSuccess)
        {
            var ruleResult = await onSuccess().ConfigureAwait(false);
            ruleResult.RuleResultChain = currentResult;
            return ruleResult;
        }

        return currentResult;
    }
    public static async Task<RuleResult> OnFailure(this Task<RuleResult> thisRuleResult, string ruleName, ConditionEngine conditionEngine, RuleData contexts)
    {
        var currentResult = await thisRuleResult.ConfigureAwait(false);

        if (false == currentResult.IsSuccess)
        {
            var ruleResult = await conditionEngine.EvaluateRule(ruleName, contexts).ConfigureAwait(false);
            ruleResult.RuleResultChain = currentResult;
            return ruleResult;
        }

        return currentResult;
    }



    public static async Task<RuleResult> OnFailure(this Task<RuleResult> thisRuleResult, Rule nextRule, ConditionEvaluatorResolver evaluatorResolver, RuleData contexts, EventPublisher eventPublisher)
    {
        var currentResult = await thisRuleResult.ConfigureAwait(false);

        if (false == currentResult.IsSuccess)
        {
            var ruleResult = await nextRule.Evaluate(evaluatorResolver, contexts, eventPublisher).ConfigureAwait(false);
            ruleResult.RuleResultChain = currentResult;
            return ruleResult;
        }

        return currentResult;
    }
    public static async Task<RuleResult> OnFailure(this Task<RuleResult> thisRuleResult, Action<RuleResult> act_onFailure)
    {
        var currentResult = await thisRuleResult.ConfigureAwait(false);

        if (false == currentResult.IsSuccess) act_onFailure(currentResult);

        return currentResult;
    }
    public static async Task<RuleResult> OnFailure(this Task<RuleResult> thisRuleResult, Func<RuleResult,Task<RuleResult>> onFailure)
    {
        var currentResult = await thisRuleResult.ConfigureAwait(false);

        if (false == currentResult.IsSuccess)
        {
            var ruleResult = await onFailure(currentResult).ConfigureAwait(false);
            ruleResult.RuleResultChain = currentResult;
            return ruleResult;
        }

        return currentResult;
    }


}
