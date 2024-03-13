using Autofac;
using Autofac.Extensions.DependencyInjection;
using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Events;
using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Demo.BasicConsole.Common.CustomConditionEvaluators;
using DevsRule.Demo.BasicConsole.Common.EventHandlers;
using DevsRule.Demo.BasicConsole.Common.Events;
using DevsRule.Demo.BasicConsole.Common.Models;
using DevsRule.Demo.BasicConsole.Common.StaticData;
using DevsRule.Demo.BasicConsole.Scenarios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevsRule.Demo.BasicConsole;

internal class Program
{
    static async Task Main(string[] args)
    {
        /*
            * IMPORTANT - The data objects passed to condition evaluators are not cloned i.e a change to a data object in a custom evaluator would affect the original that may also being passed 
            * to other conditions if you have used the same instance of the data type for all conditions using that data type.
            * 
            * Events are given a cloned serialized version. When this data is accessed via a TryGetData method on the event, a json string is deserialized to an object and as such maintains 
            * immutability across event handlers.
            * 
            * Given the cloning for events, data objects used as contexts for the conditions that have events, need to ensure that the data contexts are serializable using the system.text.json 
            * JsonSerializer. If you are not using events then serialization is not an issue. Any serialization issues encountered during event creation are added as an Exception to the event 
            * class and as such, the event process would proceed as normal minus the data object.
            * 
            * You can register the ConditionEngine in a DI container as a singleton/single instance. If you need to use custom evaluators (requiring constructor injection) or dynamic event 
            * handlers then you will need to use your chosen DI container
            * 
            * Custom evaluators without constructor injection do not need a DI container. 
            * All custom evaluators do need adding to the ConditionEngine in order for the engine to know what types it has to create and/or request from the DI container.
            * 
            * As well as using dynamic event handlers you can also subscribe to receive events that are handled in your local event handlers i.e forms/view models etc
            * 

        */

        try
        {
            //var conditionEngine = ConfigureMicrosoftContainer().GetRequiredService<ConditionEngine>();
            var conditionEngine = ConfigureAutofac().Resolve<ConditionEngine>();

            var lineSeparator = new String('-', 100) + "\r\n";

            Console.WriteLine("Store card application single condition and data context");
            Console.WriteLine(lineSeparator);

            var storeCardApplications = new StoreCardApplications(conditionEngine);

            await storeCardApplications.UsingCodeSingleContextAndCondition();
            await storeCardApplications.UsingJsonSingleContextAndCondition();

            Console.WriteLine("\r\nStore card application multiple conditions and data contexts");
            Console.WriteLine(lineSeparator);

            await storeCardApplications.UsingCodeMultipleContextAndConditions();
            await storeCardApplications.UsingJsonMultipleContextAndConditions();

            Console.WriteLine("\r\nCustomer discount using multiple condition sets, just one data context");
            Console.WriteLine(lineSeparator);

            await new OrderDiscounts(conditionEngine).CustomerDiscountMultipleConditionSets();

            Console.WriteLine("\r\nUsing the built-in predicate evaluator and a custom predicate evaluator with a constructor injected httpclientfactory");
            Console.WriteLine(lineSeparator);

            await new CustomEvaluatorDI(conditionEngine).UseCustomAndBuiltInEvaluators();

            Console.WriteLine("\r\nUsing a context specific evaluator with a custom predicate condition and a non predicate condition, without dependency injection");
            Console.WriteLine(lineSeparator);

            await new CustomEvaluatorNoDI(conditionEngine).UseAContextSpecificCustomEvaluatorForConditions();

            Console.WriteLine("\r\nUsing a json rule with the bare minimum of fields");
            Console.WriteLine(lineSeparator);

            await new MinimalRuleInfo(conditionEngine).AMinimalRule();

            Console.WriteLine("\r\nRegister to receive events that conditions may raise");
            Console.WriteLine(lineSeparator);

            await new SubscribeToEvents(conditionEngine).RunRuleAndHandleSubscribedForEventsThatAreWaitForAll_1_Second_Delay_In_Handler();
            Console.WriteLine();
            await new SubscribeToEvents(conditionEngine).RunRuleAndHandleSubscribedForEventsThatAreFireAndForget();

            Console.WriteLine("\r\nRegistered event handlers in your DI container can receive events that conditions may raise");
            Console.WriteLine(lineSeparator);

            await new DynamicEventHandlers(conditionEngine).RuleWithEventDynaicallyHandledUsingWaitForAll();
            Console.WriteLine();
            await new DynamicEventHandlers(conditionEngine).RuleWithEventDynaicallyHandledUsingFireAndFoget();

            Console.WriteLine("\r\nUse which ever combination is applicable to your scenario");
            Console.WriteLine(lineSeparator);

            await new CustomEvaluatorsAllEventsAndDI(conditionEngine).MixAndMatch();

            Console.WriteLine("\r\nAll different conditions types in a single condition set");
            Console.WriteLine(lineSeparator);

            await new CustomConditionsAndEvaluators(conditionEngine).MixAndMaxConditions();

            Console.WriteLine("\r\nMiscellaneous ways to evaluate and/or check rules");
            Console.WriteLine(lineSeparator);

            var miscellaneous = new Miscellaneous(conditionEngine);

            await miscellaneous.EvaluatingRulesNotAddedToTheEngine();

            Console.WriteLine("\r\nUsing the engine to test your rules via a try catch");
            Console.WriteLine(lineSeparator);

            await miscellaneous.UseTheEngineToTestYourJsonRuleSyntax();

            /*
                * For additional testing I would create a console app install the NuGet System.Linq.Dynamic.Core and use DynamicExpressionParser.ParseLambda to test lambda strings
                * which the condition uses.Or create a separate PredicateCondition i.e new PredicateCondition<DataType> as internally the code does a ToString on the expression and
                * then converts the string back into a Func using DynamicExpressionParser.ParseLambda from Dynamic.Core.
                * Dynamic.Core will throw and exception if it cannot parse the lambda.
            */

        }
        catch (Exception ex)
        {
           Console.WriteLine($"Error: {ex.Message}");
        }

        Console.ReadLine();
    }

    private static ServiceProvider ConfigureMicrosoftContainer()
    {
        var appSettings = new AppSettings("admin@devsrulesdemo.com", "Service API Endpoint", "Server=.;Initial Catalog=FAKE;Integrated Security= SSPI; Encrypt=false;MultipleActiveResultSets=True;");

        return Host.CreateApplicationBuilder()
                .Services.AddHttpClient()
                         .AddSingleton(appSettings)
                         .AddTransient<IEventHandler<AnotherDiscountRuleConditionEvent>,AnotherDiscountRuleConditionEventHandler>()
                         .AddTransient<IEventHandler<DeviceRuleEvent>, DeviceRuleEventHandler>()
                         .AddTransient<IEventHandler<DeviceConditionEvent>, DeviceBatteryEventHandler>()
                         .AddTransient<MyCustomGenericDIAwareEvaluator<Address>>()
                         .AddTransient<ProbeValueConditionEvaluator>()
                         .AddSingleton<ConditionEngine>(provider => new ConditionEngine(type => provider.GetRequiredService(type)))
                .BuildServiceProvider();


    }

    private static IContainer ConfigureAutofac()
    {
        var appSettings         = new AppSettings("admin@devsrulesdemo.com", "Service API Endpoint", "Server=.;Initial Catalog=FAKE;Integrated Security= SSPI; Encrypt=false;MultipleActiveResultSets=True;");
        var serviceCollection   = new ServiceCollection().AddHttpClient();

        var builder = new ContainerBuilder();

        builder.Populate(serviceCollection);
        builder.RegisterInstance(appSettings).SingleInstance();
        builder.RegisterType<AnotherDiscountRuleConditionEventHandler>().As<IEventHandler<AnotherDiscountRuleConditionEvent>>().InstancePerDependency();
        builder.RegisterType<DeviceRuleEventHandler>().As<IEventHandler<DeviceRuleEvent>>().InstancePerDependency();
        builder.RegisterType<DeviceBatteryEventHandler>().As<IEventHandler<DeviceConditionEvent>>().InstancePerDependency();
        builder.RegisterType<MyCustomGenericDIAwareEvaluator<Address>>().InstancePerDependency();
        builder.RegisterType<ProbeValueConditionEvaluator>().InstancePerDependency();
        builder.Register(c =>
        {
            var context = c.Resolve<IComponentContext>();
            return new ConditionEngine(type => context.Resolve(type));
        }).As<ConditionEngine>().SingleInstance();


        return builder.Build();

    }
}