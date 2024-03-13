using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;

namespace DevsRule.Tests.SharedDataAndFixtures.Evaluators;

public class ExceptionInDisposeEvaluator<TContext> : IConditionEvaluator<TContext>, IDisposable
{
    private bool _disposedValue;

    public async Task<EvaluationResult> Evaluate(Condition<TContext> condition, TContext data, CancellationToken cancellationToken, string tenantID)
    {

        return await Task.FromResult(new EvaluationResult(true));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing) throw new Exception("Bad stuff happened");

            _disposedValue = true;
        }
    }


    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
