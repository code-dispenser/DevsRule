using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Seeds;
using DevsRule.Demo.BasicConsole.Common.Events;
using DevsRule.Demo.BasicConsole.Common.Models;
using DevsRule.Demo.BasicConsole.Common.StaticData;

namespace DevsRule.Demo.BasicConsole.Scenarios;

public class DynamicEventHandlers
{
    private readonly ConditionEngine _conditionEngine;
    public DynamicEventHandlers(ConditionEngine conditionEngine)

        => _conditionEngine = conditionEngine;

    public async Task RuleWithEventDynaicallyHandledUsingWaitForAll()
    {
        var theRule = RuleBuilder.WithName("RuleWithEventForDynamicHandlerWaitForAll")
                          .ForConditionSetNamed("DiscountIf")
                              .WithPredicateCondition<Customer>("IsStudent", c => c.CustomerType.ToString() == "Student", "Customer @{CustomerName} is not a student",
                                EventDetails.Create<AnotherDiscountRuleConditionEvent>(EventWhenType.OnSuccessOrFailure, PublishMethod.WaitForAll))
                              .WithoutFailureValue()
                              .CreateRule();
        /*
            * 
        */

        _conditionEngine.AddOrUpdateRule(theRule);

        var contexts = RuleDataBuilder.AddForAny(DataStore.GetCustomer(1)!).Create();

        var theResult = await _conditionEngine.EvaluateRule(theRule.RuleName, contexts);

        Console.WriteLine($"The rule: {theResult.RuleName} evaluated to: {theResult.IsSuccess}, taking {theResult.RuleTimeMilliseconds}ms to complete with event along with raising the event");
    }
    public async Task RuleWithEventDynaicallyHandledUsingFireAndFoget()
    {
        var theRule = RuleBuilder.WithName("RuleWithEventForDynamicHandlerFireAndForget")
                  .ForConditionSetNamed("DiscountIf")
                      .WithPredicateCondition<Customer>("IsStudent", c => c.CustomerType.ToString() == "Student", "Customer @{CustomerName} is not a student",
                        EventDetails.Create<AnotherDiscountRuleConditionEvent>(EventWhenType.OnSuccessOrFailure,PublishMethod.FireAndForget))
                      .WithoutFailureValue()
                      .CreateRule();
        /*
            * 
        */ 

        _conditionEngine.AddOrUpdateRule(theRule);

        var contexts = RuleDataBuilder.AddForAny(DataStore.GetCustomer(1)!).Create();

        var theResult = await _conditionEngine.EvaluateRule(theRule.RuleName, contexts);
        /*
            * For demo as this method will most likely have exited before the event gets to the handler
            * to negate its message being writen next to another scenarios output I have added a delay.
            * In normal use this would not be nescessary
        */
        await Task.Delay(20);

        Console.WriteLine($"The rule: {theResult.RuleName} evaluated to: {theResult.IsSuccess}, taking {theResult.RuleTimeMilliseconds}ms to complete with event along with raising the event");
    }
}
