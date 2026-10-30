using AuthentificationApi;

var builder = WebApplication.CreateBuilder(args);

// Enregistrement des contrôleurs
builder.Services.AddControllers();

// Swagger (package Swashbuckle.AspNetCore)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Pour autoriser les appels de nos API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "http://localhost:5114") 
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// client HTTP vers Keycloak
builder.Services.AddHttpClient<KeycloakAuthService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Keycloak:BaseUrl"]
        ?? throw new InvalidOperationException("Keycloak:BaseUrl manquant dans appsettings.json"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");

app.MapControllers();

app.Run();