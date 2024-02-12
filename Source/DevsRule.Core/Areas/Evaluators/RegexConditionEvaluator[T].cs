using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;
using System.Text.RegularExpressions;

namespace DevsRule.Core.Areas.Evaluators
{
    public sealed class RegexConditionEvaluator<TContext> : ConditionEvaluatorBase<TContext>
    {
        public override async Task<EvaluationResult> Evaluate(Condition<TContext> condition, TContext data, CancellationToken cancellationToken, string tenantID)
        {
            var regexCondition = (Condition<TContext>)condition;

            return await Task.FromResult(FromMatch(regexCondition, data)).ConfigureAwait(false);
        }

        private EvaluationResult FromMatch(Condition<TContext> regexCondition, TContext context)
        {
            var propertyValue   = base.GetPropertValueAsString(context!, regexCondition.ToEvaluate);
            var regexOptions    = RegexOptionsFromDictionary(regexCondition.AdditionalInfo);
 
            var pattern         = regexCondition.AdditionalInfo[GlobalStrings.Regex_Pattern_Key];
            var isMatch         = Regex.IsMatch(propertyValue, pattern, regexOptions, TimeSpan.FromSeconds(2));

            var failureMessage = isMatch ? String.Empty : base.BuildFailureMessage(regexCondition.FailureMessage, context!, ConditionEvaluatorBase<TContext>.MessageRegex);
            
            return isMatch ? new EvaluationResult(true, failureMessage) : new EvaluationResult(false,failureMessage);
        }

        private RegexOptions RegexOptionsFromDictionary(IReadOnlyDictionary<string,string> additionalInfo)
        {
            var regexOptions = RegexOptions.None;
            
            foreach(var key in additionalInfo.Keys)
            {
                var value = additionalInfo[key] ?? string.Empty;

                switch(key)
                {
                    case GlobalStrings.Regex_CultureInvariant_Key:          regexOptions = AddOption(regexOptions, RegexOptions.CultureInvariant, value);           break;
                    case GlobalStrings.Regex_IgnoreCase_Key:                regexOptions = AddOption(regexOptions, RegexOptions.IgnoreCase, value);                 break;
                    case GlobalStrings.Regex_Mulitline_Key:                 regexOptions = AddOption(regexOptions, RegexOptions.Multiline, value);                  break;
                    case GlobalStrings.Regex_Singleline_Key:                regexOptions = AddOption(regexOptions, RegexOptions.Singleline, value);                 break;
                    case GlobalStrings.Regex_IgnorePatternWhitespace_Key:   regexOptions = AddOption(regexOptions, RegexOptions.IgnorePatternWhitespace, value);    break;
                    case GlobalStrings.Regex_NonBacktracking_Key:           regexOptions = AddOption(regexOptions, RegexOptions.NonBacktracking, value);            break;
                    case GlobalStrings.Regex_RightToLeft_Key:               regexOptions = AddOption(regexOptions, RegexOptions.RightToLeft, value);                break;
                    case GlobalStrings.Regex_Compiled_Key:                  regexOptions = AddOption(regexOptions, RegexOptions.Compiled, value);                   break;
                    case GlobalStrings.Regex_ExplicitCapture_Key:           regexOptions = AddOption(regexOptions, RegexOptions.ExplicitCapture, value);            break;
                    case GlobalStrings.Regex_ECMAScript_Key:                regexOptions = AddOption(regexOptions, RegexOptions.ECMAScript, value);                 break;
                }

            }
            
            return regexOptions;
        }

        private RegexOptions AddOption(RegexOptions regexOptions, RegexOptions addOption, string value)
        
            => (bool.TryParse(value, out var result) == true && result == true) ? regexOptions | addOption : regexOptions;

    }
}
