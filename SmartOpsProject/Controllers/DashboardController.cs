using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartOps.Services;          // <= για TopItemRow & IDashboardService
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartOps.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dash;

        public DashboardController(IDashboardService dash)
        {
            _dash = dash;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == 0)
                return RedirectToAction("Login", "Account");

            var year = DateTime.Today.Year;

            // Top 5 πελάτες σε πωλήσεις
            var topCustomers = await _dash.GetTopCustomersAsync(CurrentUserId, 5);

            // Πωλήσεις ανά μήνα
            var salesByMonth = await _dash.GetSalesByMonthAsync(CurrentUserId, year);

            // Top 5 είδη σε ποσότητα
            var topItems = await _dash.GetTopItemsAsync(CurrentUserId, year, 5);

            // Δημιουργία του ViewModel που θα περάσει στο Dashboard view
            var vm = new DashboardVm
            {
                Year = year, // Το έτος που ζήτησε ο χρήστης για το dashboard
                TopCustomers = topCustomers, // Λίστα με τους καλύτερους πελάτες (βάσει πωλήσεων)
                SalesByMonth = salesByMonth, // Συγκεντρωτικές πωλήσεις ανά μήνα
                TopItems = topItems  // Προϊόντα με τις περισσότερες πωλήσεις
            };

            return View(vm);
        }
    }

    public class DashboardVm
    {
        public int Year { get; set; }

        // Top πελάτες (ίδιο tuple με πριν)
        public List<(string CustomerName, decimal Amount)> TopCustomers { get; set; }
            = new();

        // Πωλήσεις ανά μήνα
        public decimal[] SalesByMonth { get; set; }
            = new decimal[12];

        // ⭐ Top 5 είδη – χρησιμοποιούμε την κλάση από SmartOps.Services
        public List<TopItemRow> TopItems { get; set; }
            = new();
    }
}
