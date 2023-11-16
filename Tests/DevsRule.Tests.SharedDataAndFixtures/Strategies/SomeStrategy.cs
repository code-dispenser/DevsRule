using DevsRule.Tests.SharedDataAndFixtures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevsRule.Tests.SharedDataAndFixtures.Strategies
{
    public interface IStrategy<TContext>
    {
        public void DoSomeThing(TContext data);
    }
    public class SomeStrategy : IStrategy<Customer>
    {
        public void DoSomeThing(Customer data)
        {
            var customerName = data.CustomerName;
        }
    }
}
