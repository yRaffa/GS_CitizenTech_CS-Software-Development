using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;

namespace CitizenTech.Tests;

/// <summary>Construtores de objetos de teste.</summary>
public static class Dados
{
    public static Estabelecimento NovoEstabelecimento(string uf = "SP", long? inep = null, string nome = "Escola Exemplo") =>
        new(nome, TipoEstabelecimento.Escola, new Endereco("Rua A", "10", "Centro", "Cidade Exemplo", uf), inep, null);

    /// <summary>Avaliação em que tudo está acessível e as medições estão dentro dos limites.</summary>
    public static Avaliacao AvaliacaoTodaAcessivel(int estabelecimentoId = 1)
    {
        var a = new Avaliacao(estabelecimentoId);
        foreach (var item in Enum.GetValues<ItemAcessibilidade>()) a.RegistrarResposta(item, Resposta.Sim);
        a.RegistrarMedicao(TipoMedicao.InclinacaoRampaPercentual, 8);
        a.RegistrarMedicao(TipoMedicao.LarguraPortaCm, 90);
        a.RegistrarMedicao(TipoMedicao.LarguraCorredorCm, 120);
        return a;
    }

    public static Barreira NovaBarreira(Gravidade gravidade, Especialidade esp = Especialidade.ObrasAcessibilidade) =>
        new(TipoBarreira.Rampa, "Barreira de teste", gravidade, esp, "Recomendação");

    public static PrevisaoRisco NovaPrevisao(long inep, string uf = "SP", string regiao = "Sudeste", double prob = 0.9,
                                             bool declarado = true, FaixaRisco faixa = FaixaRisco.Alta) =>
        new(inep, regiao, uf, 3550308, 3, prob, declarado, faixa);
}
