using System.Globalization;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;

namespace CitizenTech.Application.Regras;

public enum TipoLimite { Maximo, Minimo }

/// <summary>Gera uma barreira quando uma medição guiada ultrapassa o limite de referência.</summary>
public class RegraMedicaoForaDoLimite : RegraDiagnosticoBase
{
    private readonly TipoMedicao _medicao;
    private readonly TipoLimite _limite;
    private readonly double _valorLimite;
    private readonly string _unidade;
    private readonly string _descricao;

    public RegraMedicaoForaDoLimite(TipoMedicao medicao, TipoLimite limite, double valorLimite, string unidade,
                                    string descricao, TipoBarreira tipo, Gravidade gravidade,
                                    Especialidade especialidade, string recomendacao)
        : base(tipo, gravidade, especialidade, recomendacao)
    {
        _medicao = medicao;
        _limite = limite;
        _valorLimite = valorLimite;
        _unidade = unidade;
        _descricao = descricao;
    }

    public override IEnumerable<Barreira> Avaliar(Avaliacao avaliacao)
    {
        var valor = avaliacao.ObterMedicao(_medicao);
        if (valor is null) yield break;

        var fora = _limite == TipoLimite.Maximo ? valor > _valorLimite : valor < _valorLimite;
        if (!fora) yield break;

        var ref_ = _limite == TipoLimite.Maximo ? "máximo" : "mínimo";
        var inv = CultureInfo.InvariantCulture;
        yield return Criar($"{_descricao} (medido: {valor.Value.ToString("0.##", inv)} {_unidade}; " +
                           $"referência {ref_}: {_valorLimite.ToString("0.##", inv)} {_unidade}).");
    }
}
