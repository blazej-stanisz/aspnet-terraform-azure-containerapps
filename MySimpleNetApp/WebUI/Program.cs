using Microsoft.EntityFrameworkCore;
using MySimpleNetApp.WebUI.Data;
using MySimpleNetApp.WebUI.Monitoring;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'Default' is missing. Set ConnectionStrings__Default " +
        "(docker compose does it) or appsettings.json for local runs.");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<IDatabaseConnectionChecker, DatabaseConnectionChecker>();
builder.Services.AddSingleton<IMetricsSnapshotProvider, PrometheusMetricsSnapshotProvider>();

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services, app.Logger);

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpMetrics();
app.MapMetrics();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
