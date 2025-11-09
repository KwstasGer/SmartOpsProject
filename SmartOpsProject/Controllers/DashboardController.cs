using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartOps.Services;

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
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var year = DateTime.Today.Year;

            var top = await _dash.GetTopCustomersAsync(CurrentUserId, 5);
            var sales = await _dash.GetSalesByMonthAsync(CurrentUserId, year);
            var open = await _dash.GetOpenBalancesAsync(CurrentUserId);

            var vm = new DashboardVm
            {
                Year = year,
                TopCustomers = top,
                SalesByMonth = sales,
                OpenBalances = open
            };

            return View(vm);
        }
    }

    public class DashboardVm
    {
        public int Year { get; set; }
        public List<(string CustomerName, decimal Amount)> TopCustomers { get; set; } = new();
        public decimal[] SalesByMonth { get; set; } = new decimal[12];
        public List<(string CustomerName, decimal OpenAmount)> OpenBalances { get; set; } = new();
    }
}
