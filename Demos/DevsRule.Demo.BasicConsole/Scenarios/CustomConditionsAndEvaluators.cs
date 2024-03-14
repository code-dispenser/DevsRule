using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Demo.BasicConsole.Common.CustomConditionEvaluators;
using DevsRule.Demo.BasicConsole.Common.Models;
using DevsRule.Demo.BasicConsole.Common.StaticData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevsRule.Demo.BasicConsole.Scenarios;

public class CustomConditionsAndEvaluators
{
    private readonly ConditionEngine _conditionEngine;

    public CustomConditionsAndEvaluators(ConditionEngine conditionEngine)
        
        => _conditionEngine = conditionEngine;
        
    
    public async Task MixAndMaxConditions()
    {
        /*
            * There are three types of conditions that you can use, all have a ToEvaluate property.
            * 
            * PredicateCondition - this is for lambda predicates and gets a compiled lambda expression/func predicate
            * RegexCondition     - this is for regular expression conditions and gets a property path to be evaluated with a regex pattern
            * CustomCondition    - this can use either a lambda expression (compiled - IsLambdaPredicate = true) or just some string value.
            * 
            * All conditions have an AdditionalInfo property which is a Dictionary<string,string> that can be used to pass additional information to 
            * custom evaluators. The rule builder builds the dictionary for Regex and adds any RegexOptions to the dictionary.
            * If you manually create a RegexCondition you need to add a key: Pattern, with the value being the reqex pattern to the dictionary. 
            * 
            * You cannot create your own conditions (unless you fork the repo), you can only create custom evaluators which works with condition(s).
            * The scenario CustomEvaluatorsAllEventsAndDI uses a custom condition with a custom evaluator, ProbeValueConditionEvaluator, replicated below
            * in this somewhat nonsensical scenario.
        */


        var conditionsRule = RuleBuilder.WithName("ConditionsRule")
                                        .ForConditionSetNamed("MixedConditions")
                                           .WithRegexCondition<Customer>("RegexCondition", c => c.CustomerName, @"^(?!.*[\-&'' _]{2})[\w][-\w&'' ]{1,100}(?<![\-_& ])$",
                                                                        "Customer must name cannot contain double spaces, double hyphens or double dashes and can be no longer that 100 characters")

                                           .AndPredicateCondition<Address>("AddressCondition", a => a.Country == "United Kingdom", "Must be a resident of the UK")

                                           .AndCustomPredicateCondition<Probe>("ProbePredicateCondition", p => p.ProbeValue > 30, "Value should be above 30", "MyCustomGenericPredicateConditionEvaluator",
                                                                                new Dictionary<string, string> { ["KeyOne"]="ValueOne", ["KeyTwo"]="ValueTwo" })

                                           .AndCustomCondition<Probe>("ValueCondition", "CalibrationTest", "Probe outside of expected norm", "ProbeValueConditionEvaluator",
                                                                        new Dictionary<string, string> { ["MeanValue"]="50", ["MinValue"]="20", ["MaxValue"]="80" })
                                           .WithoutFailureValue()
                                           .CreateRule();


        /*
            * Register custom evaluators with the condition engine if they need dependency injection.
            * NB the probe ProbeValueConditionEvaluator uses DI and is in the IOC container see the program class 
            * ProbeValueConditionEvaluator is a closed generic i.e ProbeValueConditionEvaluator : ConditionEvaluatorBase<Probe>
            * MyCustomGenericPredicateConditionEvaluator is an open generic that does not need DI i.e MyCustomGenericPredicateConditionEvaluator<TContext> : ConditionEvaluatorBase<TContext>
            * 
        */

        _conditionEngine.RegisterCustomEvaluator("MyCustomGenericPredicateConditionEvaluator", typeof(MyCustomGenericPredicateConditionEvaluator<>));
        _conditionEngine.RegisterCustomEvaluatorForDependencyInjection("ProbeValueConditionEvaluator", typeof(ProbeValueConditionEvaluator));
        /*
            * The MyCustomGenericPredicateConditionEvaluator has not been used before and as such has not been created and added to cache
            * The ProbeValueConditionEvaluator name is in cache but is created by the IOC container using Transient/per dependency.
            * The register method uses an AddOrUpdate cache method so technically the none IOC evaluator could be loaded at runtime
            * from a folder i.e to dynamically change the way that evaluation is done.
        */

        var jsonString = conditionsRule.ToJsonString();

        _conditionEngine.IngestRuleFromJson(jsonString);

        var ruleDataContexts = RuleDataBuilder.AddForAny(DataStore.GetAddress(1)!)
                                                .AndForAny(DataStore.GetCustomer(1)!)
                                                .AndForAny(DataStore.GetTenantDevice(1).Probes[0]).Create();

        var result = await _conditionEngine.EvaluateRule("ConditionsRule", ruleDataContexts);

       Console.WriteLine($"\r\nThe rule: {result.RuleName} evaluated to {result.IsSuccess}, it ran {result.TotalEvaluations} evaluations in {result.RuleTimeMilliseconds}ms - {result.RuleTimeMicroseconds} microseconds");

        Console.WriteLine($"\r\nRunning the rule again now that the MyCustomGenericPredicateConditionEvaluator has been created and in cache\r\n");
       
       result = await _conditionEngine.EvaluateRule("ConditionsRule", ruleDataContexts);

       Console.WriteLine($"\r\nThe rule: {result.RuleName} evaluated to {result.IsSuccess}, it ran {result.TotalEvaluations} evaluations in {result.RuleTimeMilliseconds}ms - {result.RuleTimeMicroseconds} microseconds");
    }
}
