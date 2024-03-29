﻿using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;

namespace DevsRule.Demo.BasicConsole.Common.CustomConditionEvaluators
{
    public class MyCustomGenericPredicateConditionEvaluator<TContext> : ConditionEvaluatorBase<TContext>
    {
        public override async Task<EvaluationResult> Evaluate(Condition<TContext> condition, TContext data, CancellationToken cancellationToken, string tenantID)
        {
            /*
                * you can use the AdditionalInfo dictionary<string,string> to pass data from the condition for the custom evaluator 
             */

            /*
                * This evaluator has used predicates in the Condition so it will have a compiled predicate
                * do things before and/or after and then return a simple EvaluationResult
                * If you have no async work then just use Task.FromResult
                * You can Implement IConditionEvaluator<TContext> or ConditionEvaluatorBase<TContext> which implements IConditionEvaluator<TContext>
                * The ConditionEvaluator base has a method for replacing failure message placeholders
            */

            var result = condition.CompiledPredicate!(data);
            /*
                * Without any try catch blocks, any errors will get caught in the condition set and added to the condition result marking the condition result as false/failed
                * You can add a try catch here and add the exception to the evaluation result which will then set the condition result exception
                * but mark the result as true/passed if its something you want to ignore/handle and retry etc
            */

            /*
                * If you need to do any replacements for the failure message you can do it here
                * If you do not set any failure message or do replacements it will use the FailureMessage set in the condition
                * ConditionEvaluatorBase<TContext>.MessageRegex is a compiled regex for @{} to get the Matches for replacements
             */
            var failureMessage = result ? string.Empty : base.BuildFailureMessage(condition.FailureMessage, data!, MessageRegex);

            await Console.Out.WriteLineAsync($"MyCustomGenericPredicateConditionEvaluator was used for the condition {condition.ConditionName}");

            return new EvaluationResult(result, failureMessage);
        }

    }
}
