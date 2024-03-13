using System.Data;
using System.Text.Json;
using DevsRule.Tests.SharedDataAndFixtures.Models;

namespace DevsRule.Tests.SharedDataAndFixtures.Data;

public class StaticData
{
    public static Customer CustomerOne() => new Customer("CustomerOne", 111, 111.11M, 1);
    public static Customer CustomerTwo() => new Customer("CustomerTwo", 222, 222.22M, 2);
    public static Customer CustomerThree() => new Customer("CustomerThree", 333, 333.33M, 3);

    public static readonly string Customer_One_Name_Message = "The customer name should be CustomerOne";
    public static readonly string Customer_Two_Name_Message = "The customer name should be CustomerTwo";
    public static readonly string Customer_Three_Name_Message = "The customer name should be CustomerThree";

    public static readonly string Customer_One_No_Message = "The customer No. should be 111";
    public static readonly string Customer_Two_No_Message = "The customer No. should be 222";
    public static readonly string Customer_Three_No_Message = "The customer No. should be 333";


    public static readonly string Customer_One_Spend_Message = "The customer total spend should be 111.11";
    public static readonly string Customer_Two_Spend_Message = "The customer total spend should be 222.22";
    public static readonly string Customer_Three_Spend_Message = "The customer total spend should be 333.33";

    public static readonly string Customer_One_Member_Years = "The customer member years should be 1";
    public static readonly string Customer_Two_Member_Years = "The customer member years should be 2";
    public static readonly string Customer_Three_Member_Years = "The customer member years should be 3";


    public static Supplier SupplierOne() => new Supplier("SupplierOne", 111, 111.11M);
    public static Supplier SupplierTwo() => new Supplier("SupplierTwo", 222, 222.22M);
    public static Supplier SupplierThree() => new Supplier("SupplierThree", 333, 333.33M);

    public static readonly string Supplier_One_Name_Message = "The supplier name should be SupplierOne";
    public static readonly string Supplier_Two_Name_Message = "The supplier name should be SupplierTwo";
    public static readonly string Supplier_Three_Name_Message = "The supplier name should be SupplierThree";

    public static readonly string Supplier_One_No_Message = "The supplier No. should be 111";
    public static readonly string Supplier_Two_No_Message = "The supplier No. should be 222";
    public static readonly string Supplier_Three_No_Message = "The supplier No. should be 333";

    public static readonly string Supplier_One_Spend_Message = "The supplier total spend should be 111.11";
    public static readonly string Supplier_Two_Spend_Message = "The supplier total spend should be 222.22";
    public static readonly string Supplier_Three_Spend_Message = "The supplier total spend should be 333.33";


    public static readonly string JsonRuleText = """
                                                    {
                                                        "RuleName": "RuleOne",
                                                        "SuccessValue": "10",
                                                        "FailureValue": "5",
                                                        "IsEnabled": true,
                                                        "ConditionSets": [
                                                            {
                                                                "ConditionSetName": "SetOne",
                                                                "SetValue": null,
                                                                "Conditions": [
                                                                        {

                                                                            "ContextTypeName": "DevsRule.Tests.SharedDataAndFixtures.Models.Customer",
                                                                            "ConditionName": "Customer Name condition",
                                                                            "ToEvaluate": "c => (c.CustomerName == \"CustomerOne\")",
                                                                            "FailureMessage": "The customer name should be equal to CustomerOne",
                                                                            "EvaluatorTypeName": "PredicateConditionEvaluator",
                                                                            "AdditionalInfo": { "Pattern": "^SomePattern$" },
                                                                            "IsLambdaPredicate": true
                                                                        }
                                                                    ]
                                                            }
                                                        ]
                                                    }
                                                """;

    public static readonly string JsonRulePredicateMissingLambdaFlagText = """
                                                    {
                                                        "RuleName": "RuleOneMissingLambdaFlag",
                                                        "ConditionSets": [
                                                            {
                                                                "ConditionSetName": "SetOne",
                                                                "Conditions": [
                                                                        {
                                                                            "ContextTypeName": "DevsRule.Tests.SharedDataAndFixtures.Models.Customer",
                                                                            "ConditionName": "Customer Name condition",
                                                                            "ToEvaluate": "c => c.CustomerName = \"CustomerOne\"",
                                                                            "FailureMessage": "The customer name should be equal to CustomerOne",
                                                                            "EvaluatorTypeName": "PredicateConditionEvaluator"

                                                                        }
                                                                    ]
                                                            }
                                                        ]
                                                    }
                                                """;



    public static string CustomerOneAsJsonString()
    
        => JsonSerializer.Serialize<Customer>(StaticData.CustomerOne());

    public static Client GetClientHierarchy()
    {
        var outComeOne = new Outcome(true, "Won the case");
        var outComeTwo = new Outcome(false, "Client withdrew complaint");

        var outcomes = new List<Outcome> { outComeOne,outComeTwo};

        var employeeOne = new Employee(1, "Paul");
        var employeeTwo = new Employee(1, "Russ");

        var caseActionOne = new CaseAction(DateTime.Now.AddDays(-10), employeeOne, outcomes);
        var caseActionTwo = new CaseAction(DateTime.Now, employeeOne, outcomes);

        var caseActions = new List<CaseAction> { caseActionOne, caseActionTwo };

        var caseOne = new Case("RefOne", "Case One", caseActions);

        var cases = new List<Case> { caseOne };

        var client = new Client("Client One", cases);

        return client;
    }









}