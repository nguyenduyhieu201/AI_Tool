using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ChatGPTTool.Data;
using ChatGPTTool.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<IIPService, IPService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure HttpClient with base address for API calls
builder.Services.AddHttpClient("API", client =>
{
    // ✅ SỬA: Trong Docker environment, luôn sử dụng nginx gateway
    var apiBaseUrl = "http://nginx:80";  // Sử dụng nginx gateway

    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
