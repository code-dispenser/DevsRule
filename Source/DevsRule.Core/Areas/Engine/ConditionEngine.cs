using DevsRule.Core.Areas.Caching;
using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using DevsRule.Core.Common.Utilities;
using DevsRule.Core.Common.Validation;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace DevsRule.Core.Areas.Engine;

///<inheritdoc cref="IConditionEngine"/>
public class ConditionEngine : IConditionEngine
{
    private readonly List<(string assemblyQualifiedName, string fullName)> _assemblyTypeNames;

    private readonly InternalCache          _cache = new();
    private readonly EventAggregator        _eventAggregator;
    private readonly Func<Type, dynamic>?   _customTypeResolver;
    private readonly bool                   _allowDependencyInjectionEvaluators = false;
    
    /// <summary>
    /// Initialises the condition engine with a callback function that is used to resolve custom evaluators requiring dependency injection support
    /// and/or that uses dynamic event handlers. Please see the online documentation for more information on how to link this to your choosen 
    /// IOC container.
    /// </summary>
    /// <param name="customTypeResolver">A call back function used to communicate with an IOC container</param>
    public ConditionEngine(Func<Type, object> customTypeResolver)
    {
        _customTypeResolver                  = customTypeResolver;
        _allowDependencyInjectionEvaluators  = true;
        _assemblyTypeNames                   = GetAssemblyTypeNames();
        _eventAggregator                     = new EventAggregator(_customTypeResolver);
    }
    ///<inheritdoc />
    public ConditionEngine()

        => (_assemblyTypeNames, _eventAggregator)  = (GetAssemblyTypeNames(), new EventAggregator());

    private List<(string assemblyQualifiedName, string fullName)> GetAssemblyTypeNames()

        => GeneralUtils.AssemblyTypeNames;//Assigning to engine field but only used in ,

    #region Caching
    ///<inheritdoc />
    public void AddOrUpdateRule(Rule rule, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)
        
        => this.AddOrUpdateRule(rule, tenantID, cultureID, false);

    private void AddOrUpdateRule(Rule rule, string tenantID, string cultureID, bool fromJson)
    {
        Check.ThrowIfNull(rule);

        tenantID    = String.IsNullOrWhiteSpace(tenantID) ? GlobalStrings.Default_TenantID : tenantID;
        cultureID   = String.IsNullOrWhiteSpace(cultureID) ? GlobalStrings.Default_CultureID : cultureID;

        var cacheKeyPart = String.Join("_", GlobalStrings.CacheKey_Part_Rule, rule.RuleName);
        
        var ruleToCache = fromJson == true ? rule : Rule.DeepCloneRule(rule);//clone it to stop any chance of the cache getting changed.

        _cache.AddOrUpdateItem(cacheKeyPart, ruleToCache, tenantID, cultureID);
    }
    ///<inheritdoc />
    public bool TryGetRule(string ruleName, out Rule? rule, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)
    {
        var cacheKeyPart = String.Join("_", GlobalStrings.CacheKey_Part_Rule, ruleName);

        if (true == _cache.TryGetItem<Rule>(cacheKeyPart, out var cacheItem, tenantID, cultureID))
        {
            rule = Rule.DeepCloneRule(cacheItem!);//clone it to stop any chance of the cache getting changed.
            return true;
        }

        rule = default;
        return false;
    }
    ///<inheritdoc />
    public bool ContainsRule(string ruleName, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)

        => _cache.ContainsItem(String.Join("_", GlobalStrings.CacheKey_Part_Rule, ruleName), tenantID, cultureID);
    
    ///<inheritdoc />
    public void RemoveRule(string ruleName, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)

        => _cache.RemoveItem(String.Join("_", GlobalStrings.CacheKey_Part_Rule, ruleName), tenantID, cultureID);

    #endregion

    #region Rules

    ///<inheritdoc />
    public async Task<RuleResult> EvaluateRule(string ruleName, RuleData contexts)

        => await EvaluateRule(ruleName, contexts, CancellationToken.None).ConfigureAwait(false);

    ///<inheritdoc />
    public async Task<RuleResult> EvaluateRule(string ruleName, RuleData contexts, CancellationToken cancellationToken)

        => await EvaluateRule(ruleName, contexts, cancellationToken, GlobalStrings.Default_TenantID, GlobalStrings.Default_CultureID).ConfigureAwait(false);

    ///<inheritdoc />
    public async Task<RuleResult> EvaluateRule(string ruleName, RuleData contexts, CancellationToken cancellationToken, string tenantID = GlobalStrings.Default_TenantID, string cultureID = GlobalStrings.Default_CultureID)
    {
        var cacheKeyPart = String.Join("_", GlobalStrings.CacheKey_Part_Rule, ruleName);

        if (false == _cache.TryGetItem<Rule>(cacheKeyPart, out var rule, tenantID, cultureID) || rule == null) throw new RuleNotFoundException(String.Format(GlobalStrings.No_Rule_In_Cache_Exception_Message, ruleName));

        return await rule!.Evaluate(this.GetEvaluatorByName, contexts, this.EventPublisher, cancellationToken).ConfigureAwait(false);
    }

    ///<inheritdoc />
    public void IngestRuleFromJson(string ruleJson)
    {
        var rule = RuleFromJson(ruleJson);
        this.AddOrUpdateRule(rule, rule.TenantID, rule.CultureID, true);
    }

    ///<inheritdoc />
    public Rule RuleFromJson(string ruleJson)
    {
        ruleJson = Check.ThrowIfNullOrWhitespace(ruleJson);

        try
        {
            JsonRule? jsonRule = JsonSerializer.Deserialize<JsonRule>(ruleJson);
            return RuleFromJsonRule(jsonRule!, _assemblyTypeNames);
        }
        catch (MissingConditionToEvaluatePropertyValue) { throw; }
        catch (ContextTypeAssemblyNotFound) { throw; }
        catch (Exception ex)
        {
            throw new RuleFromJsonException(GlobalStrings.Rule_From_Json_Exception_Message, ex);
        }
    }

    private Rule RuleFromJsonRule(JsonRule jsonRule, List<(string assemblyQualifiedName, string fullName)> assemblyTypeNames)
    {
        var stringType          = typeof(string);
        var types               = new Type[5] { stringType, stringType, typeof(EventDetails), stringType, stringType, };
        var ruleName            = Check.ThrowIfNullOrWhitespace(jsonRule.RuleName);
        var failureValue        = jsonRule.FailureValue ?? String.Empty;
        var tenantID            = jsonRule.TenantID ?? GlobalStrings.Default_TenantID;
        var cultureID           = jsonRule.CultureID ?? GlobalStrings.Default_CultureID;
        var ruleEventDetails    = EventDetails.FromJsonRule(jsonRule.RuleEventDetails);

        if (false == String.IsNullOrWhiteSpace(jsonRule.RuleEventDetails?.EventTypeName) && ruleEventDetails == null)
        {
            throw new EventNotFoundException(String.Format(GlobalStrings.Event_Not_Found_Exception_Message, jsonRule.RuleEventDetails?.EventTypeName));
        }


        var rule = (Rule)typeof(Rule).GetConstructor(types)!.Invoke(new object[] { ruleName!, failureValue, ruleEventDetails!, tenantID, cultureID });

        rule.IsEnabled = jsonRule.IsEnabled;

        string[] excludedNamespaces = new string[] { "System.", "Microsoft." };

        for (int setIndex = 0; setIndex < jsonRule.ConditionSets.Count; setIndex++)
        {
            var jsonRuleConditionSet    = jsonRule.ConditionSets[setIndex];
            var setName                 = jsonRuleConditionSet.ConditionSetName;
            var setValue                = jsonRuleConditionSet.SetValue ?? String.Empty;

            ConditionSet conditionSet = new (conditionSetName: setName!, setValue: setValue);

            foreach (var jsonRuleCondition in jsonRuleConditionSet.Conditions)
            {
                EventDetails? eventDetails = EventDetails.FromJsonRule(jsonRuleCondition.ConditionEventDetails);

                if (false == String.IsNullOrWhiteSpace(jsonRuleCondition.ConditionEventDetails?.EventTypeName) && eventDetails == null)
                {
                    throw new EventNotFoundException(String.Format(GlobalStrings.Event_Not_Found_Exception_Message, jsonRuleCondition.ConditionEventDetails?.EventTypeName));
                }

                _ = Check.ThrowIfNullOrWhitespace(jsonRuleCondition.ConditionName?.Trim());

                if (true == String.IsNullOrWhiteSpace(jsonRuleCondition.ToEvaluate)) throw new MissingConditionToEvaluatePropertyValue(String.Format(GlobalStrings.Missing_Condition_ToEvaluate_Property_Value_Exception_Message, jsonRuleCondition.ConditionName, setName));

                var contextSearchName = jsonRuleCondition?.ContextTypeName?.Contains('.') == true ? jsonRuleCondition?.ContextTypeName : String.Concat(".", jsonRuleCondition?.ContextTypeName);

                var assemblyQualifiedName = assemblyTypeNames.Where(t => t.fullName.EndsWith(contextSearchName!)).FirstOrDefault().assemblyQualifiedName ?? String.Empty;

                if (String.IsNullOrWhiteSpace(assemblyQualifiedName)) throw new ContextTypeAssemblyNotFound(String.Format(GlobalStrings.Context_Type_Assembly_Not_Found_Exception_Message, jsonRuleCondition!.ConditionName));

                var contextType = Type.GetType(assemblyQualifiedName);

                Type[] typeArgs = { contextType! };

                MethodInfo methodInfo = this.GetType().GetMethod(nameof(this.ConditionFromJsonCondition), BindingFlags.NonPublic | BindingFlags.Instance)!
                                                .MakeGenericMethod(typeArgs);

                var condition = methodInfo.Invoke(this, new object[] { jsonRuleCondition!, eventDetails! });

                conditionSet.AndCondition(condition!);
            }

            rule.OrConditionSet(conditionSet);
        }

        return rule;
    }

    private Condition<TContext> ConditionFromJsonCondition<TContext>(JsonRule.ConditionSet.Condition jsonRuleCondition, EventDetails? eventDetails)
    {
        /*
            * I could not get the code to work without seperating out the code in to this and CreateConditionCreator<TContext> method?
        */
        var failureMessage      = false == String.IsNullOrWhiteSpace(jsonRuleCondition.FailureMessage) ? jsonRuleCondition.FailureMessage.Trim() : "Condition failed";
        var evaluatorTypeName   = false == String.IsNullOrWhiteSpace(jsonRuleCondition.EvaluatorTypeName) ? jsonRuleCondition.EvaluatorTypeName.Trim() : "N/A";
        /*
            * Add a condition creator for each combination of the condition<TContext> and evaluator type, should only be a handfull
         */
        var cacheKey = String.Join("_", GlobalStrings.CacheKey_Part_ConditionCreator, typeof(TContext).FullName, evaluatorTypeName);

        var conditionCreator = _cache.GetOrAddItem(cacheKey, () => CreateConditionCreator<TContext>());

        return conditionCreator(jsonRuleCondition.ConditionName!, jsonRuleCondition.ToEvaluate!, failureMessage, evaluatorTypeName, jsonRuleCondition.IsLambdaPredicate, jsonRuleCondition.AdditionalInfo, eventDetails!);
    }

    private Func<string, string, string, string, bool, Dictionary<string, string>, EventDetails, Condition<TContext>> CreateConditionCreator<TContext>()
    {
        Type eventDetailsType   = typeof(EventDetails);
        Type conditionType      = typeof(Condition<>).MakeGenericType(typeof(TContext));
        Type stringType         = typeof(string);
        Type dictionaryType     = typeof(Dictionary<,>).MakeGenericType(stringType, stringType);
        Type boolType           = typeof(bool);
        Type[] paramTypes       = new Type[] { stringType, stringType, stringType, stringType, boolType, dictionaryType, eventDetailsType };

        var conditionNameParam      = Expression.Parameter(stringType, "conditionName");
        var toEvaluateParam         = Expression.Parameter(stringType, "toEvaluate");
        var failureMessageParam     = Expression.Parameter(stringType, "failureMessage");
        var evaluatorTypeNameParam  = Expression.Parameter(stringType, "evaluatorTypeName");
        var additionalInfoParam     = Expression.Parameter(dictionaryType, "additionalInfo");
        var isPredicaterParam       = Expression.Parameter(boolType, "isLambdaPredicate");
        var eventDetailsParam       = Expression.Parameter(eventDetailsType, "eventDetails");

        var constructor     = conditionType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, paramTypes)!;
        var newCondition    = Expression.New(constructor, conditionNameParam, toEvaluateParam, failureMessageParam, evaluatorTypeNameParam, isPredicaterParam, additionalInfoParam, eventDetailsParam);

        var lambdaFunc = Expression.Lambda<Func<string, string, string, string, bool, Dictionary<string, string>, EventDetails, Condition<TContext>>>
            (newCondition, conditionNameParam, toEvaluateParam, failureMessageParam, evaluatorTypeNameParam, isPredicaterParam, additionalInfoParam, eventDetailsParam);

        return lambdaFunc.Compile();

    }

    ///<inheritdoc />
    public void RegisterCustomEvaluator(string evaluatorName, Type evaluatorType)

        => _cache.AddOrUpdateItem(String.Join("_", GlobalStrings.CacheKey_Part_Evaluator_Type, evaluatorName), evaluatorType);
    /*
        * Made two separate methods to negate mistakes with one method and a flag 
    */
    ///<inheritdoc />
    public void RegisterCustomEvaluatorForDependencyInjection(string evaluatorName, Type evaluatorType)

        => _cache.AddOrUpdateItem(String.Join("_", GlobalStrings.CacheKey_Part_Evaluator_Type_DI, evaluatorName), evaluatorType);

    
    private Type? CheckGetEvaluatorInDI(string evaluatorName)

        => _cache.TryGetItem(String.Join("_", GlobalStrings.CacheKey_Part_Evaluator_Type_DI, evaluatorName), out Type? evaluatorType) ? evaluatorType : null;

    ///<inheritdoc />
    public IConditionEvaluator GetEvaluatorByName(string evaluatorName, Type contextType)
    {
        if (true == _allowDependencyInjectionEvaluators)//skip if engine not initialised for DI
        {
            var evalutatorType = CheckGetEvaluatorInDI(evaluatorName);
            if (evalutatorType != null)
            {
                var customType = evalutatorType.IsGenericType ? evalutatorType.MakeGenericType(contextType) : evalutatorType;

                return _customTypeResolver!(customType);
            }
        }

        var cacheKey = String.Join("_", evaluatorName, contextType.FullName);

        Func<Type, IConditionEvaluator> makeEvaluator = (theContextType) =>
        {
            Type evaluatorType;

            switch (evaluatorName)
            {
                case GlobalStrings.Predicate_Condition_Evaluator: evaluatorType = typeof(PredicateConditionEvaluator<>).MakeGenericType(theContextType); break;
                case GlobalStrings.Regex_Condition_Evaluator: evaluatorType = typeof(RegexConditionEvaluator<>).MakeGenericType(theContextType); break;
                default:

                    var evaluatorTypeKey = String.Join("_", GlobalStrings.CacheKey_Part_Evaluator_Type, evaluatorName);

                    if (true == _cache.TryGetItem<Type>(evaluatorTypeKey, out var customEvaluatorType))
                    {
                        evaluatorType = customEvaluatorType!.IsGenericType && customEvaluatorType.ContainsGenericParameters == true ? customEvaluatorType.MakeGenericType(theContextType) : customEvaluatorType;
                    }
                    else
                    {
                        throw new MissingConditionEvaluatorException(String.Format(GlobalStrings.Missing_Condition_Evaluator_Exception_Message, evaluatorName));
                    }

                    break;
            }

            return (IConditionEvaluator)Activator.CreateInstance(evaluatorType)!;
            
        };

        return _cache.GetOrAddItem(cacheKey, contextType, makeEvaluator);
    }

    #endregion

    #region Eventing

    ///<inheritdoc />
    public async Task EventPublisher<TEvent>(TEvent eventToPublish, CancellationToken cancellationToken, PublishMethod publishMethod = PublishMethod.FireAndForget) where TEvent : IEvent
    {
        /*
             * If this method is called via the delegate void EventPublisher(Event eventToPublish), which in 99% cases  
             * TEvent is always IEvent so we need to call the publish method via reflection using its actual type that is registered/subscribed to
             * Suggestions welcome on how to solve this type in a better way?
         */
        try
        {
            if (false == typeof(TEvent).Equals(eventToPublish.GetType()))
            {
                Type actualType = eventToPublish.GetType();

                MethodInfo openMethod = _eventAggregator.GetType().GetMethod("Publish")!;
                MethodInfo closedMethod = openMethod.MakeGenericMethod(actualType);

                await (Task)closedMethod.Invoke(_eventAggregator, new object[] { eventToPublish, cancellationToken, publishMethod })!;

                return;
            }

            await _eventAggregator.Publish<TEvent>(eventToPublish, cancellationToken, publishMethod).ConfigureAwait(false);
        }
        catch { }
    }

    ///<inheritdoc />
    public EventSubscription SubscribeToEvent<TEvent>(HandleEvent<TEvent> eventHandler) where TEvent : IEvent

        => _eventAggregator.Subscribe<TEvent>(eventHandler);

    #endregion
}
