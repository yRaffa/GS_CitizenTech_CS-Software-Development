using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Exceptions;

namespace CitizenTech.Domain.Entities;

/// <summary>Conexão de um diagnóstico com um parceiro, com ciclo de vida controlado.</summary>
public class Encaminhamento
{
    private static readonly Dictionary<StatusEncaminhamento, StatusEncaminhamento[]> Transicoes = new()
    {
        [StatusEncaminhamento.Pendente] = new[] { StatusEncaminhamento.Enviado, StatusEncaminhamento.Cancelado },
        [StatusEncaminhamento.Enviado] = new[] { StatusEncaminhamento.EmAnalise, StatusEncaminhamento.Cancelado },
        [StatusEncaminhamento.EmAnalise] = new[] { StatusEncaminhamento.Concluido, StatusEncaminhamento.Cancelado },
        [StatusEncaminhamento.Concluido] = Array.Empty<StatusEncaminhamento>(),
        [StatusEncaminhamento.Cancelado] = Array.Empty<StatusEncaminhamento>()
    };

    private Encaminhamento() { } // EF Core

    public Encaminhamento(int diagnosticoId, int parceiroId, string? observacao = null)
    {
        DiagnosticoId = diagnosticoId;
        ParceiroId = parceiroId;
        Observacao = observacao?.Trim();
        Status = StatusEncaminhamento.Pendente;
        DataCriacao = DataAtualizacao = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public int DiagnosticoId { get; private set; }
    public int ParceiroId { get; private set; }
    public Parceiro? Parceiro { get; private set; }
    public StatusEncaminhamento Status { get; private set; }
    public string? Observacao { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime DataAtualizacao { get; private set; }

    public bool PodeMudarPara(StatusEncaminhamento novo) => Transicoes[Status].Contains(novo);

    public void AlterarStatus(StatusEncaminhamento novo, string? observacao = null)
    {
        if (!PodeMudarPara(novo))
            throw new DomainException($"Transição inválida: {Status} -> {novo}.");

        Status = novo;
        if (!string.IsNullOrWhiteSpace(observacao)) Observacao = observacao.Trim();
        DataAtualizacao = DateTime.UtcNow;
    }
}
