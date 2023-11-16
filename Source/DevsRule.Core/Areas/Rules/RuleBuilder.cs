using DevsRule.Core.Areas.Events;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Core.Common.Utilities;
using DevsRule.Core.Common.Validation;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace DevsRule.Core.Areas.Rules;

public interface IForConditionSetNamed
{
    IForCondition ForConditionSetNamed(string setName);
    IForCondition ForConditionSetNamed(string setName, string setSuccessValue);
}
public interface IForCondition
{
    IContinueWith WithCustomCondition<TContext>(string conditionName, string expressionString, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData, EventDetails? eventDetails = null);

    IContinueWith WithCustomPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> expression, string failureMessage, string evaluatorTypeName);
    IContinueWith WithCustomPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> expression, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData);
    IContinueWith WithCustomPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> expression, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData, EventDetails? eventDetails = null);

    IContinueWith WithPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> expression, string failureMessage);
    IContinueWith WithPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> expression, string failureMessage, EventDetails? eventDetails);

    IContinueWith WithRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage);
    IContinueWith WithRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions);
    IContinueWith WithRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, EventDetails eventDetails);
    IContinueWith WithRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions, EventDetails? eventDetails = null);

}
public interface IContinueWith
{
    IContinueWith AndCustomPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> expression, string failureMessage, string evaluatorTypeName);
    IContinueWith AndCustomPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> expression, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData);
    IContinueWith AndCustomPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> expression, string failureMessage, string evaluatorTypeName, Dictionary<string, string>? customData, EventDetails? eventDetails);

    IContinueWith AndCustomCondition<TContext>(string conditionName, string expressionString, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData);

    IContinueWith AndCustomCondition<TContext>(string conditionName, string expressionString, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData, EventDetails? eventDetails);
    IContinueWith AndPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> expression, string failureMessage);
    IContinueWith AndPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> expression, string failureMessage, EventDetails? eventDetails);

    IContinueWith AndRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage);
    IContinueWith AndRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions);
    IContinueWith AndRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, EventDetails? eventDetails);
    IContinueWith AndRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions, EventDetails? eventDetails = null);


    IForCondition OrConditionSetNamed(string setName);
    IForCondition OrConditionSetNamed(string setName, string setSuccessValue);
    ICreateRule WithFailureValue(string failureValue);
    ICreateRule WithoutFailureValue();
}
public interface ICreateRule { Rule CreateRule(string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID); }


public class RuleBuilder : IForConditionSetNamed, IForCondition, IContinueWith, ICreateRule
{
    private readonly Dictionary<string, List<dynamic>> _setConditions = new Dictionary<string, List<dynamic>>();
    private readonly SortedList<int, (string setName, string setValue)> _dictionaryOrder = new SortedList<int, (string setName, string setValue)>();//using seperate list as its easier than dictionary of dictionaries or casting with ordereddictionary etc
    private readonly EventDetails? _ruleEventDetails = null;

    private string _ruleName = String.Empty;
    private string _failureValue = String.Empty;
    private string _setName = String.Empty;


    private RuleBuilder(string ruleName, EventDetails? ruleEventDetails)
    {
        _ruleName           = Check.ThrowIfNullOrWhitespace(ruleName);
        _ruleEventDetails   = ruleEventDetails;
    }
    public static IForConditionSetNamed WithName(string ruleName, EventDetails? ruleEventDetails = null)
    {
        return new RuleBuilder(ruleName, ruleEventDetails);
    }
    public IForCondition ForConditionSetNamed(string setName)

        => ForConditionSetNamed(setName, String.Empty);

    public IForCondition ForConditionSetNamed(string setName, string setSuccessValue)
    {
        _setName = Check.ThrowIfNullOrWhitespace(setName);

        if (false == _setConditions.ContainsKey(setName))
        {
            _dictionaryOrder.Add(0, (setName, setSuccessValue));
            _setConditions[setName] = new List<dynamic>();
        }
        return this;
    }

    public IContinueWith WithCustomCondition<TContext>(string conditionName, string expressionString, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData)
    {
        AddCustomCondition<TContext>(conditionName, expressionString, failureMessage, evaluatorTypeName, customData);
        return this;
    }
    public IContinueWith WithCustomCondition<TContext>(string conditionName, string expressionString, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData, EventDetails? eventDetails = null)
    {
        AddCustomCondition<TContext>(conditionName, expressionString, failureMessage, evaluatorTypeName, customData, eventDetails);
        return this;
    }
    public IContinueWith WithCustomPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, string evaluatorTypeName)
    {
        AddPredicateCondition(conditionName, predicateExpression, failureMessage, evaluatorTypeName, new Dictionary<string, string>());
        return this;
    }

    public IContinueWith WithCustomPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData)
    {
        AddPredicateCondition(conditionName, predicateExpression, failureMessage, evaluatorTypeName, customData);
        return this;
    }
    public IContinueWith WithCustomPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData, EventDetails? eventDetails)
    {
        AddPredicateCondition(conditionName, predicateExpression, failureMessage, evaluatorTypeName, customData, eventDetails);
        return this;
    }
    public IContinueWith WithPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage)
    {
        AddPredicateCondition(conditionName, predicateExpression, failureMessage);
        return this;
    }
    public IContinueWith WithPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, EventDetails? eventDetails)
    {
        AddPredicateCondition(conditionName, predicateExpression, failureMessage, eventDetails);
        return this;
    }

    public IContinueWith WithRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage)
    {
        AddRegexCondition(conditionName, propertyExpression, pattern, failureMessage, RegexOptions.None, null);
        return this;
    }
    public IContinueWith WithRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions)
    {
        AddRegexCondition(conditionName, propertyExpression, pattern, failureMessage, regexOptions, null);
        return this;
    }
    public IContinueWith WithRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, EventDetails? eventDetails)
    {
        AddRegexCondition(conditionName, propertyExpression, pattern, failureMessage, RegexOptions.None, eventDetails);
        return this;
    }

    public IContinueWith WithRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions, EventDetails? eventDetails = null)
    {
        AddRegexCondition(conditionName, propertyExpression, pattern, failureMessage, regexOptions, eventDetails);
        return this;
    }

    public IContinueWith AndCustomCondition<TContext>(string conditionName, string expressionString, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData)
    {
        AddCustomCondition<TContext>(conditionName, expressionString, failureMessage, evaluatorTypeName, customData);
        return this;
    }
    public IContinueWith AndCustomCondition<TContext>(string conditionName, string expressionString, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData, EventDetails? eventDetails)
    {
        AddCustomCondition<TContext>(conditionName, expressionString, failureMessage, evaluatorTypeName, customData, eventDetails);
        return this;
    }

    public IContinueWith AndPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage)
    {
        AddPredicateCondition(conditionName, predicateExpression, failureMessage);
        return this;
    }

    public IContinueWith AndPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, EventDetails? eventDetails)
    {
        AddPredicateCondition(conditionName, predicateExpression, failureMessage, eventDetails);
        return this;
    }


    public IContinueWith AndCustomPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, string evaluatorTypeName)
    {
        AddPredicateCondition<TContext>(conditionName, predicateExpression, failureMessage, evaluatorTypeName, new Dictionary<string, string>(), null);
        return this;
    }

    public IContinueWith AndCustomPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData)
    {
        AddPredicateCondition(conditionName, predicateExpression, failureMessage, evaluatorTypeName, customData, null);
        return this;
    }
    public IContinueWith AndCustomPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, string evaluatorTypeName, Dictionary<string, string>? customData, EventDetails? eventDetails)
    {
        AddPredicateCondition<TContext>(conditionName, predicateExpression, failureMessage, evaluatorTypeName, customData ?? new Dictionary<string, string>(), eventDetails);
        return this;
    }
    public IContinueWith AndRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage)
    {
        AddRegexCondition(conditionName, propertyExpression, pattern, failureMessage, RegexOptions.None);
        return this;
    }
    public IContinueWith AndRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions)
    {
        AddRegexCondition(conditionName, propertyExpression, pattern, failureMessage, regexOptions);
        return this;
    }

    public IContinueWith AndRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, EventDetails? eventDetails)
    {
        AddRegexCondition(conditionName, propertyExpression, pattern, failureMessage, RegexOptions.None, eventDetails);
        return this;
    }

    public IContinueWith AndRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions, EventDetails? eventDetails = null)
    {
        AddRegexCondition(conditionName, propertyExpression, pattern, failureMessage, regexOptions, eventDetails);
        return this;
    }

    public IForCondition OrConditionSetNamed(string setName)

        => OrConditionSetNamed(setName, String.Empty);

    public IForCondition OrConditionSetNamed(string setName, string setSuccessValue)
    {
        _setName = Check.ThrowIfNullOrWhitespace(setName);

        if (false == _setConditions.ContainsKey(setName))
        {
            int keyValue = _dictionaryOrder.Keys.Max() + 1;
            _dictionaryOrder.Add(keyValue, (setName, setSuccessValue));
            _setConditions[setName] = new List<dynamic>();
        }

        return this;
    }

    public ICreateRule WithFailureValue(string failureValue)
    {
        _failureValue = failureValue ?? String.Empty;
        return this;
    }

    public ICreateRule WithoutFailureValue()
    {
        _failureValue           = String.Empty;
        return this;
    }

    public Rule CreateRule(string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)
    {
        var rule = new Rule(_ruleName, _failureValue, _ruleEventDetails, tenantID, cultureID);

        foreach (var keyPair in _dictionaryOrder)
        {
            var setName = keyPair.Value.setName;
            var setValue = keyPair.Value.setValue;

            var conditionSet = new ConditionSet(setName, setValue);

            foreach (var condition in _setConditions[setName])
            {
                conditionSet.AndCondition(condition);
            }

            rule.OrConditionSet(conditionSet);
        }

        return rule;
    }

    private void AddPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, EventDetails? eventDetails = null, string evaluatorTypeName = GlobalStrings.Predicate_Condition_Evaluator)

        => AddPredicateCondition<TContext>(conditionName, predicateExpression, failureMessage, evaluatorTypeName, new Dictionary<string, string>(), eventDetails);

    private void AddPredicateCondition<TContext>(string conditionName, Expression<Func<TContext, bool>> predicateExpression, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData, EventDetails? eventDetails = null)
    {
        conditionName       = Check.ThrowIfNullOrWhitespace(conditionName);
        predicateExpression = Check.ThrowIfNull(predicateExpression);
        failureMessage      = Check.ThrowIfNull(failureMessage);

        var predicateCondition = new PredicateCondition<TContext>(conditionName, predicateExpression, failureMessage, evaluatorTypeName, customData, eventDetails);

        if (false == _setConditions[_setName].Exists(c => c.ConditionName == conditionName)) _setConditions[_setName].Add(predicateCondition);
    }

    private void AddRegexCondition<TContext>(string conditionName, Expression<Func<TContext, object>> propertyExpression, string pattern, string failureMessage, RegexOptions regexOptions, EventDetails? eventDetails = null)
    {
        pattern             = String.IsNullOrWhiteSpace(pattern) ? String.Empty : pattern.Trim();
        conditionName       = Check.ThrowIfNullOrWhitespace(conditionName);
        propertyExpression  = Check.ThrowIfNull(propertyExpression);
        failureMessage      = Check.ThrowIfNull(failureMessage);

        Dictionary<string, string> additionalInfo = GeneralUtils.CreateDictionaryForRegex(pattern, regexOptions);

        var regexCondition = new RegexCondition<TContext>(conditionName, propertyExpression, failureMessage, additionalInfo, eventDetails);

        if (false == _setConditions[_setName].Exists(c => c.ConditionName == conditionName)) _setConditions[_setName].Add(regexCondition);
    }


    private void AddCustomCondition<TContext>(string conditionName, string expressionString, string failureMessage, string evaluatorTypeName, Dictionary<string, string> customData, EventDetails? eventDetails = null)
    {
        conditionName       = Check.ThrowIfNullOrWhitespace(conditionName);
        expressionString    = Check.ThrowIfNull(expressionString);
        failureMessage      = Check.ThrowIfNull(failureMessage);
        evaluatorTypeName   = Check.ThrowIfNull(evaluatorTypeName);

        customData ??= new Dictionary<string, string>();

        var customCondition = new Condition<TContext>(conditionName, expressionString, failureMessage, evaluatorTypeName, false, customData, eventDetails);

        if (false == _setConditions[_setName].Exists(c => c.ConditionName == conditionName)) _setConditions[_setName].Add(customCondition);
    }


}
