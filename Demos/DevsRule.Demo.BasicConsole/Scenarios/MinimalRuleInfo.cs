using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Demo.BasicConsole.Common.StaticData;

namespace DevsRule.Demo.BasicConsole.Scenarios;

public class MinimalRuleInfo
{
    private readonly ConditionEngine _conditionEngine;

    public MinimalRuleInfo(ConditionEngine conditionEngine)
    
        => _conditionEngine = conditionEngine;
    
    public async Task AMinimalRule()
    {
        /*
            * IMPORTANT, type names such as ContextTypeNames or EventTypeName can be shortened but the Rule.ToJsonString will provide the full name.
            * When the json is converted to a Rule, the engine scans (cached) types in assemblies and uses FullName.EndsWith(typeName) for matching
            * if your name does not have a '.' in it one will be added to the start for the namespace separator
            * If you were confident that the type name used is unique then the following ContextTypeName: "DevsRule.Demo.BasicConsole.Common.Models.Customer"
            * could be shortened to just Customer, or Model.Customer or Common.Models.Customer dependent on what makes it unique etc
        */


        string minimalJsonRuleString = """
                                        {
                                            "RuleName": "RuleWithMinimalFields",
                                            "ConditionSets": [
                                                {
                                                    "ConditionSetName": "SetOne",
                                                    "Conditions": [
                                                            {
                                                                "ContextTypeName": "DevsRule.Demo.BasicConsole.Common.Models.Customer",
                                                                "ConditionName": "Customer Name condition",
                                                                "ToEvaluate": "c => c.CustomerName == \"Danielle Baker\"",
                                                                "FailureMessage": "The customer name should be equal to CustomerOne",
                                                                "EvaluatorTypeName": "PredicateConditionEvaluator",
                                                                "IsLambdaPredicate": true
                                                            }
                                                        ]
                                                }
                                            ]
                                        }
                                        """;

        /*
            * Please also see rules the JsonRules folder to see rules with all of the fields present. 
            * 
            * Rule
            * By default unless specified all rules will be for any tenant in a multitenant environment "TenantID": "All_Tenants"
            * By default unless specified all rules will use the "CultureID": "en-GB" 
            * Rules get cached using a combination of RuleName + TenantID + CultureID. In essence you can create a rule with the same name that targets different tenants and/or different languages for the messages etc.
            * i.e DiscountRule_TenantOne_en_GB and DiscountRule_All_Tenants_fr-FR
            * Setting a cultureID does not alter any thread culture its just an identifier for you i.e maybe the failure messages are displayed to clients and as such written in the specific language etc
            * By default unless specified all rules are enabled by default "IsEnabled": true . You can overwrite the rule with IsEnabled: false, to leave it in place.
            * By default unless specified the Rule will have an empty "FailureValue":""
            * 
            * ConditionSet
            * By default unless specified the ConditionSets will have an empty "SetValue":""
            * The SetValue if entered of the passing set (sets use the Or logic) will be used for the RuleResult SuccessValue.
            * 
            * Condition
            * The AdditionalInfo dictionary if omitted will default to an empty Dictionary<string,string>
         */


         _conditionEngine.IngestRuleFromJson(minimalJsonRuleString);

        var ruleResult = await _conditionEngine.EvaluateRule("RuleWithMinimalFields", RuleDataBuilder.AddForAny(DataStore.GetCustomer(1)!).Create()!);

        Console.WriteLine($"Rule IsSuccess: {ruleResult.IsSuccess} - rule time {ruleResult.RuleTimeMilliseconds}ms - {ruleResult.RuleTimeMicroseconds} microseconds");
    }
}
