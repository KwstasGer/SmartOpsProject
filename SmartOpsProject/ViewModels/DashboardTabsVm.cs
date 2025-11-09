using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartOps.ViewModels
{
    public record NameValue(string Name, decimal Value);

    public class CustomerBalanceDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public decimal OpenAmount { get; set; }
        public DateTime? OldestDueDate { get; set; }
    }

    public class DashboardTabsVm
    {
        public int Year { get; set; }
        public List<NameValue> TopCustomers { get; set; } = new();
        public decimal[] SalesByMonth { get; set; } = new decimal[12];
        public List<CustomerBalanceDto> Receivables { get; set; } = new();
        public decimal ReceivablesTotal => Receivables.Sum(x => x.OpenAmount);
    }
}
