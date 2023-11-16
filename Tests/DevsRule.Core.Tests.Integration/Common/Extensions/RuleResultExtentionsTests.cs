using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.SharedFixtures;
using DevsRule.Core.Common.Extensions;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using DevsRule.Core.Common.Seeds;
using DevsRule.Core.Areas.Evaluators;
using FluentAssertions.Execution;
using DevsRule.Core.Common.Models;

namespace DevsRule.Core.Tests.Integration.Common.Extensions
{
    public class RuleResultExtentionsTests : IClassFixture<ConditionEngineFixture>
    {
        private readonly ConditionEngine _conditionEngine;
        private readonly ITestOutputHelper _outputHelper;

        public RuleResultExtentionsTests(ConditionEngineFixture conditionEngineFixture, ITestOutputHelper outputHelper)

            => (_conditionEngine, _outputHelper) = (conditionEngineFixture.ConditionEngine, outputHelper);


        [Fact]
        public async Task Should_be_able_to_chain_a_successfull_rule_result_in_order_to_run_another_rule_from_the_engine()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                                         .ForConditionSetNamed("SetOne", "50")
                                         .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "CustomerOne", "The customer name must be CustomerOne")
                                         .WithoutFailureValue()
                                         .CreateRule();

            var ruleTwo = RuleBuilder.WithName("RuleTwo")
                                         .ForConditionSetNamed("SetTwo", "100")
                                         .WithPredicateCondition<Customer>("CustomerNameTwo", c => c.CustomerName == "CustomerTwo", "The customer name must be CustomerTwo")
                                         .WithoutFailureValue()
                                         .CreateRule();

            _conditionEngine.AddOrUpdateRule(ruleOne);
            _conditionEngine.AddOrUpdateRule(ruleTwo);

            var theResult = await _conditionEngine.EvaluateRule("RuleOne", RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create())
                                       .OnSuccess("RuleTwo", _conditionEngine, RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create());


            theResult.Should().Match<RuleResult>(r => r.RuleName == "RuleTwo" && r.IsSuccess == true && r.SuccessValue == "100"
                                                   && r.RuleResultChain!.RuleName == "RuleOne" && r.RuleResultChain.IsSuccess == true);

        }
        [Fact]
        public async Task Should_be_able_to_chain_a_successfull_rule_result_in_order_to_run_another_rule_from_the_engine_using_a_lambda()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                                         .ForConditionSetNamed("SetOne", "50")
                                         .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "CustomerOne", "The customer name must be CustomerOne")
                                         .WithoutFailureValue()
                                         .CreateRule();

            var ruleTwo = RuleBuilder.WithName("RuleTwo")
                                         .ForConditionSetNamed("SetTwo", "100")
                                         .WithPredicateCondition<Customer>("CustomerNameTwo", c => c.CustomerName == "CustomerTwo", "The customer name must be CustomerTwo")
                                         .WithoutFailureValue()
                                         .CreateRule();

            _conditionEngine.AddOrUpdateRule(ruleOne);
            _conditionEngine.AddOrUpdateRule(ruleTwo);

            var theResult = await _conditionEngine.EvaluateRule("RuleOne", RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create())
                                       .OnSuccess(async () => await _conditionEngine.EvaluateRule(ruleTwo.RuleName, RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create()));


            theResult.Should().Match<RuleResult>(r => r.RuleName == "RuleTwo" && r.IsSuccess == true && r.SuccessValue == "100"
                                                   && r.RuleResultChain!.RuleName == "RuleOne" && r.RuleResultChain.IsSuccess == true);

        }

        [Fact]
        public async Task A_failing_result_should_not_run_the_next_rule_when_using_onsuccess()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                                         .ForConditionSetNamed("SetOne", "50")
                                         .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "WrongName", "The customer name must be CustomerOne")
                                         .WithoutFailureValue()
                                         .CreateRule();

            var ruleTwo = RuleBuilder.WithName("RuleTwo")
                                         .ForConditionSetNamed("SetTwo", "100")
                                         .WithPredicateCondition<Customer>("CustomerNameTwo", c => c.CustomerName == "CustomerTwo", "The customer name must be CustomerTwo")
                                         .WithoutFailureValue()
                                         .CreateRule();

            _conditionEngine.AddOrUpdateRule(ruleOne);
            _conditionEngine.AddOrUpdateRule(ruleTwo);

            var theResult = await _conditionEngine.EvaluateRule("RuleOne", RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create())
                                       .OnSuccess(async () => await _conditionEngine.EvaluateRule(ruleTwo.RuleName, RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create()));


            theResult.Should().Match<RuleResult>(r => r.RuleName == "RuleOne" && r.IsSuccess == false && r.RuleResultChain == null);

        }
        [Fact]
        public async Task A_passing_result_should_not_run_the_next_rule_when_using_onfailure()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                                         .ForConditionSetNamed("SetOne", "50")
                                         .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "CustomerOne", "The customer name must be CustomerOne")
                                         .WithoutFailureValue()
                                         .CreateRule();

            var ruleTwo = RuleBuilder.WithName("RuleTwo")
                                         .ForConditionSetNamed("SetTwo", "100")
                                         .WithPredicateCondition<Customer>("CustomerNameTwo", c => c.CustomerName == "CustomerTwo", "The customer name must be CustomerTwo")
                                         .WithoutFailureValue()
                                         .CreateRule();

            _conditionEngine.AddOrUpdateRule(ruleOne);
            _conditionEngine.AddOrUpdateRule(ruleTwo);

            var theResult = await _conditionEngine.EvaluateRule("RuleOne", RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create())
                                        .OnFailure(ruleTwo.RuleName,_conditionEngine, RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create());

            theResult.Should().Match<RuleResult>(r => r.RuleName == "RuleOne" && r.IsSuccess == true && r.RuleResultChain == null);

        }
        [Fact]
        public async Task Should_be_able_to_chain_a_successfull_rule_result_in_order_to_run_another_rule_not_in_the_engine()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                                         .ForConditionSetNamed("SetOne", "50")
                                         .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "CustomerOne", "The customer name must be CustomerOne")
                                         .WithoutFailureValue()
                                         .CreateRule();

            var ruleTwo = RuleBuilder.WithName("RuleTwo")
                                         .ForConditionSetNamed("SetTwo", "100")
                                         .WithPredicateCondition<Customer>("CustomerNameTwo", c => c.CustomerName == "CustomerTwo", "The customer name must be CustomerTwo")
                                         .WithoutFailureValue()
                                         .CreateRule();

            _conditionEngine.AddOrUpdateRule(ruleOne);

            var theResult = await _conditionEngine.EvaluateRule(ruleOne.RuleName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create())
                                                  .OnSuccess(ruleTwo, _conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create(),_conditionEngine.EventPublisher);


            theResult.Should().Match<RuleResult>(r => r.RuleName == "RuleTwo" && r.IsSuccess == true && r.SuccessValue == "100"
                                                   && r.RuleResultChain!.RuleName == "RuleOne" && r.RuleResultChain.IsSuccess == true);

        }
        [Fact]
        public async Task Should_be_able_to_take_some_action_on_a_successfule_result()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                             .ForConditionSetNamed("SetOne", "50")
                             .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "CustomerOne", "The customer name must be CustomerOne")
                             .WithoutFailureValue()
                             .CreateRule();

            var checkActionString = String.Empty;

            _conditionEngine.AddOrUpdateRule(ruleOne);

            var result = await ruleOne.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), _conditionEngine.EventPublisher)
                                .OnSuccess(r => checkActionString = r.RuleName);


            checkActionString.Should().Be("RuleOne");
        }

        [Fact]
        public async Task Should_pass_through_successes_if_is_result_is_a_failure()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                                         .ForConditionSetNamed("SetOne", "50")
                                         .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "NotCustomerONe", "The customer name must be CustomerOne")
                                         .WithoutFailureValue()
                                         .CreateRule();

            var ruleTwo = RuleBuilder.WithName("RuleTwo")
                                         .ForConditionSetNamed("SetTwo", "100")
                                         .WithPredicateCondition<Customer>("CustomerNameTwo", c => c.CustomerName == "CustomerTwo", "The customer name must be CustomerTwo")
                                         .WithoutFailureValue()
                                         .CreateRule();

            var ruleThree = RuleBuilder.WithName("RuleThree")
                             .ForConditionSetNamed("SetThree", "200")
                             .WithPredicateCondition<Customer>("CustomerNameThree", c => c.CustomerName == "CustomerThree", "The customer name must be CustomerThree")
                             .WithoutFailureValue()
                             .CreateRule();

            _conditionEngine.AddOrUpdateRule(ruleOne);
            _conditionEngine.AddOrUpdateRule(ruleTwo);
            _conditionEngine.AddOrUpdateRule(ruleThree);

            var checkActionString = String.Empty;

            var theResult = await _conditionEngine.EvaluateRule(ruleOne.RuleName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create())
                                                    .OnSuccess(ruleTwo.RuleName,_conditionEngine, RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create())
                                                    .OnSuccess(ruleThree.RuleName,_conditionEngine, RuleDataBuilder.AddForAny(StaticData.CustomerThree()).Create())
                                                    .OnFailure(r => checkActionString = r.RuleName);



            using (new AssertionScope())
            {

                theResult.Should().Match<RuleResult>(r => r.RuleName == "RuleOne" && r.IsSuccess == false && r.RuleResultChain == null);

                checkActionString.Should().Be("RuleOne"); 
            }
        }


        [Fact]
        public async Task Should_be_able_to_take_some_action_on_a_failed_result()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                             .ForConditionSetNamed("SetOne", "50")
                             .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "Incorrect", "The customer name must be CustomerOne")
                             .WithoutFailureValue()
                             .CreateRule();

            var checkActionString = String.Empty;

            _conditionEngine.AddOrUpdateRule(ruleOne);

            var result = await ruleOne.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create(), _conditionEngine.EventPublisher)
                                .OnFailure(r => checkActionString = r.RuleName);


            checkActionString.Should().Be("RuleOne");
        }

        [Fact]
        public async Task Should_be_able_to_run_a_rule_after_a_failure_without_the_rule_being_in_the_engine()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                             .ForConditionSetNamed("SetOne", "50")
                             .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "Incorrect", "The customer name must be CustomerOne")
                             .WithoutFailureValue()
                             .CreateRule();

            var ruleTwo = RuleBuilder.WithName("RuleTwo")
                             .ForConditionSetNamed("SetTwo", "100")
                             .WithPredicateCondition<Customer>("CustomerNameTwo", c => c.CustomerName == "CustomerTwo", "The customer name must be CustomerTwo")
                             .WithoutFailureValue()
                             .CreateRule();

            _conditionEngine.AddOrUpdateRule(ruleOne);

            Func<Task<RuleResult>> ruleRunner = () => ruleTwo.Evaluate(_conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create(),_conditionEngine.EventPublisher);

            var checkActionString = String.Empty;

            var result = await _conditionEngine.EvaluateRule(ruleOne.RuleName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create())
                                                    .OnFailure((result) => checkActionString = result.RuleName)
                                                    .OnFailure(ruleTwo, _conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create(), _conditionEngine.EventPublisher);     


            using (new AssertionScope())
            {
                checkActionString.Should().Be("RuleOne");
                result.Should().Match<RuleResult>(r => r.IsSuccess == true && r.RuleName == "RuleTwo" && r.RuleResultChain!.RuleName == "RuleOne" && r.RuleResultChain.IsSuccess == false);

            }
        }


        [Fact]
        public async Task Should_be_able_to_chain_a_failed_rule_result_and_run_another_rule_from_the_engine()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                                         .ForConditionSetNamed("SetOne", "50")
                                         .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "NotCustomerOne", "The customer name must be CustomerOne")
                                         .WithoutFailureValue()
                                         .CreateRule();

            var ruleTwo = RuleBuilder.WithName("RuleTwo")
                                         .ForConditionSetNamed("SetTwo", "100")
                                         .WithPredicateCondition<Customer>("CustomerNameTwo", c => c.CustomerName == "CustomerTwo", "The customer name must be CustomerTwo")
                                         .WithoutFailureValue()
                                         .CreateRule();

            _conditionEngine.AddOrUpdateRule(ruleOne);
            _conditionEngine.AddOrUpdateRule(ruleTwo);

            var theResult = await _conditionEngine.EvaluateRule("RuleOne", RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create())
                                       .OnFailure("RuleTwo", _conditionEngine, RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create());


            theResult.Should().Match<RuleResult>(r => r.RuleName == "RuleTwo" && r.IsSuccess == true && r.SuccessValue == "100"
                                                   && r.RuleResultChain!.RuleName == "RuleOne" && r.RuleResultChain.IsSuccess == false);

        }

        //[Fact]
        //public async Task Should_be_able_to_chain_a_failed_rule_result_in_order_to_run_another_rule_not_in_the_engine()
        //{
        //    var ruleOne = RuleBuilder.WithName("RuleOne")
        //                                 .ForConditionSetNamed("SetOne", "50")
        //                                 .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "NotCustomerOne", "The customer name must be CustomerOne")
        //                                 .WithoutFailureValue()
        //                                 .CreateRule();

        //    var ruleTwo = RuleBuilder.WithName("RuleTwo")
        //                                 .ForConditionSetNamed("SetTwo", "100")
        //                                 .WithPredicateCondition<Customer>("CustomerNameTwo", c => c.CustomerName == "CustomerTwo", "The customer name must be CustomerTwo")
        //                                 .WithoutFailureValue()
        //                                 .CreateRule();

        //    _conditionEngine.AddOrUpdateRule(ruleOne);

        //    var theResult = await _conditionEngine.EvaluateRule(ruleOne.RuleName, ContextBuilder.AddForAny(StaticData.CustomerOne()).ToContextArray())
        //                               .OnFailure(ruleTwo, _conditionEngine.GetEvaluatorByName, ContextBuilder.AddForAny(StaticData.CustomerTwo()).ToContextArray());


        //    theResult.Should().Match<RuleResult>(r => r.RuleName == "RuleTwo" && r.IsSuccess == true && r.SuccessValue == "100"
        //                                           && r.RuleResultChain!.RuleName == "RuleOne" && r.RuleResultChain.IsSuccess == false);

        //}

        [Fact]
        public async Task Should_pass_through_failures_if_result_is_a_success()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                                         .ForConditionSetNamed("SetOne", "50")
                                         .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "CustomerOne", "The customer name must be CustomerOne")
                                         .WithoutFailureValue()
                                         .CreateRule();

            var ruleTwo = RuleBuilder.WithName("RuleTwo")
                                         .ForConditionSetNamed("SetTwo", "100")
                                         .WithPredicateCondition<Customer>("CustomerNameTwo", c => c.CustomerName == "CustomerTwo", "The customer name must be CustomerTwo")
                                         .WithoutFailureValue()
                                         .CreateRule();

            var ruleThree = RuleBuilder.WithName("RuleThree")
                             .ForConditionSetNamed("SetThree", "200")
                             .WithPredicateCondition<Customer>("CustomerNameThree", c => c.CustomerName == "CustomerThree", "The customer name must be CustomerThree")
                             .WithoutFailureValue()
                             .CreateRule();

            _conditionEngine.AddOrUpdateRule(ruleOne);

            var checkActionString = String.Empty;

            var theResult = await _conditionEngine.EvaluateRule(ruleOne.RuleName, RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create())
                                       .OnFailure(ruleTwo, _conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create(),_conditionEngine.EventPublisher)
                                       .OnFailure(r => checkActionString = r.RuleName)
                                       .OnSuccess(ruleThree, _conditionEngine.GetEvaluatorByName, RuleDataBuilder.AddForAny(StaticData.CustomerThree()).Create(), _conditionEngine.EventPublisher);


            using (new AssertionScope())
            {

                theResult.Should().Match<RuleResult>(r => r.RuleName == "RuleThree" && r.IsSuccess == true && r.SuccessValue == "200"
                                                       && r.RuleResultChain!.RuleName == "RuleOne" && r.RuleResultChain.IsSuccess == true);

                checkActionString.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task Should_be_able_to_chain_the_the_result_onsuccess_without_async_await()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                                 .ForConditionSetNamed("SetOne", "50")
                                 .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "CustomerOne", "The customer name must be CustomerOne")
                                 .WithoutFailureValue()
                                 .CreateRule();

            _conditionEngine.AddOrUpdateRule(ruleOne);

            var theResult = await _conditionEngine.EvaluateRule("RuleOne", RuleDataBuilder.AddForAny(StaticData.CustomerOne()).Create());

            var theSuccessValue = String.Empty;

            Action<RuleResult> onSuccessAction = (r) => theSuccessValue = r.SuccessValue;

            theResult.OnFailure(f => theSuccessValue = f.FailureMessages[0])
                      .OnSuccess(onSuccessAction);

            theSuccessValue.Should().Be("50");

        }
        [Fact]
        public async Task Should_be_able_to_chain_the_result_onfailure_without_async_await()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                                 .ForConditionSetNamed("SetOne", "50")
                                 .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "CustomerOne", "The customer name must be CustomerOne")
                                 .WithoutFailureValue()
                                 .CreateRule();

            _conditionEngine.AddOrUpdateRule(ruleOne);

            var theResult = await _conditionEngine.EvaluateRule("RuleOne", RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create());

            var theFailureMessage = String.Empty;

            Action<RuleResult> onFailureAction = (r) => theFailureMessage = r.FailureMessages[0];

            theResult.OnFailure(onFailureAction)
                     .OnSuccess(r => theFailureMessage = r.IsSuccess.ToString());

            theFailureMessage.Should().Be("The customer name must be CustomerOne");

        }

        [Fact]
        public async Task Should_be_able_on_failure_to_access_the_result_and_evaluate_another_rule_in_the_lambda_statement()
        {
            var ruleOne = RuleBuilder.WithName("RuleOne")
                     .ForConditionSetNamed("SetOne", "50")
                     .WithPredicateCondition<Customer>("CustomerNameOne", c => c.CustomerName == "CustomerOne", "The customer name must be CustomerOne")
                     .WithoutFailureValue()
                     .CreateRule();

            var ruleTwo = RuleBuilder.WithName("RuleTwo")
                     .ForConditionSetNamed("SetOne", "50")
                     .WithPredicateCondition<Customer>("CustomerNameTwo", c => c.CustomerName == "CustomerTwo", "The customer name must be CustomerTwo")
                     .WithoutFailureValue()
                     .CreateRule();

            _conditionEngine.AddOrUpdateRule(ruleOne);
            _conditionEngine.AddOrUpdateRule(ruleTwo);

            var customerThreeData = RuleDataBuilder.AddForAny(StaticData.CustomerThree()).Create();
            var customerTwoData = RuleDataBuilder.AddForAny(StaticData.CustomerTwo()).Create();

            var previousResult = true;

            var theResult = await _conditionEngine.EvaluateRule(ruleOne.RuleName, customerThreeData)
                                                .OnFailure(async (result) => 
                                                {
                                                    previousResult = result.IsSuccess;
                                                    return await _conditionEngine.EvaluateRule(ruleTwo.RuleName, customerTwoData);
                                                });        

            using(new AssertionScope())
            {
                previousResult.Should().BeFalse();
                theResult.IsSuccess.Should().BeTrue();
            }

        }
    }
}

