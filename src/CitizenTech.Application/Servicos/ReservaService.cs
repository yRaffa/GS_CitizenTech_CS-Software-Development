using CitizenTech.Application.Dtos;
using CitizenTech.Application.Equipe;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Exceptions;
using CitizenTech.Domain.Interfaces;

namespace CitizenTech.Application.Servicos;

/// <summary>
/// Operação transacional (R6) do fluxo da CitizenTech: ENCAMINHAR um diagnóstico a um parceiro reservando vagas.
/// Três escritas atômicas: (1) UPDATE Parceiros (baixa o saldo), (2) INSERT Encaminhamentos, (3) INSERT ReservasAtendimento.
/// </summary>
public class ReservaService : IReservaService
{
    private readonly IParceiroRepository _parceiros;
    private readonly IDiagnosticoRepository _diagnosticos;
    private readonly IEncaminhamentoRepository _encaminhamentos;
    private readonly IReservaRepository _reservas;
    private readonly IUnitOfWork _uow;
    private readonly EquipeInfo _equipe;
    private readonly bool _permitirFalhaSimulada;

    public ReservaService(IParceiroRepository parceiros, IDiagnosticoRepository diagnosticos,
                          IEncaminhamentoRepository encaminhamentos, IReservaRepository reservas, IUnitOfWork uow, EquipeInfo equipe, bool permitirFalhaSimulada = true)
    {
        _parceiros = parceiros;
        _diagnosticos = diagnosticos;
        _encaminhamentos = encaminhamentos;
        _reservas = reservas;
        _uow = uow;
        _equipe = equipe;
        _permitirFalhaSimulada = permitirFalhaSimulada;
    }

    public async Task<ReservaResponse> ReservarAsync(ReservaRequest request, CancellationToken ct = default)
    {
        // Validações ANTES de abrir a transação: rejeição antecipada, sem escrita (T06).
        if (request.Quantidade <= 0) throw new DomainException("A quantidade de vagas deve ser maior que zero.");
        var executor = _equipe.ValidarExecutor(request.Executor);
        var parceiro = await _parceiros.ObterPorIdAsync(request.ParceiroId, ct)
            ?? throw new NaoEncontradoException(nameof(Parceiro), request.ParceiroId);
        var diagnostico = await _diagnosticos.ObterCompletoAsync(request.DiagnosticoId, ct)
            ?? throw new NaoEncontradoException(nameof(Diagnostico), request.DiagnosticoId);
        var estabelecimentoId = diagnostico.Avaliacao?.EstabelecimentoId
            ?? throw new DomainException("O diagnóstico não está ligado a um estabelecimento.");
        if (!parceiro.Ativo) throw new DomainException("O parceiro está inativo.");
        if (await _encaminhamentos.ExisteAsync(request.DiagnosticoId, request.ParceiroId, ct))
            throw new ConflitoException("Já existe um encaminhamento deste diagnóstico para este parceiro.");

        await using var transacao = await _uow.IniciarTransacaoAsync(ct); // BEGIN TRANSACTION
        try
        {
            parceiro.ReservarVagas(request.Quantidade);                    // escrita 1: saldo do parceiro (409 se insuficiente)
            var encaminhamento = new Encaminhamento(request.DiagnosticoId, parceiro.Id,
                $"Encaminhado com reserva de {request.Quantidade} vaga(s) por {executor}.");
            await _encaminhamentos.AdicionarAsync(encaminhamento, ct);      // escrita 2: encaminhamento do diagnóstico ao parceiro
            var reserva = new ReservaAtendimento(encaminhamento, estabelecimentoId, request.Quantidade, _equipe.RmLider, executor);
            await _reservas.AdicionarAsync(reserva, ct);                    // escrita 3: reserva das vagas
            await _uow.SalvarAsync(ct);                                     // UPDATE + 2 INSERT enviados ao banco (ainda sem COMMIT)

            if (request.SimularFalhaAposEscritas && _permitirFalhaSimulada)
                throw new OperacaoRevertidaException(
                    "Falha simulada DEPOIS do UPDATE e dos INSERTs e ANTES do COMMIT: ROLLBACK executado, nenhuma alteração foi mantida.");

            await transacao.CommitAsync(ct);                                // COMMIT: as três alterações persistem juntas
            return reserva.ToResponse(parceiro.Nome, parceiro.VagasAtendimento);
        }
        catch
        {
            await transacao.RollbackAsync(ct);                              // ROLLBACK: desfaz UPDATE e INSERTs
            throw;
        }
    }

    public async Task<IReadOnlyList<ReservaResponse>> ListarAsync(int? parceiroId, CancellationToken ct = default)
    {
        var lista = await _reservas.ListarAsync(parceiroId, ct);
        return lista.Select(r => r.ToResponse(r.Parceiro?.Nome ?? string.Empty, r.Parceiro?.VagasAtendimento ?? 0)).ToList();
    }
}
