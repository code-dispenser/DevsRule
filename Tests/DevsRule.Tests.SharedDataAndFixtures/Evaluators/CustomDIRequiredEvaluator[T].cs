using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.Strategies;


namespace DevsRule.Tests.SharedDataAndFixtures.Evaluators
{
    public class CustomDIRequiredEvaluator<TContext> : ConditionEvaluatorBase<TContext>
    {
        private readonly IStrategy<TContext> _stategy;
        public CustomDIRequiredEvaluator(IStrategy<TContext> stategy) => _stategy = stategy;
        /*
             * Cant be used by the engine as the engine only deals with default parameterless constructors
        */
        public override async Task<EvaluationResult> Evaluate(Condition<TContext> condition, TContext data, CancellationToken cancellationToken, string tenantID)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var passed = false;

            if (true == condition.IsLambdaPredicate)
            {
                passed = condition.CompiledPrediate!(data);
            }

            _stategy.DoSomeThing(data);

            var failureMessage = passed ? String.Empty : base.BuildFailureMessage(condition.FailureMessage, data!, ConditionEvaluatorBase<Customer>.MessageRegex);

            return await Task.FromResult(new EvaluationResult(passed,failureMessage));
        }

   
    }
}
