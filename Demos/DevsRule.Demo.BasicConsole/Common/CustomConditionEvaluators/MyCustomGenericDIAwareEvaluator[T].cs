using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Demo.BasicConsole.Common.Models;
using System.Net.Http.Json;

namespace DevsRule.Demo.BasicConsole.Common.CustomConditionEvaluators
{
    public class MyCustomGenericDIAwareEvaluator<TContext> : ConditionEvaluatorBase<TContext>
    {
        private readonly HttpClient _httpClient;
        public MyCustomGenericDIAwareEvaluator(IHttpClientFactory clientFactory)

            => _httpClient = clientFactory.CreateClient();

        public override async Task<EvaluationResult> Evaluate(Condition<TContext> condition, TContext data, CancellationToken cancellationToken, string tenantID)
        {
            var additionalInfo = condition.AdditionalInfo;

            var productID = Random.Shared.Next(1, 100);

            bool enoughStock = false;
            bool conditionResult = false;

            var failureMessage = string.Empty;

            if (true == condition.IsLambdaPredicate)
            {
                conditionResult = condition.CompiledPredicate!(data);
                failureMessage = conditionResult == false ? base.BuildFailureMessage(condition.FailureMessage, data!, MessageRegex) : string.Empty;
            }

            CheckStock? stockCheck = null;

            if (true == conditionResult)
            {
                if (true == additionalInfo.TryGetValue("StockUrl", out var url))
                {
                    try
                    {
                        Console.WriteLine("Contacting an online web api to get dummy data, 5 second timeout value incase you are offline");
                        _httpClient.Timeout = TimeSpan.FromSeconds(5);
                        stockCheck = await _httpClient.GetFromJsonAsync<CheckStock>(url + productID, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        return new EvaluationResult(false, ex.Message, ex);
                    }

                    if (stockCheck != null && stockCheck.Stock > 50) enoughStock = true;
                }

                if (false == additionalInfo.TryGetValue("StockMessage", out var stockMessage)) stockMessage = string.Empty;

                if (enoughStock == false && stockCheck != null) failureMessage = string.Concat(failureMessage, "\r\n", base.BuildFailureMessage(stockMessage, stockCheck!, MessageRegex));


            }

            return new EvaluationResult(conditionResult && enoughStock, failureMessage);
        }

    }

}
