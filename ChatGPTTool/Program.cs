using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using ChatGPTTool.Data;
using ChatGPTTool.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers(); // Thêm controllers
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<IIPService, IPService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add Google OAuth Service
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();



// Add HttpInterceptor
builder.Services.AddTransient<HttpInterceptor>();

// Add HttpClient for API calls with interceptor
builder.Services.AddHttpClient("API", client =>
{
    var inContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    var apiBaseUrl = inContainer ? "http://nginx" : "http://localhost:5001"; // Users.API port
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<HttpInterceptor>(); // Thêm interceptor

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

app.MapControllers(); // Thêm route cho controllers
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
