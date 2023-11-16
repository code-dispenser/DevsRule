
using DevsRule.Core.Common.Seeds;

namespace DevsRule.Core.Common.Models
{
    internal class JsonRule
    {

        public string?  RuleName        { get; set; }
        public string   TenantID        { get; set; } = GlobalStrings.Default_TenantID;
        public string   CultureID       { get; set; } = GlobalStrings.Default_CultureID;
        public string?  FailureValue    { get; set; }
        public bool     IsEnabled       { get; set; } = true;

        public EventDetails? RuleEventDetails { get; set; }

        public List<ConditionSet> ConditionSets { get; set; } = new();

        public class EventDetails
        {
            public string EventTypeName { get; set; } = default!;
            public string EventWhenType { get; set; } = default!;
            public string PublishMethod { get; set; } = default!;
        }

        public class ConditionSet
        {
            public string? ConditionSetName      { get; set; }
            public string? SetValue              { get; set; }
            public List<Condition> Conditions    { get; set; } = new();

            public class Condition
            {
                public string? ConditionName     { get; set; }
                public string? ContextTypeName   { get; set; }
                public string? ToEvaluate        { get; set; }
                public string? FailureMessage    { get; set; }
                public string? EvaluatorTypeName { get; set; }
                public bool IsLambdaPredicate    { get; set; }
                public Dictionary<string, string> AdditionalInfo { get; set; } = new();

                public EventDetails? ConditionEventDetails { get; set; }


            }

        }


    }

}
