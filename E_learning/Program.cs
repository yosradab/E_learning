using Microsoft.EntityFrameworkCore;
using E_learning.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Correct placement: Configure controllers and JSON serialization at the top level.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Permet la sérialisation des références circulaires.
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();

// Configure la sécurité JWT pour les services de l'API
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Ajoute la configuration de Swagger pour prendre en charge l'authentification par jeton Bearer
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "E-learning API", Version = "v1" });

    // Définit le schéma de sécurité pour Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Entrez 'Bearer' suivi d'un espace et de votre jeton. Exemple: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Indique les exigences de sécurité pour l'API
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Récupération de la chaîne de connexion depuis appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Enregistrement du DbContext pour l'injection de dépendances
builder.Services.AddDbContext<ElearningDbContext>(options =>
    options.UseSqlServer(connectionString));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Active les middlewares d'authentification et d'autorisation
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
