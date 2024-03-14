using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Extensions;
using DevsRule.Core.Common.Models;
using DevsRule.Demo.BasicConsole.Common.CustomConditionEvaluators;
using DevsRule.Demo.BasicConsole.Common.Models;
using DevsRule.Demo.BasicConsole.Common.StaticData;

namespace DevsRule.Demo.BasicConsole.Scenarios
{
    public class CustomEvaluatorNoDI
    {
        private readonly ConditionEngine _conditionEngine;
        public CustomEvaluatorNoDI(ConditionEngine conditionEngine)

            => _conditionEngine = conditionEngine;

        public async Task UseAContextSpecificCustomEvaluatorForConditions()
        {
            var reservationRule = CreateCustomConditionUsingTheRuleBuilder();
            // reservationRule = CreateRuleConditionsWithoutBuilder()

            /*
                * Register your specific evaluator type with the engine so it can create and cache it when needed.
                * As its a closed generic with no constructor injection there is no need to register it with any DI container.
                * MyOrderContextSpecificConditionEvaluator : ConditionEvaluatorBase<OrderHistoryView>
            */
            _conditionEngine.RegisterCustomEvaluator("MyOrderContextSpecificConditionEvaluator", typeof(MyOrderContextSpecificConditionEvaluator));
            _conditionEngine.AddOrUpdateRule(reservationRule);

            var customers = DataStore.Customers;

            var ruleTasks = customers.Select(c =>
                            {
                                var contexts = RuleDataBuilder.AddForAny(DataStore.GetOrderHistory(c.CustomerID)!).AndForAny(DataStore.GetAddress(c.CustomerID)!).Create();
                                return _conditionEngine.EvaluateRule(reservationRule.RuleName, contexts);
                            }).ToList();

            var ruleResults = await Task.WhenAll(ruleTasks);

            for(int index = 0; index < ruleResults.Length; index++)
            {
                var result = ruleResults[index];
                Console.WriteLine($"Details for {DataStore.GetCustomer(((dynamic)result.EvaluationChain!.EvaluationData!).CustomerID).CustomerName}");
               _= result.OnSuccess(r =>
                {
                    Console.WriteLine($"Passed a sets requirements");
                    Console.WriteLine($"\tNo. evaluations: {r.TotalEvaluations}, processing time for rule: {r.RuleTimeMilliseconds}ms - {r.RuleTimeMicroseconds} microseconds\r\n");
                })
                .OnFailure(r =>
                {
                    Console.WriteLine($"Failed requirements due to:");
                    r.FailureMessages.ForEach(f => Console.WriteLine($"\t{f}"));
                    Console.WriteLine($"\tNo. evaluations: {r.TotalEvaluations}, processing time for rule: {r.RuleTimeMilliseconds}ms - {r.RuleTimeMicroseconds} microseconds\r\n");
                });
            }

        }

        private Rule CreateCustomConditionUsingTheRuleBuilder()
        /*
            * Each condition gets an evaluator from the condition engine, one per condition 
         */
            => RuleBuilder.WithName("CanReserveProductsRule")
                                .ForConditionSetNamed("OrderHistoryRequirements")
                                    .WithCustomPredicateCondition<OrderHistoryView>("SpendCondition", o => o.TotalSpend > 3000, "The total spend of @{TotalSpend} does not satisfy the requirements", "MyOrderContextSpecificConditionEvaluator")
                                    .AndCustomCondition<OrderHistoryView>("OtherCondition", "Some text to evaluate", "Customer ID @{CustomerID} failed the condition", "MyOrderContextSpecificConditionEvaluator", new Dictionary<string, string> { ["PassOrFail"]="Pass" })
                                 .OrConditionSetNamed("ResidenceRequirements")
                                    .WithPredicateCondition<Address>("CountryCondition", a => a.Country == "United States", "Only residents of the USA can make reservations")
                                .WithoutFailureValue()
                                .CreateRule();

        
        private Rule CreateRuleConditionsWithoutBuilder()
        {
            var spendCondition   = new CustomCondition<OrderHistoryView>("SpendCondition", o => o.TotalSpend > 3000, "The total spend of @{TotalSpend} does not satisfy the requirements", "MyOrderContextSpecificConditionEvaluator");
            var otherCondition   = new CustomCondition<OrderHistoryView>("OtherCondition", "Some text to evaluate", "Customer ID @{CustomerID} failed the condition", "MyOrderContextSpecificConditionEvaluator", new Dictionary<string, string> { ["PassOrFail"]="Pass" });
            var countryCondition = new PredicateCondition<Address>("CountryCondition", a => a.Country == "United States", "Only residents of the USA can make reservations");

            var orderSet     = new ConditionSet("OrderHistoryRequirements",spendCondition).AndCondition(otherCondition);
            var residenceSet = new ConditionSet("ResidenceRequirements", countryCondition);

            return new Rule("CanReserveProductsRule", orderSet).OrConditionSet(residenceSet);
         }
    }
}
