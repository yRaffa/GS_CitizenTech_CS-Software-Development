using System.Text.Json.Serialization;
using CitizenTech.Api.Middleware;
using CitizenTech.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); // enums como texto
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Banco (EF Core + SQLite), repositórios, regras de diagnóstico e serviços
builder.Services.AddCitizenTech(builder.Configuration);

var app = builder.Build();

await app.Services.InicializarBancoAsync();

app.UseMiddleware<TratamentoDeErrosMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapControllers();

app.Run();
