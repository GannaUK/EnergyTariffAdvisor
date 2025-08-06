using EnergyTariffAdvisor.Models;
using EnergyTariffAdvisor.OctopusApi;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
//builder.Services.AddHttpClient<OctopusTariffService>();
builder.Services.AddHttpClient<IOctopusTariffProvider, OctopusTariffService>();

builder.Services.AddSession();

// Регистрация сервиса для работы с CSV файлами
// added singleton to ensure only one instance is used throughout the app
builder.Services.AddSingleton<HouseholdSurveyCsvService>();


// без этого путает точки с запятыми в числах
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var supportedCultures = new[] { new CultureInfo("en-UK") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-UK"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
