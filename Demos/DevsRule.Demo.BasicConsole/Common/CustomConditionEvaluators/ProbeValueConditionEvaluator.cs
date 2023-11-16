using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Demo.BasicConsole.Common.Models;

namespace DevsRule.Demo.BasicConsole.Common.CustomConditionEvaluators
{
    public class ProbeValueConditionEvaluator : ConditionEvaluatorBase<Probe>
    {
        private readonly AppSettings _appSettings;
        public ProbeValueConditionEvaluator(AppSettings appSettings)
            
            => _appSettings = appSettings;

        public override async Task<EvaluationResult> Evaluate(Condition<Probe> condition, Probe data, CancellationToken cancellationToken, string tenantID)
        {
            var conditionPassed = false;

            if (false == condition.IsLambdaPredicate && condition.ToEvaluate.Contains("CalibrationTest"))
            {
                var dictionary = condition.AdditionalInfo;
                //Use defaults if missing
                var minValue  = dictionary.TryGetValue("MinValue", out var min) ? int.Parse(min)    : 25;
                var meanValue = dictionary.TryGetValue("MeanValue", out var mean) ? int.Parse(mean) : 50;
                var maxValue  = dictionary.TryGetValue("MaxValue", out var max) ? int.Parse(max)    : 90;

                await Console.Out.WriteLineAsync($"Ran calibration tests using values min: {minValue}, mean: {meanValue}, max: {maxValue}, all ok");
                await Console.Out.WriteLineAsync($"Added the calibration results to database using the connection string {_appSettings.DBWriteConnectionString}");

                conditionPassed = true;
            }
            else
            {
                await Console.Out.WriteLineAsync($"Test could not be run, probe flagged as failing");
            }

            return new EvaluationResult(conditionPassed, "Test not found");
        }
    }
}
