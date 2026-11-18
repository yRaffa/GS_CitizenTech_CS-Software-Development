using CitizenTech.Domain.Exceptions;

namespace CitizenTech.Domain.Entities;

/// <summary>
/// Reserva de vagas de atendimento de um parceiro para tratar um diagnóstico de acessibilidade.
/// Nasce junto com o <see cref="Encaminhamento"/> na MESMA transação: o saldo do parceiro cai,
/// o encaminhamento é criado e a reserva é registrada (3 escritas, COMMIT ou ROLLBACK).
/// </summary>
public class ReservaAtendimento
{
    private ReservaAtendimento() { EquipeRmLider = Executor = string.Empty; } // EF Core

    public ReservaAtendimento(Encaminhamento encaminhamento, int estabelecimentoId, int quantidade, string equipeRmLider, string executor)
    {
        if (quantidade <= 0) throw new DomainException("A quantidade de vagas deve ser maior que zero.");
        Encaminhamento = encaminhamento;
        ParceiroId = encaminhamento.ParceiroId;
        DiagnosticoId = encaminhamento.DiagnosticoId;
        EstabelecimentoId = estabelecimentoId;
        Quantidade = quantidade;
        DataUtc = DateTime.UtcNow;
        EquipeRmLider = equipeRmLider;
        Executor = executor;
    }

    public int Id { get; private set; }
    public int EncaminhamentoId { get; private set; }
    public Encaminhamento? Encaminhamento { get; private set; }
    public int DiagnosticoId { get; private set; }
    public Diagnostico? Diagnostico { get; private set; }
    public int ParceiroId { get; private set; }
    public Parceiro? Parceiro { get; private set; }
    public int EstabelecimentoId { get; private set; }
    public Estabelecimento? Estabelecimento { get; private set; }
    public int Quantidade { get; private set; }
    public DateTime DataUtc { get; private set; }
    public string EquipeRmLider { get; private set; }
    public string Executor { get; private set; }
}
