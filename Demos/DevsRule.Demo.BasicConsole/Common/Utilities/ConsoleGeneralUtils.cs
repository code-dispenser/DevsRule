using DevsRule.Core.Areas.Rules;
using System.Text;

namespace DevsRule.Demo.BasicConsole.Common.Utilities;

public static class ConsoleGeneralUtils
{
    public static async Task WriteToJsonFile(Rule rule, string filePath, bool writeIndented = true, bool useEscaped = true )
   
        => await File.WriteAllTextAsync(filePath, rule.ToJsonString(writeIndented,useEscaped));


    public static async Task<string> ReadJsonRuleFile(string filePath)

        => await File.ReadAllTextAsync(filePath);


    public static List<string> BuildChainString(RuleResult ruleResult)
    {
        var chainList = new List<string>();
        var result   = ruleResult;

        while (result != null)
        {
            var conditionChain = result.EvaluationChain;
            while (conditionChain != null)
            {
                chainList.Add($"{conditionChain.SetName} - {conditionChain.ConditionName} ({conditionChain.IsSuccess})");
                conditionChain = conditionChain.EvaluationChain;
            }
            chainList.Add(result.RuleName);
            result = result.RuleResultChain;
        }
        chainList.Reverse();
        return chainList;
    }
}
