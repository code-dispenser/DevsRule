namespace DevsRule.Core.Common.Seeds;

internal static class GlobalStrings
{
    public static readonly string Argument_Null_Empty_Exception_Message                         = "The argument cannot be null or empty.";

    public static readonly string No_Rule_Conditions_Exception_Message                          = "The rule condition set named '{0}' has no conditions.";
    public static readonly string No_Rule_Condition_Sets_Exception_Message                      = "The rule has no conditions sets.";
    public static readonly string No_Rule_Contexts_Exception_Message                            = "The rule is missing the contexts for the conditions and/or could not match the ConditionSetName '{0}' to the ConditionSetName in the ConditionSetContexts.";
    public static readonly string No_Matching_ContextSet_Exception_Message                      = "The rule could not match its ConditionSet name '{0}' to any in the ConditionSetContexts.";
    public static readonly string Missing_Rule_Contexts_Exception_Message                       = "The rule condition set named '{0}' is missing the following context types: ";
    public static readonly string Unmatched_Condition_Contexts_Exception_Message                = "The following data context for the rule '{0}' had these unmatching condition names: ";
    public static readonly string Missing_Rule_Contexts_Null_Context_Exception_Message          = "A data context for the rule is null";

    public static readonly string No_Rule_In_Cache_Exception_Message                            = "The rule named {0} as not found in the condition engines cache.";
    public static readonly string Rule_From_Json_Exception_Message                              = "The rule could not be created from the json string; please see the inner exception for details.";
    public static readonly string Context_Type_Assembly_Not_Found_Exception_Message             = "The assembly for the type of data context used for the condition {0} could not be found in the assemblies for the current app domain.";
    public static readonly string Missing_Condition_Evaluator_Exception_Message                 = "The condition evaluator named '{0}' has not been registered with the condition engine or could not be created.";
    public static readonly string Disposing_Removed_Item_Exception_Message                      = "An exception occurred whilst trying to dispose of the item '{0}' that was removed from cache; please see the inner exception for details.";
    public static readonly string Missing_Condition_ToEvaluate_Property_Value_Exception_Message = "The condition named '{0}' in the condition set '{1}' is missing it's ToEvaluate property value.";
    public static readonly string Missing_Regex_Pattern_Or_Pattern_Empty_Exception_Message      = "The condition named {0} is missing the Pattern Key or the pattern is null or empty";

    public static readonly string Missing_Condition_EvaluatorDI_Exception_Message              = "The condition evaluator named '{0}' was not found in the DI container; please see inner exception for details";


    public static readonly string Condition_Engine_Configuration_Exception_DI_Message           = "You cannot register an evaluator as being in a DI container without the condition engine being created with a CustomEvaluatorResolver callback function";
    public static readonly string Predicate_Condition_Compilation_Exception_Message             = "The compiled flag is set to false when it should be true for a predicate condition. Check that the json rule condition has the IsLambdaPredicate set to true";

    public static readonly string Event_Not_Found_Exception_Message                             = "The event named {0} could not be found whilst ingesting the json rule";

    public static readonly string CacheKey_Part_ConditionCreator    = "ConditionCreator";
    public static readonly string CacheKey_Part_Rule                = "Rule";
    public static readonly string CacheKey_Part_Evaluator           = "Evaluator";
    public static readonly string CacheKey_Part_Evaluator_Type      = "EvaluatorType";
    public static readonly string CacheKey_Part_Evaluator_Type_DI   = "EvaluatorType_DI";


    public const string Default_TenantID  = "All_Tenants";
    public const string Default_CultureID = "en-GB";


    public const string Predicate_Condition_Evaluator   = "PredicateConditionEvaluator";
    public const string Regex_Condition_Evaluator       = "RegexConditionEvaluator";


    public const string Regex_Pattern_Key                  = "Pattern";
    public const string Regex_Mulitline_Key                = "Mulitline";
    public const string Regex_Singleline_Key               = "Singleline";
    public const string Regex_IgnoreCase_Key               = "IgnoreCase";
    public const string Regex_CultureInvariant_Key         = "CultureInvariant";
    public const string Regex_IgnorePatternWhitespace_Key  = "IgnorePatternWhitespace";
    public const string Regex_NonBacktracking_Key          = "NonBacktracking";
    public const string Regex_RightToLeft_Key              = "RightToLeft";
    public const string Regex_Compiled_Key                 = "Compiled";
    public const string Regex_ExplicitCapture_Key          = "ExplicitCapture";
    public const string Regex_ECMAScript_Key               = "ECMAScript";

    
    
    


}

