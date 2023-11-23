[![.NET](https://github.com/code-dispenser/DevsRule/actions/workflows/buildandtest.yml/badge.svg)](https://github.com/code-dispenser/DevsRule/actions/workflows/buildandtest.yml) [![Coverage Status](https://coveralls.io/repos/github/code-dispenser/DevsRule/badge.svg?branch=main)](https://coveralls.io/github/code-dispenser/DevsRule?branch=main) [![Nuget download][download-image]][download-url]

[download-image]: https://img.shields.io/nuget/dt/DevsRule.Core
[download-url]: https://www.nuget.org/packages/DevsRule.Core
<h1>
<img src="https://raw.github.com/code-dispenser/DevsRule/main/Assets/icon-64.png" align="center" alt="Devs Rule icon" /> Devs' Rule
</h1>
<!--
# ![icon](https://raw.github.com/code-dispenser/DevsRule/main/Assets/icon-64.png) Devs Rule
-->
<!-- H1 for git hub, but for nuget the markdown is fine as it centers the image, uncomment as appropriate and do the same at the bottom of this file for the icon author -->

## Overview
Devs' Rule at its core is a lamdba predicate condition engine that allows you to create rules comprised of condition sets, with each condition set containing one or more conditions.

Rules are created in code but can be exported (or manually created) as JSON files which can both be ingested by the condition engine. This provides flexibility by allowing rule changes without 
the need to redeploy the application. Simply supply the condition engine periodically with any rules changes, that can be fetched at runtime from a file store or a database, for example.

Conditions within a condition set use short-circuiting 'And' logic between conditions, while condition sets short-circuit with 'Or' logic between each set. You do not add operators between 
your declared conditions or condition sets within a rule, internally, this is done by convention.

Each condition is evaluated using an evaluator. The condition engine contains a lambda predicate condition evaluator and a regular expression condition evaluator. You can add your own custom 
evaluators to enhance both lambda predicate conditions, custom predicate conditions and non-lambda conditions. Custom evaluators can use dependency injection via your chosen IOC container.
In essense a custom condition could be anything which is then evaluated with the appropriate evaluator, providing a true or false evaluation result to be passed back up the evaluation chain.

await ConditionEngine.EvaluateRule -> await Rule.Evaluate -> (for each) await ConditionSet.EvaulateConditions -> (for each) await Condition.EvaluateWith(evaluator, data, cancellationToken) 

Each condition within a rule and the rule itself can have an associated event. These events, depending on configuration, are raised on success, on failure, or for both success and failure. 
Events can be subscribed to for objects such as forms and view models that have local event handlers. There's also the ability to register dynamic event handlers. Dynamic event handlers are 
registered with your chosen IOC container and get instantiated with any injected dependencies every time their associated event is published via the condition engine.

Once a rule is evaluated, a RuleResult is returned containing a boolean indicating success or failure, along with any associated success or failure output value. The RuleResult contains the full 
evaluation path, showing information regarding each condition evaluation, timings, failure messages, exceptions, and the input data used, etc.

The libray also contains a couple of extension methods for the RuleResult so you can chain rules together and/or take action dependent on the result.

## Installation

Download and install the latest version of the [DevsRule.Core](https://www.nuget.org/packages/DevsRule.Core) package from [nuget.org](https://www.nuget.org/) using your preferred client tool.

## Example usage

At its simplest, it's a matter of creating an instance of the ConditionEngine, adding a rule, and then having that rule evaluated with your instance data. Rules can be built bottom up by 
creating and adding conditions to condition sets and then adding the conditions sets to a rule, or top down via the RuleBuilder. A Rule can have a failure value (default) for failing conditions 
with its overall success value being assigned from the passing condition set. 

For the majority of applications it is envisaged that the ConditionEngine will be added to an IOC container as a Singleton/SingleInstance and injected into the required areas of the application,
however, there is no technical reason preventing you from having mulitple isolated instances of the ConditionEnginge, each maintaining its own set of cached rules and evaluators if that better 
meets your requirements.

```
var conditionEngine = new ConditionEngine();

var storeCardRule = RuleBuilder.WithName("StoreCardApplicationRule")
                                    .ForConditionSetNamed("AllRequirements","Approved")
                                        .WithPredicateCondition<StoreCardApplication>("ApplicationCondition", s => s.Age >= 18 && s.CountryOfResidence == "United Kingdom" && s.TotalOrders > 5,
                                                            "You must be over 18, living in the United Kingdom and have made at least 5 orders to be eligible")
                                    .WithFailureValue("Declined")
                                    .CreateRule();

conditionEngine.AddOrUpdateRule(storeCardRule);

var applicantData  = DataStore.GetApplicantData();
var ruleResult     = await conditionEngine.EvaluateRule(storeCardRule.RuleName, RuleDataBuilder.AddForAny(applicantData).Create());

/*
    * You could also have ingested the json from a file/string. The IngestRuleFromJson method internally
    * converts the json string to a rule object and then calls the AddOrUpdateRule method.
*/

var jsonString = storeCardRule.ToJsonString();

conditionEngine.IngestRuleFromJson(jsonString);

var ruleResult = await conditionEngine.EvaluateRule("StoreCardApplicationRule", RuleDataBuilder.AddForAny(applicantData).Create());
```
Valid JSON for the above example, omitting unused properties such as those used for events would be:
```
{
  "RuleName": "StoreCardApplicationRule",
  "FailureValue: "Declined",
  "IsEnabled": true,
  "ConditionSets": [
    {
      "ConditionSetName": "AllRequirements",
      "SetValue" : "Approved",
      "Conditions": [
        {
            "ConditionName": "ApplicationCondition",
            "ContextTypeName": "MyApplicationName.Common.Models.StoreCardApplication",
            "ToEvaluate": "s => s.Age == 18 && s.CountryOfResidence == \"United Kingdom\" && s.TotalOrders == 5",
            "FailureMessage": "You must be over 18, living in the United Kingdom and have made at least 5 orders to be eligible",
            "EvaluatorTypeName": "PredicateConditionEvaluator",
            "IsLambdaPredicate": true,
        }
      ]
    }
  ]
}
```
**Note:** In the above JSON, the ContextTypeName refers to the data type used as input. This could be simplified from the type's full name "MyApplicationName.Common.Models.StoreCardApplication" 
to just "StoreCardApplication" if it's known that the StoreCardApplication object name is unique within the domain and/or any type names in referenced assemblies.

The Rule.ToJsonString method outputs the full JSON inluding all propertiess with defaults values, empty or nulls that the engine accepts (not shown above). 

In conjunction with the [documentation](https://github.com/code-dispenser/DevsRule/wiki), it is recommended that you download the source code from the [Git repository](https://github.com/code-dispenser/DevsRule) and explore the scenarios within the demo project. These sample 
scenarios and their comments should answer most of your questions.

Any feedback, positive or negative, is welcome, especially surrounding scenarios/usage.

## Acknowledgments

Currently, this library uses the method "DynamicExpressionParser.ParseLambda" from the [System.Linq.Dynamic.Core project](https://www.nuget.org/packages/System.Linq.Dynamic.Core) to create the 
compiled lambda predicate from the string represenation in any json rule files used. Many thanks to all of the contributors on that project for making it much easier for me to create this 
project/nuget package.

In the demo project included in the repo, one of the Custom Condition Evaluators calls a web api to get some dummy data. When looking for an api I could use for this purpose I came across 
[DummyJSON](https://dummyjson.com/) a free service that did not require me to create an account and get an api key which was a welcomed surprise, so many thanks to the contributors on that 
project, [DummyJSON project](https://github.com/Ovi/DummyJSON)

<img src="https://raw.githubusercontent.com/code-dispenser/DevsRule/main/Assets/icon-64.png" align="middle" height="32px" alt="Devs Rule icon" /> Thanks also to Peerapak Takpho the icon creator, which I found on [freepik.com](https://www.freepik.com/icon/setting_7012934).

<!--
![icon](https://raw.github.com/code-dispenser/DevsRule/main/Assets/icon-32.png) Thanks also to Peerapak Takpho the icon creator, which I found on [freepik.com](https://www.freepik.com/icon/setting_7012934).
-->


