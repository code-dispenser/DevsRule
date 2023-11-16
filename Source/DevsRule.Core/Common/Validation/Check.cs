using DevsRule.Core.Common.Seeds;
using System.Runtime.CompilerServices;

namespace DevsRule.Core.Common.Validation;

public static class Check
{
    public static T ThrowIfNullOrWhitespace<T>(T argument, [CallerArgumentExpression("argument")] string argumentName = "")

        => (argument is null || (typeof(T).Name == "String" && String.IsNullOrWhiteSpace(argument as string)))
                ? throw new ArgumentException(GlobalStrings.Argument_Null_Empty_Exception_Message, argumentName)
                    : argument;

    public static T ThrowIfNull<T>(T argument, [CallerArgumentExpression("argument")] string argumentName = "")

        => (argument is null)
                ? throw new ArgumentNullException(GlobalStrings.Argument_Null_Empty_Exception_Message, argumentName)
                    : argument;

}