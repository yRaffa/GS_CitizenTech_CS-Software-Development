using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CitizenTech.Infrastructure.Persistencia;

/// <summary>Contexto do EF Core. Também funciona como Unit of Work (SalvarAsync).</summary>
public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Estabelecimento> Estabelecimentos => Set<Estabelecimento>();
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
    public DbSet<RespostaItem> Respostas => Set<RespostaItem>();
    public DbSet<Medicao> Medicoes => Set<Medicao>();
    public DbSet<Diagnostico> Diagnosticos => Set<Diagnostico>();
    public DbSet<Barreira> Barreiras => Set<Barreira>();
    public DbSet<Parceiro> Parceiros => Set<Parceiro>();
    public DbSet<Encaminhamento> Encaminhamentos => Set<Encaminhamento>();
    public DbSet<PrevisaoRisco> Previsoes => Set<PrevisaoRisco>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Carrega todas as classes IEntityTypeConfiguration<T> deste assembly (arquivo Configuracoes.cs)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public Task<int> SalvarAsync(CancellationToken ct = default) => SaveChangesAsync(ct);
}
