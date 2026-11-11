using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;

namespace CitizenTech.Application.Regras;

/// <summary>Sinaliza itens sem resposta ou marcados como "Não sei": a triagem fica menos confiável.</summary>
public class RegraInformacaoPendente : RegraDiagnosticoBase
{
    public RegraInformacaoPendente()
        : base(TipoBarreira.InformacaoPendente, Gravidade.Baixa, Especialidade.ConsultoriaTecnica,
               "Complete o questionário ou solicite uma avaliação técnica para verificar os itens pendentes.") { }

    public override IEnumerable<Barreira> Avaliar(Avaliacao avaliacao)
    {
        var pendentes = Enum.GetValues<ItemAcessibilidade>().Count(i => avaliacao.ObterResposta(i) == Resposta.NaoSei);
        if (pendentes > 0)
            yield return Criar($"{pendentes} item(ns) do questionário sem resposta ou marcado(s) como \"Não sei\".");
    }
}
