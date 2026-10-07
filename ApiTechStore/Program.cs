using System.Reflection;
using ApiTechStore.Data;
using ApiTechStore.Infrastructure;
using ApiTechStore.Services;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Azure Key Vault
// Quando "KeyVault:Uri" está configurado (no App Service: KeyVault__Uri), os
// segredos do cofre entram na configuração. O segredo
// "ConnectionStrings--SqlTechStore" vira "ConnectionStrings:SqlTechStore".
// A autenticação usa a identidade gerenciada do App Service; na máquina local,
// a conta do Visual Studio / Azure CLI.
// ---------------------------------------------------------------------------
var keyVaultUri = builder.Configuration["KeyVault:Uri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}

// ---------------------------------------------------------------------------
// Azure Monitor / Application Insights
// Ativado somente quando a connection string do Application Insights existe,
// para a API rodar localmente sem depender do Azure.
// ---------------------------------------------------------------------------
if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

// ---------------------------------------------------------------------------
// Banco de dados (Entity Framework Core + SQL Server)
// EnableRetryOnFailure repete a operação em falhas transitórias, como a
// retomada de um Azure SQL serverless que estava pausado.
// ---------------------------------------------------------------------------
builder.Services.AddDbContext<TechStoreDbContext>((serviceProvider, options) =>
{
    var connectionString = serviceProvider
        .GetRequiredService<IConfiguration>()
        .GetConnectionString("SqlTechStore");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "A string de conexão 'SqlTechStore' não foi configurada. " +
            "No Azure, crie o segredo 'ConnectionStrings--SqlTechStore' no Key Vault e configure 'KeyVault__Uri' no App Service. " +
            "Localmente, use: dotnet user-secrets set \"ConnectionStrings:SqlTechStore\" \"<string de conexão>\".");
    }

    options.UseSqlServer(connectionString, sqlServer => sqlServer.EnableRetryOnFailure());
});

// ---------------------------------------------------------------------------
// Aplicação
// ---------------------------------------------------------------------------
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddControllers();

// Tratamento global de erros + respostas de erro no padrão Problem Details.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

// ---------------------------------------------------------------------------
// CORS
// No Azure o CORS é liberado no próprio App Service (menu API > CORS), que
// responde antes da aplicação. Aqui ele só é ligado quando há origens em
// "Cors:AllowedOrigins" (appsettings.Development.json), para o Angular local.
// ---------------------------------------------------------------------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));
}

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Swagger fica disponível também no Azure, para documentar e testar a API publicada.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

if (allowedOrigins.Length > 0)
{
    app.UseCors();
}

app.MapControllers();

app.Run();
