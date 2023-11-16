using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Extensions;
using DevsRule.Core.Common.Models;
using DevsRule.Demo.BasicConsole.Common.Models;
using DevsRule.Demo.BasicConsole.Common.Seeds;
using DevsRule.Demo.BasicConsole.Common.StaticData;
using DevsRule.Demo.BasicConsole.Common.Utilities;

namespace DevsRule.Demo.BasicConsole.Scenarios;

public class StoreCardApplications
{
    private readonly ConditionEngine _conditionEngine;
    public StoreCardApplications(ConditionEngine conditionEngine)
     
        => _conditionEngine = conditionEngine;
    
    public async Task UsingCodeSingleContextAndCondition()
    {
        var applicant = DataStore.GenerateStoreCardAppliation(1)!;

        var storeCardRule = CreateSingleContextUsingTheRuleBuilder();
        //Or
        //var storeCardRule = CreateSingleContextManually
        
        _conditionEngine.AddOrUpdateRule(storeCardRule);

        /*
            * You can add a data context for any condition within a set by using ForAny on the builder which is equivlalent to new DataContext(data,conditionName = String.Empty)
            * Or you can set a specific instance of a type for a named Condition using ForCondition i.e. may be Customer 1 for condition 1 and customer 2 for condition 2 etc.
            * If the set had 4 conditions and you set ForAny and ForConditon on one of them, 3 would get the same instance/type and the the named one would gets its own instance/type
        */

        var theResult = await _conditionEngine.EvaluateRule(storeCardRule.RuleName, RuleDataBuilder.AddForAny(applicant).Create());
            
        //Or
        //var theResult = await _conditionEngine.EvaluateRule(storeCardRule.RuleName, new Context[] { new Context(applicant) });

        _ = theResult.OnFailure(f => Console.WriteLine($"Customer {applicant.CustomerID} application rejected: {f.FailureMessages[0]}"))
                      .OnSuccess(s => Console.WriteLine($"Customer {applicant.CustomerID} application approved"));  
    }

    public async Task UsingJsonSingleContextAndCondition()
    {
        /*
            * This file was created by using the Rule.ToJsonString() which uses System.Text.Json. The default will escape charaters such as < and > as can be seen in the file
            * if necessary you can set useEscaped = false in the ToJsonString method to have unescaped characters)
            * 
            * await ConsoleGeneralUtils.WriteToJsonFile(CreateSingleContextUsingTheRuleBuilder(), Path.Combine(ConsoleGlobalStrings.Json_Rules_Folder_Path, "StoreCardApplicationSingleContextRule.json"));
        */

        var storeCardRuleJsonString = await ConsoleGeneralUtils.ReadJsonRuleFile(Path.Combine(ConsoleGlobalStrings.Json_Rules_Folder_Path, "StoreCardApplicationSingleContextRule.json"));

        var applicant = DataStore.GenerateStoreCardAppliation(2)!;

        //var storeCardRuleJsonString = CreateSingleContextUsingBuilder().ToJsonString();

        _conditionEngine.IngestRuleFromJson(storeCardRuleJsonString);

        var ruleResult = await _conditionEngine.EvaluateRule("StoreCardApplicationSingleContextRule", new RuleData(new DataContext[] { new DataContext(applicant) }));
        
        _ = ruleResult.OnFailure(f => Console.WriteLine($"Customer {applicant.CustomerID} application rejected: {f.FailureMessages[0]}"))
                      .OnSuccess(s => Console.WriteLine($"Customer {applicant.CustomerID} application approved"));

    }

    public async Task UsingCodeMultipleContextAndConditions()
    {
        /*
            * Conditions short circuit using And 
        */ 
        var storeCardRule = CreateMultipleContextUsingTheRuleBuilder();
        //var storeCardRule = CreateMultipleContextManually();

        _conditionEngine.AddOrUpdateRule(storeCardRule);
        
        var customer = DataStore.GetCustomer(1)!;

        RuleData contexts = RuleDataBuilder.AddForCondition("AgeRequirement", customer)
                                           .AndForCondition("CountryRequirement", DataStore.GetAddress(1)!)
                                           .AndForCondition("OrderRequirement", DataStore.GetOrderHistory(1)!).Create();

        var ruleResult = await _conditionEngine.EvaluateRule("StoreCardApplicationMultipleContextRule", contexts);
        
        _= ruleResult.OnFailure(f =>
        {
            Console.WriteLine($"Customer {customer.CustomerID} application rejected: {f.FailureMessages[0]}");
            Console.WriteLine($"No. evaluations: {f.TotalEvaluations}");

        }).OnSuccess(s => Console.WriteLine($"Customer {customer.CustomerID} application approved - No. evaluations {s.TotalEvaluations}"));
    }

    public async Task UsingJsonMultipleContextAndConditions()
    {
        /*
             * The json file was created by using
             * await ConsoleGeneralUtils.WriteToJsonFile(CreateMultipleContextUsingTheRuleBuilder(), Path.Combine(ConsoleGlobalStrings.Json_Rules_Folder_Path, "StoreCardApplicationMultipleContextRule.json"));
        */ 

        var storeCardRuleJsonString = await ConsoleGeneralUtils.ReadJsonRuleFile(Path.Combine(ConsoleGlobalStrings.Json_Rules_Folder_Path, "StoreCardApplicationMultipleContextRule.json"));
     
        _conditionEngine.IngestRuleFromJson(storeCardRuleJsonString);

        var customer = DataStore.GetCustomer(2)!;

        RuleData contexts = RuleDataBuilder.AddForCondition("AgeRequirement", customer)
                                   .AndForCondition("CountryRequirement", DataStore.GetAddress(2)!)
                                   .AndForCondition("OrderRequirement", DataStore.GetOrderHistory(2)!).Create();

        var ruleResult = await _conditionEngine.EvaluateRule("StoreCardApplicationMultipleContextRule", contexts);

        _= ruleResult.OnFailure(f =>
        {
            Console.WriteLine($"Customer {customer.CustomerID} application rejected: {f.FailureMessages[0]}");
            Console.WriteLine($"No. evaluations: {f.TotalEvaluations}");

        }).OnSuccess(s => Console.WriteLine($"Customer {customer.CustomerID} application approved - No. evaluations {s.TotalEvaluations}"));
    }


    private Rule CreateSingleContextUsingTheRuleBuilder()

        => RuleBuilder.WithName("StoreCardApplicationSingleContextRule")
            .ForConditionSetNamed("AllRequirements")
                .WithPredicateCondition<StoreCardApplication>("Application", s => s.Age >= 18 && s.CountryOfResidence == "United Kingdom" && s.TotalOrders > 5,
                                                             "You must be over 18, living in the United Kingdom and have made at least 5 orders to be eligible")
                .WithoutFailureValue()
                .CreateRule();
     
    
    private Rule CreateSingleContextIndividually()
    {
        var condition = new PredicateCondition<StoreCardApplication>("Application", s => s.Age >= 18 && s.CountryOfResidence == "United Kingdom" && s.TotalOrders > 5,
                                                                     "You must be over 18, living in the United Kingdom and have made at least 5 orders to be eligible");
        var conditionSet = new ConditionSet("AllRequirments", condition);

        return new Rule("StoreCardApplicationSingleContextRule", conditionSet);
    }

    private Rule CreateMultipleContextUsingTheRuleBuilder()

        => RuleBuilder.WithName("StoreCardApplicationMultipleContextRule")
                .ForConditionSetNamed("AllRequirements")
                    .WithPredicateCondition<Customer>("AgeRequirement", c => new DateTime(c.DOB.Year, c.DOB.Month, c.DOB.Day).AddYears(18) < DateTime.Now, "You must be over 18 to apply")
                    .AndPredicateCondition<Address>("CountryRequirement", a => a.Country == "United Kingdom", "You must be a resident of the United Kingdom")
                    .AndPredicateCondition<OrderHistoryView>("OrderRequirement", o => o.TotalOrders > 5, "You must have made at least five purchases")
                .WithoutFailureValue()
                .CreateRule();

    private Rule CreateMultipleContextIndividually()
    {
        var ageCondition     = new PredicateCondition<Customer>("AgeRequirement", c => new DateTime(c.DOB.Year, c.DOB.Month, c.DOB.Day).AddYears(18) < DateTime.Now, "You must be over 18 to apply");
        var countryCondition = new PredicateCondition<Address>("CountryRequirement", a => a.Country == "United Kingdom", "You must be a resident of the United Kingdom");
        var orderCondition   = new PredicateCondition<OrderHistoryView>("OrderRequirement", o => o.TotalOrders > 5, "You must have made at least five purchases");

        var conditionSet = new ConditionSet("AllRequirments", ageCondition).AndCondition(countryCondition).AndCondition(orderCondition);

        return new Rule("StoreCardApplicationMultipleContextRule", conditionSet);

    }


}
