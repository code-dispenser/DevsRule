using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Exceptions;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;

namespace DevsRule.Core.Areas.Engine
{
    /*
        * I added this interface mainly for the xml comments as to not obstruct the code in the ConditionEngine class.
        * Currently the xml comments is its only purpose
    */

    /// <summary>
    /// The condition engine is responsible for converting json into rules, storing and fetching rules from an internal cache, starting a rule evaluation 
    /// process and for the creation of the the condition evaluators. 
    /// With regards to custom evaluators and dynamic event handlers, the engine stores registration details for these, and requests them from the IOC 
    /// container when needed. Dynamic event handlers are created by the IOC container when the condition engine publishes their associated events.
    /// </summary>
    public interface IConditionEngine
    {

        /// <summary>
        /// Adds a rule to an internal cache using the combination of rule name, tenantID and cultureID as the cache key. If the rule does not exist in the 
        /// cache it is added otherwise the existing entry is replaced.
        /// </summary>
        /// <param name="rule">The rule to be added to or updated in the cache.</param>
        /// <param name="tenantID">The ID of the tenant for multitenant applications, a rule could be for a specific tenant.</param>
        /// <param name="cultureID">
        /// A rule could have text in a specific language, this allows for that selection. Nb. This does not affect an thread culture.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        void AddOrUpdateRule(Rule rule, string tenantID = "All_Tenants", string cultureID = "en-GB");


        /// <summary>
        /// Checks whether or not a rule is in the condition engines cache. 
        /// The cache key used is the combination of rule name, tenantID and cultureID.
        /// </summary>
        /// <param name="ruleName">The name of the rule.</param>
        /// <param name="tenantID">The specific tenantID if used, otherwise the default value is used.</param>
        /// <param name="cultureID">The specific cultureID if used, otherwise use default value is used.</param>
        /// <returns>true if the rule is in the condition engines cache otherwise false.</returns>
        bool ContainsRule(string ruleName, string tenantID = "All_Tenants", string cultureID = "en-GB");


        /// <summary>
        /// Removes the rule from the conditions engines cache.
        /// </summary>
        /// <param name="ruleName">The name of the rule.</param>
        /// <param name="tenantID">The specific tenantID if used, otherwise use default value is used.</param>
        /// <param name="cultureID">The specific cultureID if used, otherwise use default value is used.</param>
        void RemoveRule(string ruleName, string tenantID = "All_Tenants", string cultureID = "en-GB");

        /// <summary>
        /// Tries to get the rule from the condition engines cache.
        /// </summary>
        /// <param name="ruleName">The name of the rule.</param>
        /// <param name="rule">The rule to be returned in the out parameter if available.</param>
        /// <param name="tenantID">The specific tenantID if used, otherwise use default value is used.</param>
        /// <param name="cultureID">The specific cultureID if used, otherwise use default value is used.</param>
        /// <returns>true if the condition engine contains the rule otherwise false.</returns>
        bool TryGetRule(string ruleName, out Rule? rule, string tenantID = "All_Tenants", string cultureID = "en-GB");


        /// <summary>
        /// Gets the rule from cache and then starts the evaluation process using the provided data contexts.
        /// </summary>
        /// <param name="ruleName">The name of the rule to evaluate.</param>
        /// <param name="contexts">Contains the array of DataContexts for all conditions within a rule.</param>
        /// <exception cref="RuleNotFoundException">Thrown when the <paramref name="ruleName"/> is not found in the cache.</exception>
        /// <exception cref="MissingRuleContextsException">Thrown when the <paramref name="contexts"/> is null or an empty array.</exception>
        /// <exception cref="MissingConditionSetsException">Thrown when a rule has no condition sets.</exception>
        /// <exception cref="MissingConditionsException">Thrown when a condition set contains no conditions</exception>
        /// <returns>A RuleResult containing all of the information about the evaluation path, failure messages, exceptions, timings and the overall 
        /// outcome and return value if specified by the rule.
        /// </returns>
        Task<RuleResult> EvaluateRule(string ruleName, RuleData contexts);

        /// <summary>
        /// Gets the rule from cache and then starts the evaluation process using the data provided. The evaluation process can be cancelled by way of the
        /// provided cancellation token.
        /// </summary>
        /// <param name="ruleName">The name of the rule to evaluate, with its data contexts.</param>
        /// <param name="contexts">Contains the array of DataContexts for all conditions within a rule</param>
        /// <param name="cancellationToken">The cancellation token used to signify any cancellation requests, passed to all evaluators and event handlers 
        /// within scope of the respective rule.
        /// </param>
        /// <exception cref="RuleNotFoundException">Thrown when the <paramref name="ruleName"/> is not found in the cache.</exception>
        /// <exception cref="MissingRuleContextsException">Thrown when the <paramref name="contexts"/> is null or an empty array.</exception>
        /// <exception cref="MissingConditionSetsException">Thrown when a rule has no condition sets.</exception>
        /// <exception cref="MissingConditionsException">Thrown when a condition set contains no conditions</exception>
        /// <returns>A RuleResult containing all of the information about the evaluation path, failure messages, exceptions, timings and the overall 
        /// outcome and return value if specified by the rule.
        /// </returns>
        Task<RuleResult> EvaluateRule(string ruleName, RuleData contexts, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the rule from cache and then evaluates it using the data provided, passing along a cancellationToken. The cache lookup uses the a cache key which
        /// is the combination of ruleName, tenantID and cultureID.
        /// </summary>
        /// <param name="ruleName">The name of the rule to evaluate, with its data contexts.</param>
        /// <param name="contexts">Contains the array of DataContexts for all conditions within a rule</param>
        /// <param name="cancellationToken">The cancellation token used to signify any cancellation requests, passed to all evaluators and event handlers within scope of 
        /// the respective rule.
        /// </param>
        /// <param name="tenantID">The ID of the tenant in multitenant applications (default is "All_Tenants" for when no tenantID is specified or when the rule is good for all tenants).</param>
        /// <param name="cultureID">The ID of the culture used by the rule, to signify the language used in text messages (default is "en-GB")
        /// This does not affect any Thread cultures, its merely an ID to enable rules in with specific languages to be stored using the three part key - ruleName, tenantID and cultureID
        /// </param>
        /// <exception cref="RuleNotFoundException">Thrown when the <paramref name="ruleName"/> is not found in the cache.</exception>
        /// <exception cref="MissingRuleContextsException">Thrown when the <paramref name="contexts"/> is null or an empty array.</exception>
        /// <exception cref="MissingConditionSetsException">Thrown when a rule has no condition sets.</exception>
        /// <exception cref="MissingConditionsException">Thrown when a condition set contains no conditions</exception>
        /// <returns>A RuleResult containing all of the information about the evaluation path, failure messages, exceptions, timings and the overall 
        /// outcome and return value if specified by the rule.
        /// </returns>
        Task<RuleResult> EvaluateRule(string ruleName, RuleData contexts, CancellationToken cancellationToken, string tenantID = "All_Tenants", string cultureID = "en-GB");



        /// <summary>
        /// Gets the condition evaluator from cache or from an IOC container that is required in order for the rule condition
        /// to be evaluated.
        /// </summary>
        /// <param name="evaluatorName">The name of the evaluator</param>
        /// <param name="contextType">The data type expected by the condition</param>
        /// <exception cref="MissingConditionEvaluatorException">Thrown when an evaluator could not be found or created.</exception>
        /// <returns>an object that implements IConditionEvaluator used for evaluating conditions.</returns>
        IConditionEvaluator GetEvaluatorByName(string evaluatorName, Type contextType);


        /// <summary>
        /// Ingests a json formatted string representing a rule. This json is converted into a Rule and then cached using the AddOrUpdate method.
        /// </summary>
        /// <param name="ruleJson">The json formatted string containing the rule information.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ruleJson"/> the string is null, empty or just whitespace.</exception>
        /// <exception cref="MissingConditionToEvaluatePropertyValue">Thrown when the ToEvaluate property is null or missing.</exception>
        /// <exception cref="ContextTypeAssemblyNotFound">Thrown when the context data type for any condition is not found in your local assembly types.</exception>
        /// <exception cref="RuleFromJsonException">Thrown for other exceptions that may occur with more details within the inner exception property.</exception>
        void IngestRuleFromJson(string ruleJson);

        /// <summary>
        /// Converts a json formatted string containing rule information into a rule. This method does not add a rule to cache but rather returns it.
        /// This method is useful for test/ensuring that json strings can be ingested.
        /// </summary>
        /// <param name="ruleJson">The json formatted string containing the rule information.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ruleJson"/> the string is null, empty or just whitespace.</exception>
        /// <exception cref="MissingConditionToEvaluatePropertyValue">Thrown when the ToEvaluate property is null or missing.</exception>
        /// <exception cref="ContextTypeAssemblyNotFound">Thrown when the context data type for any condition is not found in your local assembly types.</exception>
        /// <exception cref="RuleFromJsonException">Thrown for other exceptions that may occur with more details within the inner exception property.</exception>
        Rule RuleFromJson(string ruleJson);


        /// <summary>
        /// Registers a custom evaluator that does not require IOC container services with the condition engine.
        /// Open generic types should be registered using typeof(ExampleEvaluator&lt;&gt;).
        /// </summary>
        /// <param name="evaluatorName">The name of the condition evaluator to register with the condition engine.</param>
        /// <param name="evaluatorType">The type of evaluator i.e "typeof(ExampleEvaluator&lt;&gt;)" or typeof(ExampleEvaluator)</param>
        void RegisterCustomEvaluator(string evaluatorName, Type evaluatorType);



        /// <summary>
        /// Registers a custom evaluator with the condition engine that will be fetched from your IOC container. 
        /// The custom evaluator will need to be added to your IOC container before use.
        /// </summary>
        /// <param name="evaluatorName">The name of the condition evaluator to register with the condition engine.</param>
        /// <param name="evaluatorType">The type of evaluator i.e "typeof(ExampleEvaluator&lt;&gt;)" or typeof(ExampleEvaluator)</param>
        void RegisterCustomEvaluatorForDependencyInjection(string evaluatorName, Type evaluatorType);


        /// <summary>
        /// Publishes a rule or condition event to be handled by local or dynamic event handlers. This method is called by each event enabled condition following their
        /// evaluation. Once a rule result is known but before the RuleResult is returned, if enabled, the rule will publish its associated event. 
        /// </summary>
        /// <typeparam name="TEvent">The type of event i.e typeof(MyEvent).</typeparam>
        /// <param name="eventToPublish">The event object that needs publishing.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <param name="publishMethod">An enumeration value representing how the event is published, or more importantly if the handlers are awaited, the default is FireAndForget</param>
        /// <remarks>Events are asynchronous, the FireAndForgot value does not block and will return immediately. 
        /// The WaitForAll values causes the code to use await Task.WhenAll which will wait for all handlers to return before proceeding onto the next condition evaluation or rule event.
        /// All unhandled exceptions are squashed in order to continue with the rule processing. Ensure you have the appropriate exception handling in your event handlers. 
        /// </remarks>
        /// <returns>an await able Task.</returns>
        Task EventPublisher<TEvent>(TEvent eventToPublish, CancellationToken cancellationToken, PublishMethod publishMethod = PublishMethod.FireAndForget) where TEvent : IEvent;



        /// <summary>
        /// The SubscribeToEvent allows you to subscribe to events that you can handle locally via handlers created in forms and view models, for example.
        /// Events can also be handled dynamically via registration of event handler classes in an IOC container.
        /// </summary>
        /// <typeparam name="TEvent">The type of event i.e typeof(MyEvent)</typeparam>
        /// <param name="eventHandler">This is the EventHandler you have defined. This method must accept two arguments, the actual type of event and a CancellationToken.</param>
        /// <returns>an EventSubscription to be disposed when you no longer wish to receive events.</returns>
        EventSubscription SubscribeToEvent<TEvent>(HandleEvent<TEvent> eventHandler) where TEvent : IEvent;

    }
}