using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;

namespace DevsRule.Tests.SharedDataAndFixtures.Evaluators
{
    public class TestConditionBaseEvaluator<TContext> : ConditionEvaluatorBase<TContext>
    {
        private readonly string _missingPropertyText;
        public TestConditionBaseEvaluator(string missingPropertyText) => _missingPropertyText = missingPropertyText;

        public override async Task<EvaluationResult> Evaluate(Condition<TContext> condition, TContext data, CancellationToken cancellationToken, string tenantID)
        {
            var failureMessage = base.BuildFailureMessage(condition.FailureMessage, data, MessageRegex, _missingPropertyText);

            return await Task.FromResult(new EvaluationResult(false, failureMessage));
        }
    }
}
