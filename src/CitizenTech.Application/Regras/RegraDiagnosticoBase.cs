using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;

namespace CitizenTech.Application.Regras;

/// <summary>Classe base: concentra os dados comuns (tipo, gravidade, especialidade, recomendação) das regras.</summary>
public abstract class RegraDiagnosticoBase : IRegraDiagnostico
{
    protected RegraDiagnosticoBase(TipoBarreira tipo, Gravidade gravidade, Especialidade especialidade, string recomendacao)
    {
        Tipo = tipo;
        Gravidade = gravidade;
        Especialidade = especialidade;
        Recomendacao = recomendacao;
    }

    protected TipoBarreira Tipo { get; }
    protected Gravidade Gravidade { get; }
    protected Especialidade Especialidade { get; }
    protected string Recomendacao { get; }

    public abstract IEnumerable<Barreira> Avaliar(Avaliacao avaliacao);

    protected Barreira Criar(string descricao) => new(Tipo, descricao, Gravidade, Especialidade, Recomendacao);
}
