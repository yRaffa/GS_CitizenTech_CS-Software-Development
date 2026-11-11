using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;

namespace CitizenTech.Application.Regras;

/// <summary>Gera uma barreira quando o estabelecimento responde "Não" a um item do questionário.</summary>
public class RegraItemAusente : RegraDiagnosticoBase
{
    private readonly ItemAcessibilidade _item;
    private readonly string _descricao;

    public RegraItemAusente(ItemAcessibilidade item, TipoBarreira tipo, Gravidade gravidade,
                            Especialidade especialidade, string descricao, string recomendacao)
        : base(tipo, gravidade, especialidade, recomendacao)
    {
        _item = item;
        _descricao = descricao;
    }

    public override IEnumerable<Barreira> Avaliar(Avaliacao avaliacao)
    {
        if (avaliacao.ObterResposta(_item) == Resposta.Nao)
            yield return Criar(_descricao);
    }
}
