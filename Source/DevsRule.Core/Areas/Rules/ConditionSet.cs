using DevsRule.Core.Areas.Events;
using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Core.Common.Validation;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;

namespace DevsRule.Core.Areas.Rules;

///<inheritdoc cref="IConditionSet"/>
public sealed class ConditionSet : IConditionSet
{
    private readonly List<dynamic> _conditions = new();

    public IReadOnlyList<ICondition> Conditions => _conditions.Cast<ICondition>().ToList().AsReadOnly();
    public string SetValue          { get; } = String.Empty;
    public string ConditionSetName  { get; }

    public ConditionSet(string conditionSetName)

        : this(conditionSetName, String.Empty) { }

    public ConditionSet(string conditionSetName, string setValue)
    {
        ConditionSetName  = Check.ThrowIfNullOrWhitespace(conditionSetName);
        SetValue          = setValue;
    }
    public ConditionSet(string conditionSetName, dynamic condition)
    {

        ConditionSetName  = Check.ThrowIfNullOrWhitespace(conditionSetName);
        SetValue          = String.Empty;

        AndCondition(condition);
    }
    public ConditionSet(string conditionSetName, dynamic condition, string setValue)
    {
        ConditionSetName = Check.ThrowIfNullOrWhitespace(conditionSetName);
        SetValue         = setValue;

        AndCondition(condition);
    }

    ///<inheritdoc />
    public ConditionSet AndCondition(dynamic condition)
    {
        _ = Check.ThrowIfNull(condition);

        var typeParam = condition.ContextType;

        if (false == typeof(Condition<>).MakeGenericType(typeParam).IsAssignableFrom(condition.GetType())) throw new ArgumentException("Condition must be assignable from ICondition<TContext>");

        if (false == _conditions.Exists(c => c.Equals(condition))) _conditions.Add(condition);

        return this;
    }

    ///<inheritdoc />
    public void RemoveCondition(dynamic condition)
    {
        var index = _conditions.FindIndex(c => c.Equals(condition));

        if (index > -1) _conditions.RemoveAt(index);
    }

    ///<inheritdoc />
    public void RemoveConditionByIndex(int index)
    {
        if (_conditions.Count > index && index >= 0) _conditions.RemoveAt(index);
    }

    ///<inheritdoc />
    public async Task<ConditionResult> EvaluateConditions(ConditionEvaluatorResolver resolver, RuleData contexts, EventPublisher eventPublisher, CancellationToken cancellationToken)
    {
        if (true == HasNullContextData(contexts)) throw new MissingRuleContextsException(GlobalStrings.Missing_Rule_Contexts_Null_Context_Exception_Message);
        
        if (this.Conditions.Count == 0) throw new MissingContditionsException(String.Format(GlobalStrings.No_Rule_Conditions_Exception_Message, this.ConditionSetName));

        var missingContexts = GetMissingContexts(contexts);

        if (missingContexts.Count > 0)
        {
            throw new MissingRuleContextsException(String.Format(GlobalStrings.Missing_Rule_Contexts_Exception_Message, this.ConditionSetName) + String.Join(", ", missingContexts));
        }


        var currentResult   = default(ConditionResult);
        var previousResult  = default(ConditionResult);
        var tenantID        = contexts.TenantID;

        for (int index = 0; index < _conditions.Count; index++)
        {
            var condition = _conditions[index];

            var contextData = GetContextDataForCondition(condition, contexts);

            Int64 startingTicks = 0;
            Int64 createdTicks;
            Int64 endingTicks;
            Int64 evalMicroseconds = 0;
            Int64 totalMicroseconds;

            var assemblyQualifiedName = condition.ContextType.AssemblyQualifiedName;

            try
            {
                startingTicks = Stopwatch.GetTimestamp();

                dynamic evalInstance = resolver(condition.EvaluatorTypeName, contextData.GetType());

                createdTicks = Stopwatch.GetTimestamp();

                EvaluationResult evaluationResult = await condition.EvaluateWith(evalInstance, contextData, cancellationToken,tenantID);

                var failureMessage = evaluationResult.IsSuccess ? condition.FailureMessage
                                                                : String.IsNullOrWhiteSpace(evaluationResult.FailureMeassage) ? condition.FailureMessage : evaluationResult.FailureMeassage;

                endingTicks= Stopwatch.GetTimestamp();

                evalMicroseconds    = ((createdTicks - startingTicks) * 1_000_000) / Stopwatch.Frequency;
                totalMicroseconds   = ((endingTicks - startingTicks) *  1_000_000) / Stopwatch.Frequency;

                currentResult = new ConditionResult(ConditionSetName, SetValue, condition.ConditionName, index, assemblyQualifiedName, condition.ToEvaluate, contextData, condition.EvaluatorTypeName, evaluationResult.IsSuccess,
                                                            failureMessage, evalMicroseconds, totalMicroseconds, tenantID, evaluationResult.Exception);


            }
            catch (Exception ex)
            {
                endingTicks = Stopwatch.GetTimestamp();

                totalMicroseconds   = ((endingTicks - startingTicks) *  1_000_000) / Stopwatch.Frequency;

                currentResult = new ConditionResult(ConditionSetName, SetValue, condition.ConditionName, index, assemblyQualifiedName, condition.ToEvaluate, contextData, condition.EvaluatorTypeName, false,
                                                            condition.FailureMessage, evalMicroseconds, totalMicroseconds, tenantID, ex);
            }

            if (condition.EventDetails != null) await RaiseEvent(eventPublisher, condition.EventDetails, currentResult, tenantID, cancellationToken).ConfigureAwait(false);


            if (previousResult is null)
            {
                if (false == currentResult.IsSuccess) break;
                previousResult = currentResult;
                continue;//no chain building required here
            }

            currentResult.EvaluationtChain = previousResult;
            /*
                * fail fast if result is false, equivalent to using a conditional logical AND with each condition result.
            */
            if (false == currentResult.IsSuccess) break;

            previousResult = currentResult;

        }

        return currentResult!;

    }

    private async Task RaiseEvent(EventPublisher eventPublisher, EventDetails eventDetails, ConditionResult result, string tenantID, CancellationToken cancellationToken)
    {
        var eventWhenType = eventDetails.EventWhenType;

        if ((EventWhenType.OnSuccess == eventWhenType && result.IsSuccess == true) || (EventWhenType.OnFailure == eventWhenType && result.IsSuccess == false) || (EventWhenType.OnSuccessOrFailure == eventWhenType))
        {
            var contextType = result.EvaluationData!.GetType();
            string? jsonDataClone = null;
            Exception? serializationExcepion = null;


            try
            {
                jsonDataClone = JsonSerializer.Serialize(result.EvaluationData, contextType);
            }
            catch (Exception ex) { serializationExcepion = ex; }

            var executionExceptions = new List<Exception>();

            if (result.Exception != null) executionExceptions.Add(result.Exception);

            var constructorInfo = Type.GetType(eventDetails.EventTypeName)!.GetConstructor(new[] { typeof(string), typeof(bool), typeof(Type), typeof(string), typeof(string), typeof(List<Exception>), typeof(Exception) });
            var eventToPublish = (ConditionEventBase)constructorInfo!.Invoke(new object[] { result.ConditionName, result.IsSuccess, contextType, jsonDataClone!, tenantID, executionExceptions, serializationExcepion! });

            await eventPublisher(eventToPublish, cancellationToken, eventDetails.PublishMethod).ConfigureAwait(false);
        }
    }


    private dynamic? GetContextDataForCondition(ICondition condition, RuleData contexts)
    {
        var contextData = contexts.Contexts.Where(c => c.ConditionName == condition.ConditionName && c.Data.GetType() == condition.ContextType).FirstOrDefault()?.Data;

        return contextData ?? contexts.Contexts.Where(c => c.Data.GetType() == condition.ContextType).FirstOrDefault()!.Data;
    }
    private List<string> GetMissingContexts(RuleData ruleData)
    {
        var typeNameList = _conditions.Select(c => c.ContextType.AssemblyQualifiedName).Cast<String>().ToList();

        if (ruleData.Length == 0) return typeNameList;

        foreach (var context in ruleData.Contexts)
        {
            typeNameList.RemoveAll(t => t == context.Data.GetType().AssemblyQualifiedName);
        }

        return typeNameList;
    }


    private bool HasNullContextData(RuleData contexts)
    {
        if (contexts == null) return true;

        foreach (var context in contexts.Contexts)
        {
            if (context.Data == null) return true;
        }

        return false;
    }


}
