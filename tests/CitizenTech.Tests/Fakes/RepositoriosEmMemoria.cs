using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Interfaces;
using CitizenTech.Domain.Models;

namespace CitizenTech.Tests.Fakes;

/// <summary>"Banco" compartilhado pelos repositórios falsos (usado nos testes de serviços).</summary>
public class BancoEmMemoria
{
    public List<Estabelecimento> Estabelecimentos { get; } = new();
    public List<Avaliacao> Avaliacoes { get; } = new();
    public List<Diagnostico> Diagnosticos { get; } = new();
    public List<Parceiro> Parceiros { get; } = new();
    public List<Encaminhamento> Encaminhamentos { get; } = new();
    public List<PrevisaoRisco> Previsoes { get; } = new();
    private readonly Dictionary<Type, int> _contadores = new();

    /// <summary>Simula a geração de Id do banco: uma sequência por tipo de entidade (os setters são privados no domínio).</summary>
    public void GerarId(object entidade)
    {
        var tipo = entidade.GetType();
        var prop = tipo.GetProperty("Id")!;
        if ((int)prop.GetValue(entidade)! != 0) return;
        _contadores[tipo] = _contadores.GetValueOrDefault(tipo) + 1;
        prop.SetValue(entidade, _contadores[tipo]);
    }

    public static void Definir(object alvo, string propriedade, object? valor) =>
        alvo.GetType().GetProperty(propriedade)!.SetValue(alvo, valor);
}

public class UnitOfWorkEmMemoria : IUnitOfWork
{
    public Task<int> SalvarAsync(CancellationToken ct = default) => Task.FromResult(0);
}

public class EstabelecimentoRepositorioEmMemoria : IEstabelecimentoRepository
{
    private readonly BancoEmMemoria _b;
    public EstabelecimentoRepositorioEmMemoria(BancoEmMemoria banco) => _b = banco;

    public Task<Estabelecimento?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(_b.Estabelecimentos.FirstOrDefault(e => e.Id == id));

    public Task<ResultadoPaginado<Estabelecimento>> ListarAsync(string? uf, TipoEstabelecimento? tipo, int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        var q = _b.Estabelecimentos.AsEnumerable();
        if (uf is not null) q = q.Where(e => e.Endereco.Uf == uf);
        if (tipo is not null) q = q.Where(e => e.Tipo == tipo);
        var lista = q.OrderBy(e => e.Id).ToList();
        var itens = lista.Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToList();
        return Task.FromResult(new ResultadoPaginado<Estabelecimento>(itens, lista.Count, pagina, tamanhoPagina));
    }

    public Task AdicionarAsync(Estabelecimento e, CancellationToken ct = default)
    {
        _b.GerarId(e);
        _b.Estabelecimentos.Add(e);
        return Task.CompletedTask;
    }

    public void Remover(Estabelecimento e) => _b.Estabelecimentos.Remove(e);
}

public class AvaliacaoRepositorioEmMemoria : IAvaliacaoRepository
{
    private readonly BancoEmMemoria _b;
    public AvaliacaoRepositorioEmMemoria(BancoEmMemoria banco) => _b = banco;

    public Task<Avaliacao?> ObterCompletaAsync(int id, CancellationToken ct = default)
    {
        var a = _b.Avaliacoes.FirstOrDefault(x => x.Id == id);
        if (a is not null) BancoEmMemoria.Definir(a, "Estabelecimento", _b.Estabelecimentos.FirstOrDefault(e => e.Id == a.EstabelecimentoId));
        return Task.FromResult(a);
    }

    public Task<IReadOnlyList<Avaliacao>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Avaliacao>>(_b.Avaliacoes.Where(a => a.EstabelecimentoId == estabelecimentoId).ToList());

    public Task AdicionarAsync(Avaliacao a, CancellationToken ct = default)
    {
        _b.GerarId(a);
        foreach (var r in a.Respostas) _b.GerarId(r);
        foreach (var m in a.Medicoes) _b.GerarId(m);
        _b.Avaliacoes.Add(a);
        return Task.CompletedTask;
    }
}

public class DiagnosticoRepositorioEmMemoria : IDiagnosticoRepository
{
    private readonly BancoEmMemoria _b;
    public DiagnosticoRepositorioEmMemoria(BancoEmMemoria banco) => _b = banco;

    private Diagnostico Completar(Diagnostico d)
    {
        var av = _b.Avaliacoes.FirstOrDefault(a => a.Id == d.AvaliacaoId);
        if (av is not null)
        {
            BancoEmMemoria.Definir(av, "Estabelecimento", _b.Estabelecimentos.FirstOrDefault(e => e.Id == av.EstabelecimentoId));
            BancoEmMemoria.Definir(d, "Avaliacao", av);
        }
        return d;
    }

    public Task<Diagnostico?> ObterCompletoAsync(int id, CancellationToken ct = default)
    {
        var d = _b.Diagnosticos.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(d is null ? null : Completar(d));
    }

    public Task<IReadOnlyList<Diagnostico>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default)
    {
        var ids = _b.Avaliacoes.Where(a => a.EstabelecimentoId == estabelecimentoId).Select(a => a.Id).ToHashSet();
        var lista = _b.Diagnosticos.Where(d => ids.Contains(d.AvaliacaoId)).Select(Completar).ToList();
        return Task.FromResult<IReadOnlyList<Diagnostico>>(lista);
    }

    public Task AdicionarAsync(Diagnostico d, CancellationToken ct = default)
    {
        _b.GerarId(d);
        foreach (var br in d.Barreiras) _b.GerarId(br);
        _b.Diagnosticos.Add(d);
        return Task.CompletedTask;
    }
}

public class ParceiroRepositorioEmMemoria : IParceiroRepository
{
    private readonly BancoEmMemoria _b;
    public ParceiroRepositorioEmMemoria(BancoEmMemoria banco) => _b = banco;

    public Task<Parceiro?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(_b.Parceiros.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Parceiro>> ListarAsync(Especialidade? especialidades, string? uf, CancellationToken ct = default)
    {
        var lista = _b.Parceiros
            .Where(p => p.Ativo)
            .Where(p => especialidades is null || (p.Especialidades & especialidades.Value) != 0)
            .Where(p => uf is null || p.Atende(uf))
            .OrderBy(p => p.Nome).ToList();
        return Task.FromResult<IReadOnlyList<Parceiro>>(lista);
    }

    public Task<int> ContarAsync(CancellationToken ct = default) => Task.FromResult(_b.Parceiros.Count);

    public Task AdicionarAsync(Parceiro p, CancellationToken ct = default)
    {
        _b.GerarId(p);
        _b.Parceiros.Add(p);
        return Task.CompletedTask;
    }
}

public class EncaminhamentoRepositorioEmMemoria : IEncaminhamentoRepository
{
    private readonly BancoEmMemoria _b;
    public EncaminhamentoRepositorioEmMemoria(BancoEmMemoria banco) => _b = banco;

    private Encaminhamento Completar(Encaminhamento e)
    {
        BancoEmMemoria.Definir(e, "Parceiro", _b.Parceiros.FirstOrDefault(p => p.Id == e.ParceiroId));
        return e;
    }

    public Task<Encaminhamento?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var e = _b.Encaminhamentos.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(e is null ? null : Completar(e));
    }

    public Task<IReadOnlyList<Encaminhamento>> ListarAsync(StatusEncaminhamento? status, CancellationToken ct = default)
    {
        var lista = _b.Encaminhamentos.Where(e => status is null || e.Status == status).Select(Completar).OrderBy(e => e.Id).ToList();
        return Task.FromResult<IReadOnlyList<Encaminhamento>>(lista);
    }

    public Task<bool> ExisteAsync(int diagnosticoId, int parceiroId, CancellationToken ct = default) =>
        Task.FromResult(_b.Encaminhamentos.Any(e => e.DiagnosticoId == diagnosticoId && e.ParceiroId == parceiroId));

    public Task AdicionarAsync(Encaminhamento e, CancellationToken ct = default)
    {
        _b.GerarId(e);
        _b.Encaminhamentos.Add(e);
        return Task.CompletedTask;
    }
}

public class PrevisaoRiscoRepositorioEmMemoria : IPrevisaoRiscoRepository
{
    private readonly BancoEmMemoria _b;
    public PrevisaoRiscoRepositorioEmMemoria(BancoEmMemoria banco) => _b = banco;

    private static ResumoRisco Resumir(string chave, IReadOnlyCollection<PrevisaoRisco> grupo) => new(
        chave, grupo.Count, 100.0 * grupo.Count(p => p.SemRampaDeclarado) / grupo.Count,
        grupo.Average(p => p.ProbSemRampa), grupo.Count(p => p.Faixa == FaixaRisco.Alta));

    public Task<PrevisaoRisco?> ObterPorInepAsync(long codigoInep, CancellationToken ct = default) =>
        Task.FromResult(_b.Previsoes.FirstOrDefault(p => p.CodigoInep == codigoInep));

    public Task<ResumoRisco?> ObterResumoUfAsync(string uf, CancellationToken ct = default)
    {
        var grupo = _b.Previsoes.Where(p => p.Uf == uf).ToList();
        return Task.FromResult(grupo.Count == 0 ? null : Resumir(uf, grupo));
    }

    public Task<IReadOnlyList<ResumoRisco>> ResumoPorRegiaoAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ResumoRisco>>(_b.Previsoes.GroupBy(p => p.Regiao)
            .Select(g => Resumir(g.Key, g.ToList())).OrderBy(r => r.Chave).ToList());

    public Task<IReadOnlyList<ResumoRisco>> ResumoPorUfAsync(string? regiao, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ResumoRisco>>(_b.Previsoes.Where(p => regiao == null || p.Regiao == regiao)
            .GroupBy(p => p.Uf).Select(g => Resumir(g.Key, g.ToList())).OrderBy(r => r.Chave).ToList());

    public Task<int> ContarAsync(CancellationToken ct = default) => Task.FromResult(_b.Previsoes.Count);

    public Task LimparAsync(CancellationToken ct = default)
    {
        _b.Previsoes.Clear();
        return Task.CompletedTask;
    }

    public Task InserirLoteAsync(IReadOnlyList<PrevisaoRisco> lote, CancellationToken ct = default)
    {
        foreach (var p in lote) { _b.GerarId(p); _b.Previsoes.Add(p); }
        return Task.CompletedTask;
    }
}
