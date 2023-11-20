using DevsRule.Core.Common.Validation;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Common.Validation;


public class CheckTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_throw_argument_exception_if_argument_is_null_empty_or_whitespace(string? requiredValue)
    {
        FluentActions.Invoking(() => SomeApplicationMethod(requiredValue)).Should().ThrowExactly<ArgumentException>();
    }

    private string? SomeApplicationMethod(string? requiredValue)
    {
        Check.ThrowIfNullOrWhitespace(requiredValue);

        return requiredValue;
    }
}
