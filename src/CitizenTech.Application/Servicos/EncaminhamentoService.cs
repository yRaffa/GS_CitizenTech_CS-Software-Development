using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Exceptions;
using CitizenTech.Domain.Interfaces;

namespace CitizenTech.Application.Servicos;

public class EncaminhamentoService : IEncaminhamentoService
{
    private readonly IEncaminhamentoRepository _encaminhamentos;
    private readonly IDiagnosticoRepository _diagnosticos;
    private readonly IParceiroRepository _parceiros;
    private readonly IUnitOfWork _uow;

    public EncaminhamentoService(IEncaminhamentoRepository encaminhamentos, IDiagnosticoRepository diagnosticos,
                                 IParceiroRepository parceiros, IUnitOfWork uow)
    {
        _encaminhamentos = encaminhamentos;
        _diagnosticos = diagnosticos;
        _parceiros = parceiros;
        _uow = uow;
    }

    public async Task<EncaminhamentoResponse> CriarAsync(EncaminhamentoRequest request, CancellationToken ct = default)
    {
        _ = await _diagnosticos.ObterCompletoAsync(request.DiagnosticoId, ct)
            ?? throw new NaoEncontradoException(nameof(Diagnostico), request.DiagnosticoId);
        var parceiro = await _parceiros.ObterPorIdAsync(request.ParceiroId, ct)
            ?? throw new NaoEncontradoException(nameof(Parceiro), request.ParceiroId);

        if (!parceiro.Ativo) throw new DomainException("O parceiro está inativo.");
        if (await _encaminhamentos.ExisteAsync(request.DiagnosticoId, request.ParceiroId, ct))
            throw new DomainException("Já existe um encaminhamento deste diagnóstico para este parceiro.");

        var encaminhamento = new Encaminhamento(request.DiagnosticoId, request.ParceiroId, request.Observacao);
        await _encaminhamentos.AdicionarAsync(encaminhamento, ct);
        await _uow.SalvarAsync(ct);
        return encaminhamento.ToResponse(parceiro.Nome);
    }

    public async Task<EncaminhamentoResponse> AlterarStatusAsync(int id, AlterarStatusRequest request, CancellationToken ct = default)
    {
        var encaminhamento = await _encaminhamentos.ObterPorIdAsync(id, ct)
            ?? throw new NaoEncontradoException(nameof(Encaminhamento), id);

        encaminhamento.AlterarStatus(request.Status, request.Observacao);
        await _uow.SalvarAsync(ct);
        return encaminhamento.ToResponse(encaminhamento.Parceiro?.Nome ?? string.Empty);
    }

    public async Task<IReadOnlyList<EncaminhamentoResponse>> ListarAsync(StatusEncaminhamento? status, CancellationToken ct = default)
    {
        var lista = await _encaminhamentos.ListarAsync(status, ct);
        return lista.Select(e => e.ToResponse(e.Parceiro?.Nome ?? string.Empty)).ToList();
    }
}
