using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Extensions;
using DevsRule.Demo.BasicConsole.Common.CustomConditionEvaluators;
using DevsRule.Demo.BasicConsole.Common.Models;
using DevsRule.Demo.BasicConsole.Common.StaticData;

namespace DevsRule.Demo.BasicConsole.Scenarios
{
    public  class CustomEvaluatorDI
    {
        private readonly ConditionEngine _conditionEngine;
        public CustomEvaluatorDI(ConditionEngine conditionEngine)

            => _conditionEngine = conditionEngine;
        

        public async Task UseCustomAndBuiltInEvaluators()
        {
            /*
                * For Constructor injection from a DI container you need to register the type, closed or open generic with both the DI container
                * as well as the engine so it does not try to create and cache it, The engine will only create evaluators with parameterless constructors.
                * 
                * The engine will close the generic with the context type (TContext) and ask the DI container for the closed type 
                * i.e in this instance it will ask the DI container for an instance of MyCustomGenericDIAwareEvaulator<Address>.
                * As the engine needs to close the generic type with a context type at runtime you need to use typeof(MyCustomGenericDIAwareEvaulator<>)
                * (MyCustomGenericDIAwareEvaulator<TContext> : ConditionEvaluatorBase<TContext>)
            */

            _conditionEngine.RegisterCustomEvaluatorForDependencyInjection("MyCustomGenericDIAwareEvaulator", typeof(MyCustomGenericDIAwareEvaulator<>));

            var rule = CreateRuleUsingTheRuleBuilder();

            //Or _conditionEngine.AddOrUpdateRule(rule), ingestRuleFromJson calls AddOrUpdateRule(Rule) after it converts the json into a rule;

            _conditionEngine.IngestRuleFromJson(rule.ToJsonString());

            for (int index = 1; index <= 4; index++)
            {
                var contexts = RuleDataBuilder.AddForAny(DataStore.GetAccount(index)!).AndForAny(DataStore.GetAddress(index)!).Create();
                
                Console.WriteLine($"Running the rule for Customer No: {contexts.Contexts[0].Data.CustomerID} living in {contexts.Contexts[1].Data.Country}" + "\r\n");
                
                var theResult = await _conditionEngine.EvaluateRule(rule.RuleName, contexts);

                Console.WriteLine();//default host has logging for httpclient output to console, so adding a line space;
                              
                _ = theResult.OnFailure(f => f.FailureMessages.ForEach(f => Console.WriteLine(f + "\r\n"))) 
                             .OnSuccess(s => Console.WriteLine($"Passed credit and stock checks. Total evaluations {s.TotalEvaluations}" + "\r\n"));

                Console.WriteLine();
               
            }
        }

        private Rule CreateRuleUsingTheRuleBuilder()
        { 
            Dictionary<string,string> additionInfo = new Dictionary<string, string>() { ["StockUrl"]="https://dummyjson.com/products/", ["StockMessage"]="Sorry we do not have enougth stock of @{Title} to fullfil the request" };

            return RuleBuilder.WithName("WithAPIRule")
                    .ForConditionSetNamed("CheckStockAndCreditCardSet")
                        .WithPredicateCondition<CustomerAccount>("CreditCardCondition", a => a.CardNoOnFile != null, "CustomerID: @{CustomerID} does not have a credit card")
                        .AndCustomPredicateCondition<Address>("CanDeliverTo", a => a.Country == "United States", "We cannot deliver to customers in @{Country}", "MyCustomGenericDIAwareEvaulator",additionInfo)
                    .WithoutFailureValue()
                    .CreateRule();
        }          
    }
}
