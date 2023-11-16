using DevsRule.Core.Areas.Evaluators;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;

namespace DevsRule.Core.Common.Seeds;

public delegate IConditionEvaluator ConditionEvaluatorResolver(string evaluatorTypeName, Type contextType);

public delegate Task EventPublisher(IEvent eventToPublish, CancellationToken cancellationToken, PublishMethod publishMethod);

public delegate Task HandleEvent<TEvent>(TEvent theEvent, CancellationToken cancellationToken) where TEvent : IEvent;
