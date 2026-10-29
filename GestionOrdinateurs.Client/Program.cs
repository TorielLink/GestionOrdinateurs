using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GestionOrdinateurs.Client;
using GestionOrdinateurs.Client.Utils;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Stockage de session (là où on garde le token)
builder.Services.AddBlazoredSessionStorage();

// Authentification : le provider est enregistré sous son type ET comme AuthenticationStateProvider,
// pour qu'on puisse l'injecter directement (Login, NavMenu) tout en restant la même instance
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());

// HttpClient qui passe par la Gateway et ajoute le token via JwtHandler
builder.Services.AddScoped<JwtHandler>();
builder.Services.AddHttpClient("Gateway", client =>
        client.BaseAddress = new Uri("http://localhost:5010/"))
    .AddHttpMessageHandler<JwtHandler>();
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Gateway"));

await builder.Build().RunAsync();