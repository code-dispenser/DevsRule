using DevsRule.Core.Areas.Events;
using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Core.Common.Validation;
using System.Diagnostics;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace DevsRule.Core.Areas.Rules;

///<inheritdoc cref="IRule"/>
public class Rule : IRule
{
    private List<IConditionSet> _conditionSets = new();
    public IReadOnlyList<IConditionSet> ConditionSets => _conditionSets.AsReadOnly();

    public EventDetails? RuleEventDetails { get; }

    public string CultureID     { get; } = GlobalStrings.Default_TenantID;
    public string TenantID      { get; } = GlobalStrings.Default_CultureID;
    public string RuleName      { get; }
    public string FailureValue  { get; }
    public bool   IsEnabled     { get; set; } = true;


    public Rule(string ruleName) : this(ruleName, String.Empty, null, GlobalStrings.Default_TenantID, GlobalStrings.Default_CultureID) { }

    public Rule(string ruleName, string failureValue = "", EventDetails? ruleEventDetails = null, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)
    {
        CultureID           = String.IsNullOrWhiteSpace(cultureID) ? GlobalStrings.Default_CultureID : cultureID;
        TenantID            = String.IsNullOrWhiteSpace(tenantID) ? GlobalStrings.Default_TenantID : tenantID;
        RuleName            = Check.ThrowIfNullOrWhitespace(ruleName);
        FailureValue        = failureValue;
        RuleEventDetails    = ruleEventDetails;
    }

    public Rule(string ruleName, IConditionSet conditionSet, string failureValue = "", EventDetails? ruleEventDetails = null, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)
    {
        CultureID           = String.IsNullOrWhiteSpace(cultureID) ? GlobalStrings.Default_CultureID : cultureID;
        TenantID            = String.IsNullOrWhiteSpace(tenantID) ? GlobalStrings.Default_TenantID : tenantID;
        RuleName            = Check.ThrowIfNullOrWhitespace(ruleName);
        FailureValue        = failureValue;
        RuleEventDetails    = ruleEventDetails;

        OrConditionSet(conditionSet);
    }

    ///<inheritdoc />
    public Rule OrConditionSet(IConditionSet conditionSet)
    {
        conditionSet = Check.ThrowIfNull(conditionSet);
        _conditionSets.Add(conditionSet);

        return this;
    }

    ///<inheritdoc />
    public async Task<RuleResult> Evaluate(ConditionEvaluatorResolver resolver, RuleData contexts, EventPublisher eventPublisher, CancellationToken cancellationToken = default)
    {
        Int64 startingTicks = Stopwatch.GetTimestamp();
        var setResult = default(ConditionResult);

        if (false == this.IsEnabled) return BuildDisabledRuleResult(Stopwatch.GetTimestamp() - startingTicks);

        try
        {
            if (contexts is null || contexts.Length == 0) throw new MissingRuleContextsException(GlobalStrings.No_Rule_Contexts_Exception_Message);
            if (this.ConditionSets.Count == 0) throw new MissingContditionSetsException(GlobalStrings.No_Rule_Condition_Sets_Exception_Message);

            var conditionNames    = _conditionSets.SelectMany(conditionSet => conditionSet.Conditions).Select(condition => condition.ConditionName).ToList();
            var unmatchedContexts = GetUnMatchedDataContexts(conditionNames, contexts);

            if (unmatchedContexts.Count > 0)
            {
                throw new MissingRuleContextsException(String.Format(GlobalStrings.Unmatched_Condition_Contexts_Exception_Message, this.RuleName) + String.Join(", ", unmatchedContexts));
            }


            foreach (var conditionSet in _conditionSets)
            {
                var currentResult = await conditionSet.EvaluateConditions(resolver, contexts, eventPublisher, cancellationToken).ConfigureAwait(false);

                if (setResult is null)
                {
                    setResult = currentResult;
                    if (true == HasExceptionInChain(currentResult)) break;
                    if (true == currentResult.IsSuccess) break;//Logical OR so done if true or fail due to exception
                    continue;//nothing to chain
                }

                SetEndOfChain(ref setResult, ref currentResult);

                setResult = currentResult;

                if (true == HasExceptionInChain(currentResult)) break;
                if (true == currentResult.IsSuccess) break;//Logical or so done if true
            }

            if (this.RuleEventDetails != null)
            {
                await RaiseEvent(eventPublisher, this.RuleEventDetails, this.RuleName, setResult!.IsSuccess, setResult.SetValue, this.FailureValue, setResult.TenantID, GetExecutionExceptions(setResult), cancellationToken)
                        .ConfigureAwait(false);
            }
            var totalRuleMicroseconds = ((Stopwatch.GetTimestamp() - startingTicks) * 1_000_000) / Stopwatch.Frequency;

            return BuildResult(setResult!, totalRuleMicroseconds);
        }
        catch (Exception ex)
        {
            return new RuleResult(this.RuleName, this.FailureValue, setResult,this.TenantID, new List<string>(), new List<Exception> { ex }, -1, -1);
        }

    }

    private void SetEndOfChain(ref ConditionResult previousResult, ref ConditionResult currentResult)
    {
        if (currentResult.EvaluationtChain == null)
        {
            currentResult.EvaluationtChain = previousResult;
            return;
        }

        var resultAtChainEnd = currentResult;

        while (resultAtChainEnd.EvaluationtChain != null)
        {
            resultAtChainEnd = resultAtChainEnd.EvaluationtChain;
        }

        resultAtChainEnd.EvaluationtChain = previousResult;

    }

    private bool HasExceptionInChain(ConditionResult currentResult)
    
        => currentResult.Exception != null;

    private List<Exception> GetExecutionExceptions(ConditionResult setResult)
    {
        var exceptions = new List<Exception>();
        var resultAtChainEnd = setResult;

        while (resultAtChainEnd != null)
        {
            if (resultAtChainEnd.Exception != null) exceptions.Add(resultAtChainEnd.Exception);
            resultAtChainEnd = resultAtChainEnd.EvaluationtChain;
        }

        return exceptions;
    }
    private RuleResult BuildResult(ConditionResult setResult, Int64 totalTimeForRule)
    {
        List<Exception> exceptions      = new ();
        List<string>    failureMessages = new ();
        int evaluationCount = 0;

        var checkResult = setResult;

        while (checkResult is not null)
        {
            if (checkResult.Exception != null) exceptions.Insert(0, checkResult.Exception);
            if (false == checkResult.IsSuccess) failureMessages.Insert(0, checkResult.FailureMessage);

            checkResult = checkResult.EvaluationtChain;

            evaluationCount++;
        }

        var isSuccess = setResult.IsSuccess ? exceptions.Count == 0 : false;

        return new RuleResult(this.RuleName, this.FailureValue, setResult, this.TenantID, failureMessages.ToList(), exceptions.ToList(), evaluationCount, totalTimeForRule);
    }

    private async Task RaiseEvent(EventPublisher eventPublisher, EventDetails eventDetails, string senderName, bool successEvent, string successValue, string failureValue, string tenantID, List<Exception> exceptions, CancellationToken cancellationToken)
    {
        var eventWhenType = eventDetails.EventWhenType;

        if ((EventWhenType.OnSuccess == eventWhenType && successEvent == true) || (EventWhenType.OnFailure == eventWhenType && successEvent == false) || (EventWhenType.OnSuccessOrFailure == eventWhenType))
        {
            var executionExceptions = new List<Exception>(exceptions);
            var stringType = typeof(string);
            var constructorInfo = Type.GetType(eventDetails.EventTypeName)!.GetConstructor(new[] { stringType, typeof(bool), stringType, stringType, stringType, typeof(List<Exception>) });
            var eventToPublish = (RuleEventBase)constructorInfo!.Invoke(new object[] { senderName, successEvent, successValue, failureValue, tenantID, executionExceptions });

            await eventPublisher(eventToPublish, cancellationToken, eventDetails.PublishMethod).ConfigureAwait(false);
        }
    }

    private RuleResult BuildDisabledRuleResult(Int64 totalTimeForRule)

       => new (this.RuleName, this.FailureValue, null, this.TenantID, new(), new(), 0, totalTimeForRule, true);

    private List<string> GetUnMatchedDataContexts(List<string> conditionNames, RuleData ruleData)
    {
        List<string> unmatchedContext = new();

        foreach (var data in ruleData.Contexts.Where(d => String.IsNullOrWhiteSpace(d.ConditionName) == false))
        {
            if (false == conditionNames.Any(c => c == data.ConditionName)) unmatchedContext.Add(data.ConditionName);
        }

        return unmatchedContext;
    }

    /*
        * Not keen on using attributes and I also wanted a layer between the rule and json for any slight changes so as not to affect serialization and desearialization.
        * Opted pted to create a small jsonrule object as an intermediary -> DevsRule.Core.Common.Models
    */
    internal JsonRule RuleToJsonRule()
    {
        var jsonRule = new JsonRule();

        jsonRule.RuleName           = this.RuleName;
        jsonRule.FailureValue       = this.FailureValue;
        jsonRule.CultureID          = this.CultureID;
        jsonRule.TenantID           = this.TenantID;
        jsonRule.IsEnabled          = this.IsEnabled;
        jsonRule.RuleEventDetails   = EventDetails.ToJsonRule(this.RuleEventDetails);

        foreach (var conditionSet in this.ConditionSets)
        {
            JsonRule.ConditionSet jsonSet = new ();
            jsonSet.ConditionSetName      = conditionSet.ConditionSetName;

            jsonSet.SetValue = conditionSet.SetValue;

            foreach (var condition in conditionSet.Conditions)
            {
                var jsonCondition = new JsonRule.ConditionSet.Condition();

                jsonCondition.ConditionName         = condition.ConditionName;
                jsonCondition.FailureMessage        = condition.FailureMessage;
                jsonCondition.AdditionalInfo        = condition.AdditionalInfo.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                jsonCondition.ToEvaluate            = condition.ToEvaluate;
                jsonCondition.EvaluatorTypeName     = condition.EvaluatorTypeName;
                jsonCondition.ContextTypeName       = condition.ContextType.FullName;
                jsonCondition.IsLambdaPredicate     = condition.IsLambdaPredicate;
                jsonCondition.ConditionEventDetails = EventDetails.ToJsonRule(condition.EventDetails);

                jsonSet.Conditions.Add(jsonCondition);
            }

            jsonRule.ConditionSets.Add(jsonSet);

        }
        return jsonRule;
    }

    ///<inheritdoc />
    public string ToJsonString(bool writeIndented = false, bool useEscaped = true)

        => JsonSerializer.Serialize<JsonRule>(RuleToJsonRule(), new JsonSerializerOptions { WriteIndented =  writeIndented, Encoder = useEscaped ? JavaScriptEncoder.Default : JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

    /// <summary>
    /// Creates a deep clone of the Rule.
    /// </summary>
    /// <param name="rule">The Rule to deep clone.</param>
    /// <returns>a deep cloned rule.</returns>
    public static Rule DeepCloneRule(Rule rule)
    {
        var ruleEventDetails = rule.RuleEventDetails == null ? null : new EventDetails(rule.RuleEventDetails.EventTypeName, rule.RuleEventDetails.EventWhenType, rule.RuleEventDetails.PublishMethod);

        var ruleClone = new Rule(rule.RuleName, rule.FailureValue, ruleEventDetails, rule.TenantID, rule.CultureID);

        foreach (var conditionSet in rule.ConditionSets)
        {
            var conditionSetClone = new ConditionSet(conditionSet.ConditionSetName, conditionSet.SetValue);

            foreach (var condition in conditionSet.Conditions)
            {
                EventDetails? eventDetails = null;

                if (condition.EventDetails is not null) eventDetails = new EventDetails(condition.EventDetails.EventTypeName, condition.EventDetails.EventWhenType, condition.EventDetails.PublishMethod);

                var eventDetailsType = typeof(EventDetails);
                var stringType = typeof(string);
                var contructorTypes = new Type[7] { stringType, stringType, stringType, stringType, typeof(bool), typeof(Dictionary<string, string>), eventDetailsType };
                Type closedConditionType = typeof(Condition<>).MakeGenericType(condition.ContextType);
                var additionalInfo = ((Dictionary<string, string>)(condition.AdditionalInfo)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                var conditionClone = closedConditionType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, contructorTypes)!
                                                .Invoke(new object[] { condition.ConditionName, condition.ToEvaluate, condition.FailureMessage, condition.EvaluatorTypeName, condition.IsLambdaPredicate, additionalInfo, eventDetails! });

                conditionSetClone.AndCondition(conditionClone);
            }

            ruleClone.OrConditionSet(conditionSetClone);
        }

        return ruleClone;
    }



}
