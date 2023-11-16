using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Seeds;
using DevsRule.Demo.BasicConsole.Common.Events;
using DevsRule.Demo.BasicConsole.Common.Models;
using DevsRule.Demo.BasicConsole.Common.Seeds;
using DevsRule.Demo.BasicConsole.Common.StaticData;
using DevsRule.Demo.BasicConsole.Common.Utilities;

namespace DevsRule.Demo.BasicConsole.Scenarios;

public class SubscribeToEvents
{
    /*
        * In your classes, forms, viewmodels etc you can subscribe to recieve events from event enabled conditions
        * there is no unsubscribe method, you just use the dispose on the event subscription. One subscription per event registration.
        * weakreferences are used so if dispose is not called, once the subscription goes out of scope the handler will get removed from the invocation list. 
        * 
        * All condition events must implement ConditionEventBase and rule events implementing RuleEventBase. Each derived event class is really just a marker in order to have a specific event type for a specific conditions as
        * you will see just by looking at the example DiscountRuleConditionEvent class (in the Common\Events folder).
        * 
        * Fire and Forget is just that, the event is raised and the code just move on, no exceptions are bubbled up, they are just squashed if you did not include any error handling in the event handlers.
        * WaitForAll under the covers uses Task.WhenAll. At the condition level this means the condition is first evaluated, the the event is then raised, with the code waiting for all event handlers to finish before processing the next condition.
        * At the rule level, the rule result is gained, the event is raised, the code will wait for all rule event handlers to finish before returning the rule result.
        * 
        * If WaitForAll is used the RuleResult timings show how long everything took to completed i.e all the handlers for events that did not use FireAndForget.
        * The CancellationToken (if used) for the Evaluate method gets propgated to all condition evaluators and event handlers.
    */

    private readonly ConditionEngine _conditionEngine;
    public SubscribeToEvents(ConditionEngine conditionEngine) => _conditionEngine = conditionEngine;
   
    public async Task RunRuleAndHandleSubscribedForEventsThatAreFireAndForget()
    {
        /*
             * Rule used to write the json file 
        */
        //var theRule = RuleBuilder.WithName("RuleWithEventFireAndForget")
        //                  .ForConditionSetNamed("DiscountIf")
        //                      .WithPredicateCondition<Customer>("IsStudent", c => c.CustomerType.ToString() == "Student", "Customer @{CustomerName} is not a student",
        //                        EventDetails.Create<DiscountRuleConditionEvent>(EventWhenType.OnSuccessOrFailure))
        //                      .WithoutFailureValue()
        //                      .CreateRule();

        //await ConsoleGeneralUtils.WriteToJsonFile(theRule, Path.Combine(ConsoleGlobalStrings.Json_Rules_Folder_Path, "RuleWithEventFireAndForget.json"), true, false);

        var jsonString = await ConsoleGeneralUtils.ReadJsonRuleFile(Path.Combine(ConsoleGlobalStrings.Json_Rules_Folder_Path, "RuleWithEventFireAndForget.json"));

        _conditionEngine.IngestRuleFromJson(jsonString);

        EventSubscription eventSubscription = _conditionEngine.SubscribeToEvent<DiscountRuleConditionEvent>(HandleDiscountRuleEvent);//subscribe at any time before or after rule is created/added to engine
        /*
            * Events, by default unless the PublishMethod is specified in the EventDetails uses a fire and forgot task method which just ignores exceptions that may occur in any event handlers.
        */
        var contexts = RuleDataBuilder.AddForAny(DataStore.GetCustomer(1)!).Create();

        var theResult = await _conditionEngine.EvaluateRule("RuleWithEventFireAndForget", contexts);

         /*
            * For the demo I need to add a delay as this method will exit before the event gets to the handler due to the fire and forget
            * and the message written, will be writted to some obscure place. In normal use this is not necessary.
        */
        await Task.Delay(100);

        Console.WriteLine($"The rule: {theResult.RuleName} evaluated to: {theResult.IsSuccess}, taking {theResult.RuleTimeMilliseconds}ms to complete including raising any events");

        eventSubscription?.Dispose();
    }
    private async Task HandleDiscountRuleEvent(DiscountRuleConditionEvent theEvent, CancellationToken cancellationToken)
    {
        await Console.Out.WriteLineAsync($"Handled the registered event, sent by the condition: {theEvent.SenderName} using FireAndForget");

        theEvent.TryGetData(out var contextData);
        await Console.Out.WriteLineAsync(contextData is null ? $"The context data is null due to {theEvent.SerializationException}" : $"Casting data: {(contextData as Customer)}");
    }
    public async Task RunRuleAndHandleSubscribedForEventsThatAreWaitForAll_1_Second_Delay_In_Handler()
    {
        var theRule = RuleBuilder.WithName("RuleWithEventWaitForAll")
                          .ForConditionSetNamed("DiscountIf")
                              .WithPredicateCondition<Customer>("IsStudent", c => c.CustomerType.ToString() == "Student", "Customer @{CustomerName} is not a student",
                                EventDetails.Create<DiscountRuleConditionEvent>(EventWhenType.OnSuccessOrFailure,PublishMethod.WaitForAll))
                              .WithoutFailureValue()
                              .CreateRule();

        EventSubscription eventSubscription = _conditionEngine.SubscribeToEvent<DiscountRuleConditionEvent>(HandleDiscountRuleConditionEventWaitAll);//subscribe at any time before or after rule is created/added to engine
        /*
            * Events published using WaitForAll, as the name indicates will await for all handlers to finish before proceeding to evaluate the next condition in the rule.
            * This rule only has one condition so it will be handled before the rule result is produced
        */
        _conditionEngine.AddOrUpdateRule(theRule);
        var contexts = RuleDataBuilder.AddForAny(DataStore.GetCustomer(1)!).Create();

        var theResult = await _conditionEngine.EvaluateRule(theRule.RuleName, contexts);

        Console.WriteLine($"The rule: {theResult.RuleName} evaluated to: {theResult.IsSuccess}, taking {theResult.RuleTimeMilliseconds}ms to complete inluding waiting (Task.WhenAll) for any event handlers to finish for the condition");

        eventSubscription?.Dispose();

        
    }


    private async Task HandleDiscountRuleConditionEventWaitAll(DiscountRuleConditionEvent theEvent, CancellationToken cancellationToken)
    {
        await Task.Delay(500);
        await Console.Out.WriteLineAsync($"Handled the registered event, sent by the condition: {theEvent.SenderName} uisng WaitForAll (added a 0.5 sec delay in the handler)");

        theEvent.TryGetData(out var contextData);
        Console.WriteLine(contextData is null ? $"The context data is null due to {theEvent.SerializationException}" : $"Casting data: {(contextData as Customer)}");


    }




}
