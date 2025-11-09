using Microsoft.EntityFrameworkCore;
using SmartOps.Data;
using SmartOps.Services;
using SmartOpsProject.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();  // <-- ΝΕΟ
builder.Services.AddSession();

// 🔹 Database connection
builder.Services.AddDbContext<SmartOpsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 Custom services
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<ServiceService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<InvoiceService>();     
builder.Services.AddScoped<IDashboardService, DashboardService>();



var app = builder.Build();

// 🔹 Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Πρέπει να ενεργοποιηθεί πριν την πλοήγηση
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
