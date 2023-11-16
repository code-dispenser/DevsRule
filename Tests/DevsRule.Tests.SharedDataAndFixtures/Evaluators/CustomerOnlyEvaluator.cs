using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Utilities;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace DevsRule.Tests.SharedDataAndFixtures.Evaluators;

public class CustomerOnlyEvaluator : ConditionEvaluatorBase<Customer>
{
    public override async Task<EvaluationResult> Evaluate(Condition<Customer> condition, Customer data, CancellationToken cancellationToken, string tenantID)
    {

        var isSuccess = condition.IsLambdaPredicate ? condition.CompiledPrediate!(data) : false;

        var failureMessage = isSuccess ? String.Empty : base.BuildFailureMessage(condition.FailureMessage, data, ConditionEvaluatorBase<Customer>.MessageRegex);

        return await Task.FromResult(new EvaluationResult(isSuccess,failureMessage));
    }

}
