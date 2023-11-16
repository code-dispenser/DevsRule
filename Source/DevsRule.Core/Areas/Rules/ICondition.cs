using DevsRule.Core.Areas.Events;

namespace DevsRule.Core.Areas.Rules;

/// <summary>
/// The condition to be evaluated. The condition itself will most likely be a lambda predicate held as a string in the ToEvaluate property.
/// </summary>
public interface ICondition
{
    Dictionary<string, string> AdditionalInfo { get; }

    EventDetails? EventDetails  { get; }
    string  ConditionName       { get; }
    Type    ContextType         { get; }
    string  EvaluatorTypeName   { get; }
    string  FailureMessage      { get; }
    bool    IsLambdaPredicate   { get; }
    string  ToEvaluate          { get; }


}
