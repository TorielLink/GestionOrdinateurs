using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Enregistrement du Parc d'Ordinateurs
builder.Services.AddSingleton<ParcInitializer>();

// Enregistrement des Controllers
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // coupe les boucles Reparation <-> Ordinateur au lieu de planter
        o.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Enregistrement des Services
builder.Services.AddScoped<IOrdinateurService, OrdinateurService>();
builder.Services.AddScoped<IReparationService, ReparationService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IUtilisateurService, UtilisateurService>();
builder.Services.AddSingleton<JournalService>();

// Enregistrement du Context
// -> ajout d'une fabrique pour les traitements en parallèle (méthode SimulerReparation)
builder.Services.AddDbContextFactory<ParcInformatiqueContext>();
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<ParcInformatiqueContext>>()
        .CreateDbContext()); // pour que OrdinateurService continue de fonctionner avec le scope classique

// OpenAPI
builder.Services.AddOpenApi();

// Exposition de Swagger avec JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestionOrdinateurs",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez : Bearer {token}"
    });
    
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// Configuration JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Utilisateur utilisant un Token Valide --- {context.Principal?.Identity?.Name ?? "Anonymous"}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("Token Invalide");
            return Task.CompletedTask;
        }
    };
});


// Pour autoriser les appels de nos API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5114") 
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParcInformatiqueContext>();

    db.Database.Migrate();

    // on ajoute les utilisateurs de départ seulement si la table est vide
    if (!db.Utilisateurs.Any())
    {
        db.Utilisateurs.AddRange(
            new Utilisateur { Login = "user1", Role = RoleEnum.Admin },
            new Utilisateur { Login = "user2", Role = RoleEnum.Gestionnaire },
            new Utilisateur { Login = "user3", Role = RoleEnum.Gestionnaire }
        );

        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");

app.UseAuthentication(); // lit et valide le token
app.UseAuthorization(); // applique [Authorize]

app.MapControllers();

app.Run();