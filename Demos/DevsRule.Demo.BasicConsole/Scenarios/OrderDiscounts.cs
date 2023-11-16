using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Extensions;
using DevsRule.Core.Common.Seeds;
using DevsRule.Demo.BasicConsole.Common.Models;
using DevsRule.Demo.BasicConsole.Common.Seeds;
using DevsRule.Demo.BasicConsole.Common.StaticData;
using DevsRule.Demo.BasicConsole.Common.Utilities;

namespace DevsRule.Demo.BasicConsole.Scenarios;

public class OrderDiscounts
{
    private readonly ConditionEngine _conditionEngine;
    public OrderDiscounts(ConditionEngine conditionEngine)

        => _conditionEngine = conditionEngine;
    
    public async Task CustomerDiscountMultipleConditionSets()
    {
        /*
            * The json file referenced below was created using the Rule.ToJsonString method setting the useEscaped flag to false. I would generally leave the default which is true 
            * but it shows an unescaped file compared to the others that used escaping.
            * await ConsoleGeneralUtils.WriteToJsonFile(CreateDiscountRulesUsingTheRuleBuilder(), Path.Combine(ConsoleGlobalStrings.Json_Rules_Folder_Path, "BaseCustomerDiscountRateRule.json"), true, false);
            * 
            * ConditionSets short circuit using Or logic
        */

        var discountRuleJsonString = await ConsoleGeneralUtils.ReadJsonRuleFile(Path.Combine(ConsoleGlobalStrings.Json_Rules_Folder_Path, "BaseCustomerDiscountRateRule.json"));
        
        _conditionEngine.IngestRuleFromJson(discountRuleJsonString);

        List<Customer> customers = DataStore.Customers;
        /*
            * Each condition will use the same customer context/instance passed in
            * Each condition will ask the condition engine for an evaluator which the engine will get from cache.
            * If the evaluator is not in cache it will use reflection to create an instance of the evaluator for the context, cache and return it.
        */
        var ruleResults = (await Task.WhenAll(customers.Select(customer => _conditionEngine.EvaluateRule("BaseCustomerDiscountRateRule", RuleDataBuilder.AddForAny(customer).Create())))).ToList();

        ruleResults.ForEach(result =>
        {
              result.OnSuccess(r =>
              {
                  Console.WriteLine($"Details for {((Customer)r.EvaluationChain!.EvaluationData!).CustomerName}");
                  Console.WriteLine($"Success/discount rate: {r.SuccessValue}");
                  Console.WriteLine($"\tNo. evaluations: {r.TotalEvaluations}, processing time for rule: {r.RuleTimeMilliseconds}ms - {r.RuleTimeMicroseconds} microseconds\r\n");
              })
             .OnFailure(r =>
             {
                 Console.WriteLine($"Failure/discount rate: {r.FailureValue} due to:");
                 r.FailureMessages.ForEach(f => Console.WriteLine($"\t{f}"));
                 Console.WriteLine($"\tNo. evaluations: {r.TotalEvaluations}, processing time for rule: {r.RuleTimeMilliseconds}ms - {r.RuleTimeMicroseconds} microseconds\r\n");
             });
        });
    }

    private Rule CreateDiscountRulesUsingTheRuleBuilder()
        /*
            * You can use the syntax @{PROPERTYNAME} to have parts of the failure message replaced by values from the context. The example below
            * uses a context of Customer. 
            * If the property is an object with properties then just provide the path i.e if the Customer had an Address property and the Address has a Town property
            * that you wanted to use, you would use the following @{Address.Town}, Do Not include the Customer in the path.
        */

        /*
            * Currently if you want to use Enums in the condition you will need to use ToString() and match against the Text of the Enum as shown below
            * The Enum used is in the file  BasicConsole > Common > Models > All.cs;
        */

        => RuleBuilder.WithName("BaseCustomerDiscountRateRule")
                            .ForConditionSetNamed("StudentRate", "0.10")
                                .WithPredicateCondition<Customer>("IsStudent", c => c.CustomerType.ToString() == "Student", "Customer @{CustomerName} is not a student")
                            .OrConditionSetNamed("PensionerRate", "0.15")
                                .WithPredicateCondition<Customer>("IsPensioner", c => c.CustomerType.ToString() == "Pensioner", "Customer @{CustomerName} is not a pensioner")
                            .OrConditionSetNamed("SubscriberRate", "0.20")
                                .WithPredicateCondition<Customer>("IsSubscriber", c => c.CustomerType.ToString() == "Subscriber", "Customer @{CustomerName} is not a paid subscriber")
                            .WithFailureValue("0.00")
                            .CreateRule();


}
