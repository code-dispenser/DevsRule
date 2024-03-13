using DevsRule.Core.Areas.Rules;
using DevsRule.Core.Common.Models;
using DevsRule.Core.Common.Seeds;

namespace DevsRule.Tests.SharedDataAndFixtures.Models;

public record Address(string AddressLine, string Town, string City, string PostCode);
public record Customer(string CustomerName, int CustomerNo, decimal TotalSpend, int MemberYears, Address? Address = null);
public record Supplier(string SupplierName, int SupplierNo, decimal TotalPurchase);



public record Outcome(bool IsSuccess, string Details);
public record Employee(int EmployeeID, string EmployeeName);
public record CaseAction(DateTime ActionDateTime, Employee ActionBy, List<Outcome> Outcomes);
public record Case(string CaseRef, string CaseName, List<CaseAction> CaseActions);
public record Client(string ClientName, List<Case> Cases);

public record FakeCondition(Type ContextType);

public record NonSerializable(string Name, int Age, Type ContextType);


public interface IInjectableTestItem
{
    public string Name { get; }
}

public class InjectableTestItem : IInjectableTestItem
{
    public string Name { get; }

    public InjectableTestItem() => Name = "InjectableItem";

}
