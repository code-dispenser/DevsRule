using DevsRule.Core.Areas.Caching;
using DevsRule.Core.Common.Exceptions;
using DevsRule.Tests.SharedDataAndFixtures.Data;
using DevsRule.Tests.SharedDataAndFixtures.Evaluators;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace DevsRule.Core.Tests.Unit.Areas.Caching;

public class InternalCacheTests
{
    [Fact]
    public void Should_be_able_add_an_item_to_cache_and_retrieve_it_with_the_same_key_used_to_store_it()
    {
        var cache       = new InternalCache();
        var customer    = StaticData.CustomerOne();
        var cacheKey    = String.Join("_", "Customer", customer.CustomerName);

        cache.AddOrUpdateItem(cacheKey, customer);

        _ = cache.TryGetItem<Customer>(cacheKey, out var theRetrievedCustomer);

        theRetrievedCustomer.Should().NotBeNull().And.Match<Customer>(c => c.CustomerName == customer.CustomerName && c.CustomerNo == customer.CustomerNo 
                                                                        && c.MemberYears == customer.MemberYears && c.Address == customer.Address);
    }

    [Fact]
    public void Should_be_able_add_an_item_to_cache_and_have_the_item_at_the_key_updated_if_adding_again_via_an_existing_key()
    {
        var cache       = new InternalCache();
        var customer    = StaticData.CustomerOne();
        var customerTwo = customer with { CustomerName = "New Name" };
        var cacheKey    = String.Join("_", "Customer", customer.CustomerName);

        cache.AddOrUpdateItem(cacheKey, customer);
        cache.AddOrUpdateItem(cacheKey, customerTwo);

        _ = cache.TryGetItem<Customer>(cacheKey, out var theRetrievedCustomer);

        theRetrievedCustomer.Should().NotBeNull().And.Match<Customer>(c => c.CustomerName == "New Name" && c.CustomerNo == customer.CustomerNo
                                                                        && c.MemberYears == customer.MemberYears && c.Address == customer.Address);
    }

    [Fact]
    public void Should_be_able_to_remove_an_item_from_cache_if_it_exists()
    {
        var cache       = new InternalCache();
        var customer    = StaticData.CustomerOne();
        var cacheKey    = String.Join("_", "Customer", customer.CustomerName);

        cache.AddOrUpdateItem(cacheKey, customer);

        var theItemIsInCache = cache.TryGetItem<Customer>(cacheKey, out _);

        cache.RemoveItem(cacheKey);

        var theRetrievedCustomer = cache.TryGetItem<Customer>(cacheKey, out var cacheItem) ? cacheItem : null;

        using (new AssertionScope())
        {
            theItemIsInCache.Should().BeTrue();
            theRetrievedCustomer.Should().BeNull();
        }
    }

    [Fact]
    public void Should_add_the_item_if_there_is_no_item_with_the_requested_key_already_in_the_cache()
    {
        var cache    = new InternalCache();
        var customer = StaticData.CustomerOne();
        var cacheKey = String.Join("_", "Customer", customer.CustomerName);

        var theItemIsInCache     = cache.TryGetItem<Customer>(cacheKey, out _);
        var theRetrievedCustomer = cache.GetOrAddItem<Customer>(cacheKey, () => customer);

        using(new AssertionScope())
        {
            theItemIsInCache.Should().BeFalse();
            theRetrievedCustomer.Should().NotBeNull().And.Match<Customer>(c => c.CustomerName == customer.CustomerName && c.CustomerNo == customer.CustomerNo
                                                                        && c.MemberYears == customer.MemberYears && c.Address == customer.Address);
        }
    }

    [Fact]
    public void Should_squash_exceptions_when_tying_to_dispose_objects_in_the_add_or_update_method()
    {
        var cache           = new InternalCache();
        var badEvaluatorOne = new ExceptionInDisposeEvaluator<Customer>();
        var badEvaluatorTwo = new ExceptionInDisposeEvaluator<Supplier>();
        var cacheKey        = String.Join("_", "Evaluator", badEvaluatorOne.GetType().FullName);

        cache.AddOrUpdateItem(cacheKey, badEvaluatorOne);
        cache.AddOrUpdateItem(cacheKey, badEvaluatorTwo);

        var theEvaluator = cache.TryGetItem<ExceptionInDisposeEvaluator<Supplier>>(cacheKey, out var evaluator) ? evaluator : null;

        theEvaluator.Should().NotBeNull().And.BeOfType<ExceptionInDisposeEvaluator<Supplier>>();
    }

    [Fact]
    public void Should_throw_disposing_removed_Item_exception_when_tying_to_dispose_objects_in_the_remove_Item_method()
    {
        var cache               = new InternalCache();
        var badEvaluatorOne     = new ExceptionInDisposeEvaluator<Customer>();
        var cacheKey            = String.Join("_", "Evaluator", badEvaluatorOne.GetType().FullName);
        
        var evaluatorIsInCache = cache.GetOrAddItem<ExceptionInDisposeEvaluator<Customer>>(cacheKey, () => badEvaluatorOne);

        using (new AssertionScope())
        {
            evaluatorIsInCache.Should().NotBeNull();

            FluentActions.Invoking(() => cache.RemoveItem(cacheKey)).Should().Throw<DisposingRemovedItemException>();
        }
    }


}
