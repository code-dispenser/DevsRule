using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Seeds;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Evaluators;
using DevsRule.Tests.SharedDataAndFixtures.Events;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.SharedFixtures;
using DevsRule.Tests.SharedDataAndFixtures.Utils;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace DevsRule.Core.Tests.Integration.Areas.Engine;

public class ConditionEngineTests : IClassFixture<ConditionEngineDIFixture>
{

    private readonly ConditionEngine  _conditionEngine;
    public ConditionEngineTests(ConditionEngineDIFixture conditionEngineDIFixture)
    
        => _conditionEngine = conditionEngineDIFixture.ConditionEngine;



    [Fact]
    public async Task Should_be_able_to_ingest_and_evaluate_a_rule_with_both_inbuilt_and_custom_di_evaluators_defined()
    {
        var jsonRulePath = DataHelper.GetJsonRuleFilePath("JsonRule.json");

        var jsonString = File.ReadAllText(jsonRulePath);

        _conditionEngine.IngestRuleFromJson(jsonString);

        var contexts = RuleDataBuilder.AddForCondition("Customer Name and number", StaticData.CustomerOne())
                                     .AndForAny(new Supplier("Supplier   Name",1,100.00M))
                                     .AndForCondition("Member years", StaticData.CustomerThree()).Create();

        _conditionEngine.RegisterCustomEvaluatorForDependencyInjection("CustomDIRequiredEvaluator",typeof(CustomDIRequiredEvaluator<>));

        var theResult = await _conditionEngine.EvaluateRule("Rule101", contexts);

        theResult.Should().Match<RuleResult>(r => r.IsSuccess == false && r.FailureMessages[2] == "Member years should be four but had 3");
    }
    
    
    
    [Fact]
    public async Task Should_be_able_to_update_a_rule_via_the_add_or_update_rule_method()
    {
        var theRuleClone = _conditionEngine.ContainsRule("Rule101");
        
        //changed third set to pass by correct member years value

        var theRule = RuleBuilder.WithName("Rule101")
                         .ForConditionSetNamed("SetOne", "10")
                             .WithPredicateCondition<Customer>("Customer Name and number", c => c.CustomerName == "CustomerOne" && c.CustomerNo == 2, "Customer name or number is not correct")
                         .OrConditionSetNamed("SetTwo", "20")
                             .WithRegexCondition<Supplier>("Supllier Name", s => s.SupplierName, "^(?!.*[\\-&'' _]{2})[\\w][-\\w&'' ]{1,100}(?<![\\-_& ])$", "Contains two consequtive spaces, dahes, apostrophes or underscores")
                         .OrConditionSetNamed("SetThree", "30")
                             .WithCustomPredicateCondition<Customer>("Member years", c => c.MemberYears == 3, "Member years should be four but had @{MemberYears}", "CustomDIRequiredEvaluator")
                         .WithFailureValue("0")
                         .CreateRule();

        var contexts = RuleDataBuilder.AddForCondition("Customer Name and number", StaticData.CustomerOne())
                             .AndForAny(new Supplier("Supplier   Name", 1, 100.00M))
                             .AndForCondition("Member years", StaticData.CustomerThree()).Create();

        //it will just overrite the existing if there, we are in the same fixture so dependent on order may or may not registered.

        _conditionEngine.RegisterCustomEvaluatorForDependencyInjection("CustomDIRequiredEvaluator", typeof(CustomDIRequiredEvaluator<>));

        _conditionEngine.AddOrUpdateRule(theRule);

        var theResult = await _conditionEngine.EvaluateRule(theRule.RuleName, contexts);

        theResult.Should().Match<RuleResult>(r => r.IsSuccess == true && r.FailureMessages[0] == "Customer name or number is not correct" 
                                         && r.FailureMessages[1] == "Contains two consequtive spaces, dahes, apostrophes or underscores");
    }


    [Fact]
    public async Task Should_be_able_to_ingest_and_evaulate_a_json_rule_created_with_the_minimal_number_of_fields()
    {
        var jsonRulePath = DataHelper.GetJsonRuleFilePath("JsonRuleMinimal.json");

        var jsonString = File.ReadAllText(jsonRulePath);

        _conditionEngine.IngestRuleFromJson(jsonString);

        var theResult = await _conditionEngine.EvaluateRule("Rule102", RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create());

        theResult.Should().Match<RuleResult>(r => r.IsSuccess == false && r.Exceptions.Count == 0 && r.EvaluationChain!.FailureMessage == "Customer name or number is not correct");

    }


    [Fact]
    public async Task Should_be_able_to_subscribe_to_and_recieve_events_via_the_engine()
    {
        var theHandlerCalled = 0;

        var theRule = RuleBuilder.WithName("RuleOne")
                                    .ForConditionSetNamed("SetOne")
                                        .WithPredicateCondition<Customer>("CustomerSpend", c => c.CustomerName == "CustomerOne" && c.TotalSpend >= 1_000_000,
                                                                            "Total spend should be greater than or equal to 1,000,000",
                                                                            EventDetails.Create<ConditionResultEvent>(EventWhenType.OnSuccessOrFailure,PublishMethod.WaitForAll))
                                        .WithoutFailureValue()
                                        .CreateRule();

        Customer customer = new Customer("Major Corp.", 999, 999_999, 5, new Address("Major Corp Street", "Camden", "London", "NW1 LLL"));

        _conditionEngine.AddOrUpdateRule(theRule);

        var theSubscription = _conditionEngine.SubscribeToEvent<ConditionResultEvent>(HandleEvent);

        await Task.Delay(1000);

        var result = await _conditionEngine.EvaluateRule(theRule.RuleName, RuleDataBuilder.AddForAny(customer).Create());

        async Task HandleEvent(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            theHandlerCalled++;
            await Task.CompletedTask;
        }

        using(new AssertionScope())
        {
            theSubscription.Should().NotBeNull();
            theHandlerCalled.Should().Be(1);
        }

    }

    [Fact]
    public async Task Only_added_for_publishing_an_event_directly_with_the_engine_without_a_rule_to_negate_reflection_and_as_such_use_the_other_code_path()
    {
        var theHandlerCalled        = 0;
        var conditionResultEvent    = new ConditionResultEvent("SomeSender", true, typeof(Customer), StaticData.CustomerOneAsJsonString(),"TenantID",new(), null);
        var theSubscription         = _conditionEngine.SubscribeToEvent<ConditionResultEvent>(HandleEvent);

         await _conditionEngine.EventPublisher(conditionResultEvent,CancellationToken.None);

        await Task.Delay(50); ;//published events are done with fire and forget so need delay set higher than needed

        async Task HandleEvent(ConditionResultEvent theEvent, CancellationToken cancellationToken)
        {
            theHandlerCalled++;
            await Task.CompletedTask;
        }

        using (new AssertionScope())
        {
            theSubscription.Should().NotBeNull();
            theHandlerCalled.Should().Be(1);
        }
    }

}
