using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Demo.BasicConsole.Common.Models;

namespace DevsRule.Demo.BasicConsole.Common.CustomConditionEvaluators
{
    public class MyOrderContextSpecificConditionEvaluator : ConditionEvaluatorBase<OrderHistoryView>
    {
        public override async Task<EvaluationResult> Evaluate(Condition<OrderHistoryView> condition, OrderHistoryView data, CancellationToken cancellationToken, string tenantID)
        {

            bool passed = false;
            /*
                * Maybe your custom conditions are not using predicates, and/or you want to use the same evaluator for both types of conditions
            */
            if (true == condition.IsLambdaPredicate)//if this is true the condition will have a compiled predicate
            {
                passed = condition.CompiledPrediate!(data);
            }
            else
            {
                /*
                    *  Do your own parsing of the ToEvaluate string or just ignore it if you only wanted some other type of rule in the mix.
                */
                passed = condition.AdditionalInfo.TryGetValue("PassOrFail", out var value)
                         ? value == "Passed" ? true : false
                         : false;
            }

            var failureMessage = passed ? string.Empty : base.BuildFailureMessage(condition.FailureMessage, data, MessageRegex);


            return await Task.FromResult(new EvaluationResult(passed, failureMessage));
        }
    }
}
