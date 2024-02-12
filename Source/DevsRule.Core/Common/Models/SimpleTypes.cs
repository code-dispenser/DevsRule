using DevsRule.Core.Common.Seeds;
using DevsRule.Core.Common.Validation;
using System.Runtime.CompilerServices;

namespace DevsRule.Core.Common.Models;

/// <summary>
/// The result of a condition evaluation. This gets passed to the containing condition set which uses short-circuit
/// 'And' logic to determine whether to evaluate any other conditions within its set.
/// </summary>
/// <param name="IsSuccess">True or false</param>
/// <param name="FailureMeassage">The failure message if <paramref name="IsSuccess"/> is false</param>
/// <param name="Exception">For any exception that may occur during evaluation of the condition.</param>
public record EvaluationResult(bool IsSuccess, string FailureMeassage = "", Exception? Exception = null);


/// <summary>
/// The key used to store and retrieve items for the internal cache.
/// The key is the combination of all three parameters ItemName, TenantID and CultureID.
/// </summary>
/// <param name="ItemName">The name of the item to cache.</param>
/// <param name="TenantID">The tenantID for multitenant scenarios otherwise the default "All_Tenants" is used.</param>
/// <param name="CultureID">The cultureID to differentiate any rules with differing language failure messages, has a default value of "en-GB"</param>
public record CacheKey(string ItemName, string TenantID = GlobalStrings.Default_TenantID, string CultureID = GlobalStrings.Default_CultureID);

/// <summary>
/// Wrapper to hold the object to cache.
/// </summary>
/// <param name="Value">The object to cache.</param>
public record CacheItem(object Value);

/// <summary>
/// A container for the data to be passed to conditions for evaluation.
/// </summary>
/// <param name="Data">The data for all conditions or a specific condition dependent on the <paramref name="ConditionName"/> value.</param>
/// <param name="ConditionName">If specified instructs the condition set to match the instance of this data for the conditions evaluation.</param>
public record DataContext(dynamic Data, string ConditionName = "");

/// <summary>
/// A container to hold all of the data contexts required by the conditions within a rule or condition set.
/// </summary>
public class RuleData
{
    public DataContext[] Contexts { get;}
    public string        TenantID { get; }
    public int Length => Contexts.Length;
    public RuleData(DataContext[] dataContexts, string tenantID = GlobalStrings.Default_TenantID)
    {
        Contexts = dataContexts?.ToArray() ?? new DataContext[0];
        TenantID = tenantID;
    }
    
}

