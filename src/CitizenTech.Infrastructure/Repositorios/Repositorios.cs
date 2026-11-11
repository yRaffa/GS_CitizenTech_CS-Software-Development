using System.Linq.Expressions;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Interfaces;
using CitizenTech.Domain.Models;
using CitizenTech.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace CitizenTech.Infrastructure.Repositorios;

public class EstabelecimentoRepositorio : IEstabelecimentoRepository
{
    private readonly AppDbContext _db;
    public EstabelecimentoRepositorio(AppDbContext db) => _db = db;

    public Task<Estabelecimento?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Estabelecimentos.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<ResultadoPaginado<Estabelecimento>> ListarAsync(string? uf, TipoEstabelecimento? tipo, int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        IQueryable<Estabelecimento> consulta = _db.Estabelecimentos.AsNoTracking();
        if (uf is not null) consulta = consulta.Where(e => e.Endereco.Uf == uf);
        if (tipo is not null) { var t = tipo.Value; consulta = consulta.Where(e => e.Tipo == t); }

        var total = await consulta.CountAsync(ct);
        var itens = await consulta.OrderBy(e => e.Id).Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToListAsync(ct);
        return new ResultadoPaginado<Estabelecimento>(itens, total, pagina, tamanhoPagina);
    }

    public async Task AdicionarAsync(Estabelecimento estabelecimento, CancellationToken ct = default) =>
        await _db.Estabelecimentos.AddAsync(estabelecimento, ct);

    public void Remover(Estabelecimento estabelecimento) => _db.Estabelecimentos.Remove(estabelecimento);
}

public class AvaliacaoRepositorio : IAvaliacaoRepository
{
    private readonly AppDbContext _db;
    public AvaliacaoRepositorio(AppDbContext db) => _db = db;

    public Task<Avaliacao?> ObterCompletaAsync(int id, CancellationToken ct = default) =>
        _db.Avaliacoes
            .Include(a => a.Respostas)
            .Include(a => a.Medicoes)
            .Include(a => a.Estabelecimento)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Avaliacao>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default) =>
        await _db.Avaliacoes.AsNoTracking()
            .Include(a => a.Respostas)
            .Include(a => a.Medicoes)
            .Where(a => a.EstabelecimentoId == estabelecimentoId)
            .OrderByDescending(a => a.DataAvaliacao)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Avaliacao avaliacao, CancellationToken ct = default) =>
        await _db.Avaliacoes.AddAsync(avaliacao, ct);
}

public class DiagnosticoRepositorio : IDiagnosticoRepository
{
    private readonly AppDbContext _db;
    public DiagnosticoRepositorio(AppDbContext db) => _db = db;

    public Task<Diagnostico?> ObterCompletoAsync(int id, CancellationToken ct = default) =>
        _db.Diagnosticos
            .Include(d => d.Barreiras)
            .Include(d => d.Avaliacao).ThenInclude(a => a!.Estabelecimento)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<Diagnostico>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default) =>
        await _db.Diagnosticos.AsNoTracking()
            .Include(d => d.Barreiras)
            .Include(d => d.Avaliacao).ThenInclude(a => a!.Estabelecimento)
            .Where(d => d.Avaliacao!.EstabelecimentoId == estabelecimentoId)
            .OrderByDescending(d => d.DataGeracao)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Diagnostico diagnostico, CancellationToken ct = default) =>
        await _db.Diagnosticos.AddAsync(diagnostico, ct);
}

public class ParceiroRepositorio : IParceiroRepository
{
    private readonly AppDbContext _db;
    public ParceiroRepositorio(AppDbContext db) => _db = db;

    public Task<Parceiro?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Parceiros.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Parceiro>> ListarAsync(Especialidade? especialidades, string? uf, CancellationToken ct = default)
    {
        IQueryable<Parceiro> consulta = _db.Parceiros.AsNoTracking().Where(p => p.Ativo);

        if (especialidades is not null)
        {
            var esp = especialidades.Value;
            consulta = consulta.Where(p => (p.Especialidades & esp) != Especialidade.Nenhuma);
        }
        if (uf is not null)
        {
            var chave = Parceiro.ChaveUf(uf);
            consulta = consulta.Where(p => p.UfsAtendidas == Parceiro.TodoBrasil || p.UfsAtendidas.Contains(chave));
        }
        return await consulta.OrderBy(p => p.Nome).ToListAsync(ct);
    }

    public Task<int> ContarAsync(CancellationToken ct = default) => _db.Parceiros.CountAsync(ct);

    public async Task AdicionarAsync(Parceiro parceiro, CancellationToken ct = default) =>
        await _db.Parceiros.AddAsync(parceiro, ct);
}

public class EncaminhamentoRepositorio : IEncaminhamentoRepository
{
    private readonly AppDbContext _db;
    public EncaminhamentoRepositorio(AppDbContext db) => _db = db;

    public Task<Encaminhamento?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Encaminhamentos.Include(e => e.Parceiro).FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Encaminhamento>> ListarAsync(StatusEncaminhamento? status, CancellationToken ct = default)
    {
        IQueryable<Encaminhamento> consulta = _db.Encaminhamentos.AsNoTracking().Include(e => e.Parceiro);
        if (status is not null) { var s = status.Value; consulta = consulta.Where(e => e.Status == s); }
        return await consulta.OrderByDescending(e => e.DataCriacao).ToListAsync(ct);
    }

    public Task<bool> ExisteAsync(int diagnosticoId, int parceiroId, CancellationToken ct = default) =>
        _db.Encaminhamentos.AnyAsync(e => e.DiagnosticoId == diagnosticoId && e.ParceiroId == parceiroId, ct);

    public async Task AdicionarAsync(Encaminhamento encaminhamento, CancellationToken ct = default) =>
        await _db.Encaminhamentos.AddAsync(encaminhamento, ct);
}

public class PrevisaoRiscoRepositorio : IPrevisaoRiscoRepository
{
    private readonly AppDbContext _db;
    public PrevisaoRiscoRepositorio(AppDbContext db) => _db = db;

    public Task<PrevisaoRisco?> ObterPorInepAsync(long codigoInep, CancellationToken ct = default) =>
        _db.Previsoes.AsNoTracking().FirstOrDefaultAsync(p => p.CodigoInep == codigoInep, ct);

    public async Task<ResumoRisco?> ObterResumoUfAsync(string uf, CancellationToken ct = default) =>
        (await AgruparAsync(_db.Previsoes.AsNoTracking().Where(p => p.Uf == uf), p => p.Uf, ct)).FirstOrDefault();

    public Task<IReadOnlyList<ResumoRisco>> ResumoPorRegiaoAsync(CancellationToken ct = default) =>
        AgruparAsync(_db.Previsoes.AsNoTracking(), p => p.Regiao, ct);

    public Task<IReadOnlyList<ResumoRisco>> ResumoPorUfAsync(string? regiao, CancellationToken ct = default)
    {
        IQueryable<PrevisaoRisco> consulta = _db.Previsoes.AsNoTracking();
        if (regiao is not null) consulta = consulta.Where(p => p.Regiao == regiao);
        return AgruparAsync(consulta, p => p.Uf, ct);
    }

    public Task<int> ContarAsync(CancellationToken ct = default) => _db.Previsoes.CountAsync(ct);

    public async Task LimparAsync(CancellationToken ct = default) =>
        await _db.Previsoes.ExecuteDeleteAsync(ct);

    public async Task InserirLoteAsync(IReadOnlyList<PrevisaoRisco> lote, CancellationToken ct = default)
    {
        await _db.Previsoes.AddRangeAsync(lote, ct);
        await _db.SaveChangesAsync(ct);
        _db.ChangeTracker.Clear(); // evita acumular milhares de entidades rastreadas na importação
    }

    /// <summary>Agrupamento feito no banco (GROUP BY) com LINQ.</summary>
    private static async Task<IReadOnlyList<ResumoRisco>> AgruparAsync(
        IQueryable<PrevisaoRisco> consulta, Expression<Func<PrevisaoRisco, string>> chave, CancellationToken ct)
    {
        var linhas = await consulta
            .GroupBy(chave)
            .Select(g => new
            {
                g.Key,
                Total = g.Count(),
                SemRampa = g.Count(p => p.SemRampaDeclarado),
                Media = g.Average(p => p.ProbSemRampa),
                Alto = g.Count(p => p.Faixa == FaixaRisco.Alta)
            })
            .ToListAsync(ct);

        return linhas
            .Select(l => new ResumoRisco(l.Key, l.Total, 100.0 * l.SemRampa / l.Total, l.Media, l.Alto))
            .OrderBy(r => r.Chave)
            .ToList();
    }
}
