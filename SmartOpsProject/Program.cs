using Microsoft.EntityFrameworkCore;
using SmartOps.Data;
using SmartOps.Services;
using SmartOpsProject.Services;

var builder = WebApplication.CreateBuilder(args);

// Προσθήκη υπηρεσιών στο container
builder.Services.AddControllersWithViews();



// Σύνδεση με τη βάση δεδομένων
builder.Services.AddDbContext<SmartOpsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ServiceService>();
//builder.Services.AddDbContext > ();



var app = builder.Build();

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
