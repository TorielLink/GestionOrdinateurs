var builder = WebApplication.CreateBuilder(args);

// YARP : les routes et destinations sont lues dans appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// redirige les requêtes vers les routes configurées
app.MapReverseProxy();

app.Run();