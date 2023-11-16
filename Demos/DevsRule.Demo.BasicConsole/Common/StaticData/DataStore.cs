using DevsRule.Demo.BasicConsole.Common.Models;

namespace DevsRule.Demo.BasicConsole.Common.StaticData;

public static class DataStore
{
    public static List<Customer>         Customers          { get; }
    public static List<CustomerAccount>  CustomerAccounts   { get; }
    public static List<Address>          CustomerAddresses  { get; }
    public static List<OrderHistoryView> OrderHistories     { get; }

    static DataStore()
    {
        Customers           = BuildCustomerList();
        CustomerAccounts    = BuildAccountList();
        CustomerAddresses   = BuildAddressList();
        OrderHistories      = BuildOrderHistories();
    }

    public static Customer? GetCustomer(int customerID)

        => Customers.Where(c => c.CustomerID == customerID).SingleOrDefault();

    public static Address? GetAddress(int customerID)

        => CustomerAddresses.Where(a => a.CustomerID == customerID).SingleOrDefault();

    public static CustomerAccount? GetAccount(int customerID)

        => CustomerAccounts.Where(a => a.CustomerID == customerID).SingleOrDefault();

    public static OrderHistoryView? GetOrderHistory(int customerID)

        => OrderHistories.Where(o => o.CustomerID == customerID).SingleOrDefault();

    public static StoreCardApplication? GenerateStoreCardAppliation(int customerID)
    {

        var customer        = Customers.Where(c => c.CustomerID == customerID).SingleOrDefault()!;
        var address         = CustomerAddresses.Where(c => c.CustomerID == customerID).SingleOrDefault()!;
        var orderHistory    = OrderHistories.Where(c => c.CustomerID == customerID).SingleOrDefault()!;

        return new StoreCardApplication(customer.CustomerID, customer.CustomerName, AgeFromDOB(customer.DOB), address.Country, orderHistory.TotalOrders);
    }


    public static Device GetTenantDevice(int tenantID)
    
        => new Device
            (
                TenantID: tenantID,
                new Probe[] { 
                                new Probe(TenantID: tenantID, ProbeID: Guid.Parse("4e5a2275-76ed-4b02-af97-7334d0959931"), BatteryLevel: 4, ProbeValue: 51, ResponseTimeMs: 7, ErrorCount: 0),
                                new Probe(TenantID: tenantID, ProbeID: Guid.Parse("2f0a0f4d-9088-4c2e-bd77-116c5bcce769"), BatteryLevel: 9, ProbeValue: 75, ResponseTimeMs: 2, ErrorCount: 0)
                             },
                Online:true,
                PowerOnHours: 507
           );


    private static int GetCustomerYearsFromOrder(DateOnly firstOrderDate)

        => DateTime.Now.DayOfYear < firstOrderDate.DayOfYear
            ? (DateTime.Now.Year - firstOrderDate.Year - 1) <= 0 ? 0 : DateTime.Now.Year - firstOrderDate.Year
            : (DateTime.Now.Year - firstOrderDate.Year) <= 0 ? 0 : DateTime.Now.Year - firstOrderDate.Year;
    private static int AgeFromDOB(DateOnly DOB)
    
    => DateTime.Now.DayOfYear < DateTime.Now.DayOfYear ? DateTime.Now.Year - DOB.Year -1 : DateTime.Now.Year - DOB.Year;

    private static List<Customer> BuildCustomerList()

        => new List<Customer>()
        {
            new Customer(1,"Danielle Baker",new DateOnly(2006,05,09),"01695 720312",CustomerType.Student),
            new Customer(2, "Frederik Goodwin", new DateOnly(1974,01,30),"(732) 528-6445", CustomerType.Subscriber),
            new Customer(3,"Alexander Griffiths", new DateOnly(1986,09,27),"020 8204 0666", CustomerType.Ordinary),
            new Customer(4,"Gerard Donnelly", new DateOnly(1950,07,12),"(866) 592-2742", CustomerType.Pensioner)
        };

    private static List<Address> BuildAddressList()

        => new List<Address>()
        {
            new Address(1,"35 Gateford Rd","Worksop","United Kingdom"),
            new Address(2,"9080 Dixie Hwy","Louisville","United States"),
            new Address(3, "Hyndburn Rd","Accrington", "United Kingdom"),
            new Address(4,"131 Kennilworth Rd","Marlton","United States")
        };

    private static List<CustomerAccount> BuildAccountList()

        => new List<CustomerAccount>()
        {
            new CustomerAccount(1,10001,null),
            new CustomerAccount(2, 10002,"348886240255193"),
            new CustomerAccount(3, 10003,"378955349175801"),
            new CustomerAccount(4, 10004,"3589106335091920")

        };
    private static List<OrderHistoryView> BuildOrderHistories()

        => new List<OrderHistoryView>()
        {
            new OrderHistoryView(1,new DateOnly(2022,12,15),new DateOnly(2023,07,25),5, 350.25M),
            new OrderHistoryView(2,new DateOnly(2020,03,16), new DateOnly(2023,10,10),43, 2589.99M),
            new OrderHistoryView(3, new DateOnly(2018,04,03), new DateOnly(2023,02,26),30,1005.50M),
            new OrderHistoryView(4, new DateOnly(2010,04,03), new DateOnly(2023,09,07),73,3158.27M),

        };


}