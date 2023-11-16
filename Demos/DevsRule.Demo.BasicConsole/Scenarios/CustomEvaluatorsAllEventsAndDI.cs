using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Extensions;
using DevsRule.Core.Common.Seeds;
using DevsRule.Demo.BasicConsole.Common.CustomConditionEvaluators;
using DevsRule.Demo.BasicConsole.Common.Events;
using DevsRule.Demo.BasicConsole.Common.Models;
using DevsRule.Demo.BasicConsole.Common.StaticData;
using DevsRule.Demo.BasicConsole.Common.Utilities;

namespace DevsRule.Demo.BasicConsole.Scenarios;

public class CustomEvaluatorsAllEventsAndDI
{
    private readonly ConditionEngine _conditionEngine;
    public CustomEvaluatorsAllEventsAndDI(ConditionEngine conditionEngine)

        => _conditionEngine = conditionEngine;

    public async Task MixAndMatch()
    { /*
        * Using WaitForALL (which uses Task.WhenAll) only to keep the output messages in order
      */ 
        var deviceProbeRule = RuleBuilder.WithName("DeviceHealthRule", EventDetails.Create<DeviceRuleEvent>(EventWhenType.OnSuccessOrFailure, PublishMethod.WaitForAll))
                                    .ForConditionSetNamed("ProbeHealth")
                                        .WithPredicateCondition<Probe>("ProbeCondition", d => d.ResponseTimeMs < 10 && d.ErrorCount < 3, "Probe ID: @{ProbeID} is starting to fail")

                                        .AndCustomCondition<Probe>("ValueCondition", "CalibrationTest", "Probe outside of expected norm", "ProbeValueConditionEvaluator",
                                                                        new Dictionary<string, string> { ["MeanValue"]="50", ["MinValue"]="20", ["MaxValue"]="80" })

                                        .AndPredicateCondition<Probe>("BatteryCondition", d => d.BatteryLevel >= 5, "Low battery",
                                                                        EventDetails.Create<DeviceConditionEvent>(EventWhenType.OnFailure, PublishMethod.WaitForAll))
                                        .WithoutFailureValue()
                                        .CreateRule();

        /*
            * ProbeValueConditionEvaluator is registered in the DI container in the program file, however,.you still need to tell the engine about it. 
            * As the evaluator has closed the generic with the Probe type ("ProbeValueConditionEvaluator : ConditionEvaluatorBase<Probe>"),  
            * typeof(ProbeValueConditionEvaluator) is used, NOT typeof(ProbeValueConditionEvaluator<>).
        */
        _conditionEngine.RegisterCustomEvaluatorForDependencyInjection("ProbeValueConditionEvaluator", typeof(ProbeValueConditionEvaluator));
        
        _conditionEngine.AddOrUpdateRule(deviceProbeRule);//this rule is not Tenant specific its good for any Tenant

        var tenantDevice            = DataStore.GetTenantDevice(101);//The data is for a specific Tenant;
        var primaryProbeData        = RuleDataBuilder.AddForAny(tenantDevice.Probes[0]).Create(tenantDevice.TenantID.ToString());
        var secondaryProbeData      = RuleDataBuilder.AddForAny(tenantDevice.Probes[1]).Create(tenantDevice.TenantID.ToString());

        var conditionSubscription   = _conditionEngine.SubscribeToEvent<DeviceConditionEvent>(DeviceConditionEventHandler);
        var ruleSubscription        = _conditionEngine.SubscribeToEvent<DeviceRuleEvent>(DeviceRuleEventHandler);
        /*
            * When chaining the deviceResult will be the last evaluatred result. It will however contain the previous results and their evaluation chains. 
        */ 
        var deviceResult = await _conditionEngine.EvaluateRule("DeviceHealthRule", primaryProbeData)
                                                 .OnSuccess(_ => Console.WriteLine("Primary probe is OK"))
                                                 .OnFailure(async (result) =>
                                                 {
                                                     Console.WriteLine($"Primary probe failing message: {result.FailureMessages[0] +"\r\n"}");
                                                     Console.WriteLine("Checking secondary probe\r\n");

                                                     return await _conditionEngine.EvaluateRule(deviceProbeRule.RuleName, secondaryProbeData)
                                                                        .OnSuccess(_ => Console.WriteLine("Secondary probe is OK"))
                                                                        .OnFailure(result => Console.WriteLine($"Secondary probe failure message, {result.FailureMessages[0] + "\r\n"}"));
                                                 });

        var evaluationOrder = ConsoleGeneralUtils.BuildChainString(deviceResult);

        Console.WriteLine($"\r\nScenario evaluation path:");

        evaluationOrder.ForEach(item => Console.WriteLine(item));

        conditionSubscription.Dispose();
        ruleSubscription.Dispose();
    }

    private async Task DeviceConditionEventHandler(DeviceConditionEvent deviceConditionEvent, CancellationToken cancellationToken)
    {
        _= deviceConditionEvent.TryGetData(out var data);

        await Console.Out.WriteLineAsync($"Handled the event locally for the {deviceConditionEvent.SenderName} condition, probe id {(data as Probe)?.ProbeID} owned by Tenant ID: {deviceConditionEvent.TenantID}");
    }
    private async Task DeviceRuleEventHandler(DeviceRuleEvent deviceRuleEvent, CancellationToken cancellationToken)
    {
        await Console.Out.WriteLineAsync($"Handled the event locally for rule: {deviceRuleEvent.SenderName} which evaluated to {deviceRuleEvent.IsSuccessEvent} for Tenant ID: {deviceRuleEvent.TenantID}");
    }
}
