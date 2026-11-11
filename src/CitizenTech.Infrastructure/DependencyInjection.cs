using CitizenTech.Application.Interfaces;
using CitizenTech.Application.Regras;
using CitizenTech.Application.Servicos;
using CitizenTech.Domain.Interfaces;
using CitizenTech.Infrastructure.Persistencia;
using CitizenTech.Infrastructure.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CitizenTech.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra banco, repositórios, regras e serviços de aplicação (usado pela API e pelo app Windows Forms).</summary>
    public static IServiceCollection AddCitizenTech(this IServiceCollection services, IConfiguration configuracao)
    {
        var caminho = CaminhoBanco.Resolver(configuracao);
        services.AddSingleton(new InformacoesBanco(caminho));
        services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={caminho}"));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IEstabelecimentoRepository, EstabelecimentoRepositorio>();
        services.AddScoped<IAvaliacaoRepository, AvaliacaoRepositorio>();
        services.AddScoped<IDiagnosticoRepository, DiagnosticoRepositorio>();
        services.AddScoped<IParceiroRepository, ParceiroRepositorio>();
        services.AddScoped<IEncaminhamentoRepository, EncaminhamentoRepositorio>();
        services.AddScoped<IPrevisaoRiscoRepository, PrevisaoRiscoRepositorio>();

        foreach (var regra in RegrasPadrao.ObterTodas())
            services.AddSingleton<IRegraDiagnostico>(regra);

        services.AddScoped<IEstabelecimentoService, EstabelecimentoService>();
        services.AddScoped<IAvaliacaoService, AvaliacaoService>();
        services.AddScoped<IDiagnosticoService, DiagnosticoService>();
        services.AddScoped<IParceiroService, ParceiroService>();
        services.AddScoped<IEncaminhamentoService, EncaminhamentoService>();
        services.AddScoped<IRiscoService, RiscoService>();
        return services;
    }

    /// <summary>Cria o banco (se não existir) e popula os parceiros de exemplo na primeira execução.</summary>
    public static async Task InicializarBancoAsync(this IServiceProvider provider)
    {
        using var escopo = provider.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var parceiros = escopo.ServiceProvider.GetRequiredService<IParceiroRepository>();
        if (await parceiros.ContarAsync() == 0)
        {
            foreach (var parceiro in ParceirosIniciais.Criar()) await parceiros.AdicionarAsync(parceiro);
            await db.SaveChangesAsync();
        }
    }
}
