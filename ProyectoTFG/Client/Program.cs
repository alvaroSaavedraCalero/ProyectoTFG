using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProyectoTFG.Client;
using ProyectoTFG.Client.Models.Interfaces;
using ProyectoTFG.Client.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Inyeccion de servicios
builder.Services.AddScoped<IRestService, RestService>();
builder.Services.AddScoped<IStorageService, SubjectStorageService>();

await builder.Build().RunAsync();
