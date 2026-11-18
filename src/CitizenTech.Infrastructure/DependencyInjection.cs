using System.Text.Json;
using CitizenTech.Application.Equipe;
using CitizenTech.Application.Interfaces;
using CitizenTech.Application.Regras;
using CitizenTech.Application.Servicos;
using CitizenTech.Domain.Interfaces;
using CitizenTech.Infrastructure.Persistencia;
using CitizenTech.Infrastructure.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CitizenTech.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra banco (EF Core + SQL Server), repositórios, regras e serviços de aplicação.
    /// A string de conexão vem de ConnectionStrings:CitizenTech (sem senha no ZIP: use autenticação do Windows
    /// ou User Secrets / variável de ambiente para credenciais).
    /// </summary>
    public static IServiceCollection AddCitizenTech(this IServiceCollection services, IConfiguration configuracao)
    {
        var conexao = configuracao.GetConnectionString("CitizenTech")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:CitizenTech (veja o README).");
        services.AddDbContext<AppDbContext>(o => o.UseSqlServer(conexao));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddSingleton(CarregarEquipe(configuracao));

        services.AddScoped<IEstabelecimentoRepository, EstabelecimentoRepositorio>();
        services.AddScoped<IAvaliacaoRepository, AvaliacaoRepositorio>();
        services.AddScoped<IDiagnosticoRepository, DiagnosticoRepositorio>();
        services.AddScoped<IParceiroRepository, ParceiroRepositorio>();
        services.AddScoped<IEncaminhamentoRepository, EncaminhamentoRepositorio>();
        services.AddScoped<IPrevisaoRiscoRepository, PrevisaoRiscoRepositorio>();
        services.AddScoped<IImagemRepository, ImagemRepositorio>();
        services.AddScoped<IReservaRepository, ReservaRepositorio>();

        foreach (var regra in RegrasPadrao.ObterTodas())
            services.AddSingleton<IRegraDiagnostico>(regra);

        services.AddScoped<IEstabelecimentoService, EstabelecimentoService>();
        services.AddScoped<IAvaliacaoService, AvaliacaoService>();
        services.AddScoped<IDiagnosticoService, DiagnosticoService>();
        services.AddScoped<IParceiroService, ParceiroService>();
        services.AddScoped<IEncaminhamentoService, EncaminhamentoService>();
        services.AddScoped<IRiscoService, RiscoService>();
        services.AddScoped<IImagemService, ImagemService>();
        services.AddScoped<IReservaService>(sp => new ReservaService(
            sp.GetRequiredService<IParceiroRepository>(), sp.GetRequiredService<IDiagnosticoRepository>(),
            sp.GetRequiredService<IEncaminhamentoRepository>(), sp.GetRequiredService<IReservaRepository>(), sp.GetRequiredService<IUnitOfWork>(),
            sp.GetRequiredService<EquipeInfo>(), !bool.TryParse(configuracao["Transacao:PermitirFalhaSimulada"], out var permitir) || permitir));
        return services;
    }

    /// <summary>Lê o equipe.json (CitizenTech, turma, RM do líder e integrantes reais). Não é autenticação.</summary>
    private static EquipeInfo CarregarEquipe(IConfiguration configuracao)
    {
        var caminho = configuracao["Equipe:Arquivo"];
        if (string.IsNullOrWhiteSpace(caminho)) caminho = Path.Combine(AppContext.BaseDirectory, "equipe.json");
        if (!File.Exists(caminho))
            throw new FileNotFoundException($"equipe.json não encontrado em '{caminho}'. Veja o README.", caminho);

        var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var equipe = JsonSerializer.Deserialize<EquipeInfo>(File.ReadAllText(caminho), opcoes)
            ?? throw new InvalidOperationException("equipe.json inválido.");
        if (equipe.Integrantes is null || equipe.Integrantes.Count == 0)
            throw new InvalidOperationException("equipe.json precisa ter ao menos um integrante.");
        return equipe;
    }

    /// <summary>
    /// Verifica a conexão com o banco na inicialização. A estrutura NÃO é criada pela API:
    /// ela vem dos scripts sql/01_Criar_Estrutura.sql e sql/02_Carga_Ficticia.sql.
    /// </summary>
    public static async Task VerificarBancoAsync(this IServiceProvider provider)
    {
        using var escopo = provider.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
        var log = escopo.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Banco");
        if (await db.Database.CanConnectAsync())
            log.LogInformation("Conexão com o SQL Server verificada.");
        else
            log.LogError("Não foi possível conectar ao SQL Server. Confira ConnectionStrings:CitizenTech e execute os scripts em sql/.");
    }
}
