using System.Globalization;
using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Application.Regras;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Exceptions;
using CitizenTech.Domain.Interfaces;

namespace CitizenTech.Application.Servicos;

/// <summary>
/// Gera a triagem: aplica todas as regras (polimorfismo via IRegraDiagnostico) e enriquece o resultado
/// com o contexto do modelo de ML (previsão da escola e panorama da UF).
/// </summary>
public class DiagnosticoService : IDiagnosticoService
{
    private readonly IAvaliacaoRepository _avaliacoes;
    private readonly IDiagnosticoRepository _diagnosticos;
    private readonly IPrevisaoRiscoRepository _previsoes;
    private readonly IEnumerable<IRegraDiagnostico> _regras;
    private readonly IUnitOfWork _uow;

    public DiagnosticoService(IAvaliacaoRepository avaliacoes, IDiagnosticoRepository diagnosticos,
                              IPrevisaoRiscoRepository previsoes, IEnumerable<IRegraDiagnostico> regras, IUnitOfWork uow)
    {
        _avaliacoes = avaliacoes;
        _diagnosticos = diagnosticos;
        _previsoes = previsoes;
        _regras = regras;
        _uow = uow;
    }

    public async Task<DiagnosticoResponse> GerarAsync(int avaliacaoId, CancellationToken ct = default)
    {
        var avaliacao = await _avaliacoes.ObterCompletaAsync(avaliacaoId, ct)
            ?? throw new NaoEncontradoException(nameof(Avaliacao), avaliacaoId);
        var estabelecimento = avaliacao.Estabelecimento
            ?? throw new DomainException("Avaliação sem estabelecimento associado.");

        var diagnostico = new Diagnostico(avaliacao.Id);
        foreach (var regra in _regras)
            foreach (var barreira in regra.Avaliar(avaliacao))
                diagnostico.AdicionarBarreira(barreira);

        // Integração com a entrega de IA & ML
        if (estabelecimento.CodigoInep is long inep)
        {
            var previsao = await _previsoes.ObterPorInepAsync(inep, ct);
            if (previsao is not null) diagnostico.DefinirContextoMl(previsao.ProbSemRampa, previsao.Faixa);
        }

        var resumoUf = await _previsoes.ObterResumoUfAsync(estabelecimento.Endereco.Uf, ct);
        if (resumoUf is not null)
        {
            diagnostico.DefinirContextoRegional(string.Create(CultureInfo.InvariantCulture,
                $"Em {resumoUf.Chave}, {resumoUf.PercentualSemRampaDeclarado:0.#}% das escolas declaram não ter rampa " +
                $"(Censo Escolar 2025); risco médio previsto pelo modelo de ML: {resumoUf.ProbabilidadeMedia * 100:0}%."));
        }

        diagnostico.Concluir();
        await _diagnosticos.AdicionarAsync(diagnostico, ct);
        await _uow.SalvarAsync(ct);
        return diagnostico.ToResponse(estabelecimento.Id, estabelecimento.Nome);
    }

    public async Task<DiagnosticoResponse> ObterAsync(int id, CancellationToken ct = default)
    {
        var d = await _diagnosticos.ObterCompletoAsync(id, ct) ?? throw new NaoEncontradoException(nameof(Diagnostico), id);
        return d.ToResponse(d.Avaliacao?.EstabelecimentoId ?? 0, d.Avaliacao?.Estabelecimento?.Nome ?? string.Empty);
    }

    public async Task<IReadOnlyList<DiagnosticoResponse>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default)
    {
        var lista = await _diagnosticos.ListarPorEstabelecimentoAsync(estabelecimentoId, ct);
        return lista.Select(d => d.ToResponse(estabelecimentoId, d.Avaliacao?.Estabelecimento?.Nome ?? string.Empty)).ToList();
    }
}
