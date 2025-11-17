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

        // Top 5 Πελάτες (Value = ποσό πωλήσεων)
        public List<NameValue> TopCustomers { get; set; } = new();

        // Πωλήσεις ανά μήνα (ποσά)
        public decimal[] SalesByMonth { get; set; } = new decimal[12];

        // ⭐ ΝΕΟ: Top 5 είδη (Value = συνολική ποσότητα)
        public List<NameValue> TopItems { get; set; } = new();
    }
}
