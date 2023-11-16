using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevsRule.Tests.SharedDataAndFixtures.Evaluators
{
    public class CustomPredicteEvaluator<TContext> : ConditionEvaluatorBase<TContext>
    {
        public override async Task<EvaluationResult> Evaluate(Condition<TContext> condition, TContext data, CancellationToken cancellationToken, string tenantID)
        {   /*
                * custom code, other checks etc 
            */ 
            var isSuccess = condition.IsLambdaPredicate ? condition.CompiledPrediate!(data) : false;

            var failureMessage = condition.FailureMessage;

            if (condition.AdditionalInfo.ContainsKey("DeleteMessage")) failureMessage = String.Empty;//Test bad evaluator message

            failureMessage = isSuccess ? String.Empty : base.BuildFailureMessage(failureMessage, data!, ConditionEvaluatorBase<TContext>.MessageRegex);

            return await Task.FromResult(new EvaluationResult(isSuccess));
        }
    }
}
