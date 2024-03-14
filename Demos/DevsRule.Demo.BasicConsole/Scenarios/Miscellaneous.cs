
using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Extensions;
using DevsRule.Core.Common.Models;
using DevsRule.Demo.BasicConsole.Common.Models;
using DevsRule.Demo.BasicConsole.Common.Seeds;
using DevsRule.Demo.BasicConsole.Common.StaticData;

namespace DevsRule.Demo.BasicConsole.Scenarios;

public  class Miscellaneous
{
    private readonly ConditionEngine _conditionEngine;

    public Miscellaneous(ConditionEngine conditionEngine)

        => _conditionEngine = conditionEngine;

    public async Task EvaluatingRulesNotAddedToTheEngine()
    {
        /*
            * The condition engines EvaluateRule method calls the Rule Evaluate method which calls the ConditionSet EvaluateConditions method which then gets the correct evaluator
            * and passes the evaluator into the condition EvaluateWith method.
            * 
            * Dependent on your needs you can evaluate a rule without it being added to the condition engine. It is also possible to create a set of conditions (ConditionSet)
            * and evaluate those without the engine or rule.
        */

        var addressContext = RuleDataBuilder.AddForAny(DataStore.GetAddress(2)!).Create();

        var theRule = RuleBuilder.WithName("RuleEvaluate")
                                    .ForConditionSetNamed("SetOne", "In Usa").WithPredicateCondition<Address>("AddressCondition", a => a.TownCity == "Louisville", "Customer need to be residents of Louisville")
                                    .WithoutFailureValue().CreateRule();

        /*
            * We can just use the engine for the resolution of the evaluators and any event publishing. 
        */ 
        var resultForRule = await theRule.Evaluate(_conditionEngine.GetEvaluatorByName, addressContext,_conditionEngine.EventPublisher);

        Console.WriteLine($"Using the Rule.Evaluate method: RuleResult.IsSuccess = {resultForRule.IsSuccess} - Result.SuccessValue = {resultForRule.SuccessValue}");

        var conditionSet = new ConditionSet("SetAndConditions", new PredicateCondition<Address>("AddressCondition", a => a.TownCity == "Louisville", "Customer need to be residents of Louisville"), "In USA");

        var conditionResult = await conditionSet.EvaluateConditions(_conditionEngine.GetEvaluatorByName, addressContext,_conditionEngine.EventPublisher,CancellationToken.None);

        Console.WriteLine($"Using the ConditionSet.EvaluateConditions method: ConditionResult.IsSuccess = {conditionResult.IsSuccess} - ConditionResult.SetValue = {conditionResult.SetValue}");


    }

    public async Task UseTheEngineToTestYourJsonRuleSyntax()
    {
        /*
            * The condition engine IngestRuleFromJson method uses the method RuleFromJson and then adds the result to its cache.
            * The RuleFromJson is public so you could use it in a separate project to test that your json rules can be ingested. Just add a reference to the dll that contains the data context models
            * in order for any ContextTypeNames used in the json files to match those on the target system
            * Assuming the json is good with no typos or trailing commas etc then the main point of failure will be either the ContextTypes not being available or problems with the lambda expressions.
        */
        var discountRule = RuleBuilder.WithName("BaseCustomerDiscountRateRuleCopy")
                    .ForConditionSetNamed("StudentRate", "0.10")
                        .WithPredicateCondition<Customer>("IsStudent", c => c.CustomerType.ToString() == "Student", "Customer @{CustomerName} is not a student")
                    .OrConditionSetNamed("PensionerRate", "0.15")
                        .WithPredicateCondition<Customer>("IsPensioner", c => c.CustomerType.ToString() == "Pensioner", "Customer @{CustomerName} is not a pensioner")
                    .OrConditionSetNamed("SubscriberRate", "0.20")
                        .WithPredicateCondition<Customer>("IsSubscriber", c => c.CustomerType.ToString() == "Subscriber", "Customer @{CustomerName} is not a paid subscriber")
                    .WithFailureValue("0.00")
                    .CreateRule();

        var discountRuleJsonString = discountRule.ToJsonString(writeIndented: true, useEscaped: true);
        /*
            * Save this to disc instead of handcrafting and just modify when needed.
            * To test just read back from disc and pass to the engine
        */
        
        try
        {
            
            var filePath = Path.Combine(ConsoleGlobalStrings.Json_Rules_Folder_Path, "BaseCustomerDiscountRateRuleCopy.json");

            await File.WriteAllTextAsync(filePath ,discountRuleJsonString);
            /*
                * Go to the file path and alter the json 
            */

            var ruleJsonFromDisk = await File.ReadAllTextAsync(filePath);
            // or simulate
            ruleJsonFromDisk = ruleJsonFromDisk.Replace("StudentRate", null);

            /*
                * Now see if the engine accepts it without error.
                * Nothing gets added to the engine.
            */ 

            var theDiscountRule = _conditionEngine.RuleFromJson(ruleJsonFromDisk);

            /*
                * Add it to the engine and/or use the Rule.Evaluate to see if everything is ok 
                * Or ingest the ruleJsonFromDisk
            */
            var dataContexts = RuleDataBuilder.AddForAny(DataStore.GetCustomer(1)!).Create();
            _conditionEngine.AddOrUpdateRule(theDiscountRule);

            var ruleResult = await _conditionEngine
                                    .EvaluateRule(theDiscountRule.RuleName, dataContexts)
                                        .OnSuccess(s => Console.WriteLine($"Success value = {s.SuccessValue} - From ConditionSet: {s.SuccessfulSet}"))
                                            .OnFailure(f => f.FailureMessages.ForEach(f => Console.WriteLine(f)));

            /*
                * If its accepted by the engine then the last check is the ruleResult for exceptions
                * This will most likely be missing or incorrect evaluator names.
            */ 
            if (ruleResult.Exceptions.Count > 0) Console.WriteLine(ruleResult.Exceptions[0].Message);            
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Message: {ex.Message + "\r\n\r\n"} - {ex?.InnerException?.Message} - {ex?.InnerException?.InnerException?.Message}");
        }


    }

}
