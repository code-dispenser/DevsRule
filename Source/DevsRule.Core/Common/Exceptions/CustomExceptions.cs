namespace DevsRule.Core.Common.Exceptions;

public class MissingConditionSetsException : Exception
{
    public MissingConditionSetsException(string message) : base(message) { }
}
public class MissingRuleContextsException : Exception
{
    public MissingRuleContextsException(string message) : base(message) { }
}
public class MissingConditionsException : Exception
{
    public MissingConditionsException(string message) : base(message) { }
}
public class RuleNotFoundException : Exception
{
    public RuleNotFoundException(string message, Exception? innerException = null) : base(message, innerException) { }
}

public class MissingConditionEvaluatorException : Exception
{
    public MissingConditionEvaluatorException(string message, Exception? innerException = null) : base(message,innerException) { }
}

public class RuleFromJsonException : Exception
{
    public RuleFromJsonException(string message, Exception? innerException = null) : base(message, innerException) { }
}

public class MissingConditionToEvaluatePropertyValue : Exception
{
    public MissingConditionToEvaluatePropertyValue(string message, Exception? innerException = null) : base(message, innerException) { }
}

public class ContextTypeAssemblyNotFound : Exception
{
    public ContextTypeAssemblyNotFound(string message, Exception? innerException = null) : base(message, innerException) { }
}

public class DisposingRemovedItemException : Exception
{
    public DisposingRemovedItemException(string message, Exception? innerException = null) : base(message, innerException) { }
}

public class MissingRegexPatternException : Exception
{
    public MissingRegexPatternException(string message, Exception? innerException = null) : base(message, innerException) { }
}

public class PredicateConditionCompilationException : Exception
{
    public PredicateConditionCompilationException(string message, Exception? innerException = null) : base(message, innerException) { }

}

public class EventNotFoundException : Exception
{
    public EventNotFoundException(string message, Exception? innerException = null) : base(message, innerException) { }

}