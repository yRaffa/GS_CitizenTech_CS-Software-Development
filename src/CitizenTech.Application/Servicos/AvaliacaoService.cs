using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Exceptions;
using CitizenTech.Domain.Interfaces;

namespace CitizenTech.Application.Servicos;

public class AvaliacaoService : IAvaliacaoService
{
    private readonly IEstabelecimentoRepository _estabelecimentos;
    private readonly IAvaliacaoRepository _avaliacoes;
    private readonly IUnitOfWork _uow;

    public AvaliacaoService(IEstabelecimentoRepository estabelecimentos, IAvaliacaoRepository avaliacoes, IUnitOfWork uow)
    {
        _estabelecimentos = estabelecimentos;
        _avaliacoes = avaliacoes;
        _uow = uow;
    }

    public async Task<AvaliacaoResponse> CriarAsync(int estabelecimentoId, AvaliacaoRequest request, CancellationToken ct = default)
    {
        _ = await _estabelecimentos.ObterPorIdAsync(estabelecimentoId, ct)
            ?? throw new NaoEncontradoException(nameof(Estabelecimento), estabelecimentoId);

        var avaliacao = new Avaliacao(estabelecimentoId, request.Observacoes);
        foreach (var r in request.Respostas) avaliacao.RegistrarResposta(r.Item, r.Valor);
        foreach (var m in request.Medicoes) avaliacao.RegistrarMedicao(m.Tipo, m.Valor);

        await _avaliacoes.AdicionarAsync(avaliacao, ct);
        await _uow.SalvarAsync(ct);
        return avaliacao.ToResponse();
    }

    public async Task<AvaliacaoResponse> ObterAsync(int id, CancellationToken ct = default)
    {
        var avaliacao = await _avaliacoes.ObterCompletaAsync(id, ct)
            ?? throw new NaoEncontradoException(nameof(Avaliacao), id);
        return avaliacao.ToResponse();
    }

    public async Task<IReadOnlyList<AvaliacaoResponse>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default)
    {
        var lista = await _avaliacoes.ListarPorEstabelecimentoAsync(estabelecimentoId, ct);
        return lista.Select(a => a.ToResponse()).ToList();
    }
}
