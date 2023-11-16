using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Tests.SharedDataAndFixtures.Evaluators;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.SharedFixtures;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DevsRule.Core.Tests.Integration.Areas.Evaluators;

public class CustomConditionEvaluatorTests : IClassFixture<ConditionEngineFixture>
{
    private readonly ConditionEngine    _conditionEngine;
    private readonly ITestOutputHelper  _outputHelper;
    public CustomConditionEvaluatorTests(ConditionEngineFixture conditionEngineFixture, ITestOutputHelper outputHelper)

        => (_conditionEngine, _outputHelper) = (conditionEngineFixture.ConditionEngine, outputHelper);

    [Fact]
    public async Task Should_be_able_to_add_and_use_custom_condition_evaluators_for_conditions_along_side_the_builtin_evlauators()
    {
        var openGenericEvaluatorName = "MyCustomPredicateEvaluator";
        var closedGenericEvaluatorName = "CustomerOnlyEvaluator";

        var openGenericEvaluatorType = typeof(CustomPredicteEvaluator<>);
        var closedGenericEvaluatorType = typeof(CustomerOnlyEvaluator);

        var customer = new Customer("John Smith", 101, 1000M, 5, new Address("25 Green Stree", "Camden", "London", "NW1 0HX"));
        var supplier = new Supplier("Harveys", 1002, 10_000M);

        _conditionEngine.RegisterCustomEvaluator(openGenericEvaluatorName, openGenericEvaluatorType);
        _conditionEngine.RegisterCustomEvaluator(closedGenericEvaluatorName, closedGenericEvaluatorType);

        var rule = RuleBuilder.WithName("RuleOne")
                        .ForConditionSetNamed("SetOne", "100")
                            .WithRegexCondition<Supplier>("NoDoubleSpaceOrAmbersanceName", s => s.SupplierName, @"^(?!.*[\-&''_]{2})(?!.* {2})[\w][-\w&'' ]{1,100}(?<![\-_& ])$", "Name must be between 2 and 100 character, no double spaces or double ambersands")
                            .AndCustomPredicateCondition<Supplier>("Supplier Name", s => s.SupplierName.StartsWith("S"), "Suppliers name should start with 'J'", openGenericEvaluatorName)
                        .OrConditionSetNamed("SetTwo", "200")
                            .WithCustomPredicateCondition<Customer>("Customer Name", c => c.Address!.City == "London", "Customers need to live in London", closedGenericEvaluatorName, new Dictionary<string, string>() { ["One"] = "some value" })
                            .AndPredicateCondition<Supplier>("Total purchases", s => s.TotalPurchase > 5_000, "Total purchases need to be over £5000")
                        .WithFailureValue("10")
                    .CreateRule();

        _conditionEngine.AddOrUpdateRule(rule);

        var contexts = RuleDataBuilder.AddForAny(supplier).AndForAny(customer).Create();

        var theRuleResult = await _conditionEngine.EvaluateRule("RuleOne", contexts);

        theRuleResult.Should().Match<RuleResult>(r => r.SuccessValue == "200" && r.IsSuccess == true && r.TotalEvaluations == 4);

    }


    //[Fact]
    //public async Task Should_be_able_to_chain_results_using_()
    //{
    //    var ruleOne = RuleBuilder.WithName("paul")
    //                                 .ForConditionSetNamed("kent", "50")
    //                                 .WithPredicateCondition<Customer>("CustName", c => c.CustomerName == "CustomerOne", "Should be customer one")
    //                                 .WithoutFailureValue()
    //                                 .CreateRule();

    //    var ruleTwo = RuleBuilder.WithName("russ")
    //                                 .ForConditionSetNamed("kent", "100")
    //                                 .WithPredicateCondition<Customer>("CustName", c => c.CustomerName == "CustomerTwo", "Should be customer one")
    //                                 .WithoutFailureValue()
    //                                 .CreateRule();

    //    var ruleThree = RuleBuilder.WithName("chris")
    //                                 .ForConditionSetNamed("kent", "200")
    //                                 .WithPredicateCondition<Supplier>("SupName", s => s.SupplierName == "CustomerTwo", "Should be suppliers name")
    //                                 .WithoutFailureValue()
    //                                 .CreateRule();


    //    _conditionEngine.AddRule(ruleOne);
    //    _conditionEngine.AddRule(ruleTwo);
    //    _conditionEngine.AddRule(ruleThree);

    //    var result = await _conditionEngine.FindAndEvaluateRule(ruleOne.RuleName, StaticData.CustomerOne())
    //                               .OnSuccess(ruleThree.RuleName, _conditionEngine, StaticData.SupplierOne())
    //                               .OnSuccess(ruleTwo.RuleName, _conditionEngine, StaticData.CustomerTwo())
    //                               .OnFailure(r => _outputHelper.WriteLine($"{r.RuleName} failed"));




    //}



}
