
using Autofac;
using DevsRule.Core.Areas.Engine;
using DevsRule.Core.Areas.Evaluators;
using DevsRule.Tests.SharedDataAndFixtures.Models;
using DevsRule.Tests.SharedDataAndFixtures.Strategies;
using Xunit;

namespace DevsRule.Tests.SharedDataAndFixtures.SharedFixtures
{
    public class ConditionEngineDIFixture : IAsyncLifetime
    {
        private readonly IContainer _autoFacContainer;
        
        public ConditionEngine ConditionEngine { get; }

        public ConditionEngineDIFixture()
        { 
            _autoFacContainer = ConfigureAutofac();
            ConditionEngine   = _autoFacContainer.Resolve<ConditionEngine>();
        }

        private IContainer ConfigureAutofac()
        {
            var builder = new ContainerBuilder();

            var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType &&
                            t.BaseType.GetGenericTypeDefinition() == typeof(ConditionEvaluatorBase<>) 
                            && t.BaseType.GetGenericArguments()[0].IsGenericParameter)
                .ToList();

            foreach (var type in derivedTypes)
            {
                builder.RegisterGeneric(type).As(type).InstancePerLifetimeScope();
            }

            builder.RegisterType<SomeStrategy>().As<IStrategy<Customer>>().InstancePerLifetimeScope();

            builder.Register<ConditionEngine>(c =>
            {
                var context = c.Resolve<IComponentContext>();
                return new ConditionEngine(type => context.Resolve(type));
            }).SingleInstance();

            
            return builder.Build();
        }
        

        public async Task DisposeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }
    }
}
