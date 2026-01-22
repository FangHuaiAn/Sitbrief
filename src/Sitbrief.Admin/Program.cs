using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using Sitbrief.Admin;
using Sitbrief.Admin.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Configure HttpClient with API base URL
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5167";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Register API client
builder.Services.AddScoped<IApiClient, ApiClient>();

await builder.Build().RunAsync();
