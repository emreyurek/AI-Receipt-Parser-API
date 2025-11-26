using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ReceiptParser.UI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- API ADRESÝ ---
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7117") });

builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
