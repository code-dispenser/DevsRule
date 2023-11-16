using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;

namespace DevsRule.Core.Areas.Rules;

public interface IAddData
{
    /// <summary>
    /// Specifies that the data context can be used for any condition requiring data of the supplied type. Although dynamic
    /// is used to allow differing data types used within a condition set, the actual type of data is checked against what the 
    /// condition expects.
    /// </summary>
    /// <param name="data">Data for the condition(s)</param>
    /// <returns>an IAddData to enable chaining.</returns>
    IAddData AndForAny(dynamic data);

    /// <summary>
    /// Specifies a data context for a named condition.
    /// </summary>
    /// <param name="named">The name of the condition that the data is for.</param>
    /// <param name="data">Data for the condition.</param>
    /// <returns>an IAddData to enable chaining.</returns>
    IAddData AndForCondition(string named, dynamic data);

    /// <summary>
    /// Finishes the build process by creating a RuleData object containing the data contexts for the conditions.
    /// </summary>
    /// <param name="forTenantID">Specifies the tenantID of the data supplied for the condition. 
    /// The tenantID is passed to both evaluators and added to events.</param>
    /// <returns></returns>
    RuleData Create(string forTenantID = GlobalStrings.Default_TenantID);
}


///<inheritdoc cref="IAddData"/>
/// <summary>
/// Used to build the RuleData object which is an array of DataContexts. Each condition within a rule has an associated data context
/// for evaluation.
/// </summary>
public class RuleDataBuilder : IAddData
{
    private List<DataContext> _contextData = new();

    private RuleDataBuilder() { }

    public static IAddData AddForAny(dynamic data)
    
        => new RuleDataBuilder().AndForAny(data);

    public IAddData AndForAny(dynamic data)

        => AddConditionData(data, "");
    
    public static IAddData AddForCondition(string named, dynamic data)

        => new RuleDataBuilder().AndForCondition("", data);

    public IAddData AndForCondition(string named, dynamic data)
        
        => AddConditionData(data, named);

    public RuleData Create(string forTenantID = GlobalStrings.Default_TenantID)

        => new (_contextData.ToArray(), forTenantID);

    private RuleDataBuilder AddConditionData(dynamic data, string conditionName = "")
    {
        if (false == String.IsNullOrWhiteSpace(conditionName))
        {
            if (false == _contextData.Exists(c => c.ConditionName == conditionName)) _contextData.Add(new DataContext(data, conditionName));
        }
        else
        {
            if (false == _contextData.Exists(c => c.Data.GetType() == data.GetType())) _contextData.Add(new DataContext(data, conditionName));
        }
        
        return this;
    }
}
