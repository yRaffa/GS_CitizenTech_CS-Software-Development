using CitizenTech.Application.Regras;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;
using Xunit;

namespace CitizenTech.Tests.Dominio;

public class RegrasDiagnosticoTests
{
    private static RegraItemAusente RegraRampa() => new(ItemAcessibilidade.Rampa, TipoBarreira.Rampa, Gravidade.Alta,
        Especialidade.ObrasAcessibilidade, "Ausência de rampa.", "Recomendação");

    [Fact]
    public void ItemRespondidoNao_GeraBarreiraComAGravidadeDaRegra()
    {
        var avaliacao = new Avaliacao(1);
        avaliacao.RegistrarResposta(ItemAcessibilidade.Rampa, Resposta.Nao);

        var barreira = Assert.Single(RegraRampa().Avaliar(avaliacao));

        Assert.Equal(TipoBarreira.Rampa, barreira.Tipo);
        Assert.Equal(Gravidade.Alta, barreira.Gravidade);
    }

    [Theory]
    [InlineData(Resposta.Sim)]
    [InlineData(Resposta.NaoSei)]
    public void ItemRespondidoSimOuNaoSei_NaoGeraBarreiraDeItemAusente(Resposta resposta)
    {
        var avaliacao = new Avaliacao(1);
        avaliacao.RegistrarResposta(ItemAcessibilidade.Rampa, resposta);

        Assert.Empty(RegraRampa().Avaliar(avaliacao));
    }

    [Theory]
    [InlineData(8.0, false)]
    [InlineData(8.33, false)]
    [InlineData(10.0, true)]
    [InlineData(12.5, true)]
    public void InclinacaoDaRampa_AcimaDe833PorCento_GeraBarreira(double inclinacao, bool esperaBarreira)
    {
        var avaliacao = new Avaliacao(1);
        avaliacao.RegistrarMedicao(TipoMedicao.InclinacaoRampaPercentual, inclinacao);
        var regra = new RegraMedicaoForaDoLimite(TipoMedicao.InclinacaoRampaPercentual, TipoLimite.Maximo, 8.33, "%",
            "Inclinação acima do limite", TipoBarreira.InclinacaoRampa, Gravidade.Alta, Especialidade.ObrasAcessibilidade, "Rec");

        Assert.Equal(esperaBarreira, regra.Avaliar(avaliacao).Any());
    }

    [Theory]
    [InlineData(70, true)]
    [InlineData(79.9, true)]
    [InlineData(80, false)]
    [InlineData(95, false)]
    public void LarguraDaPorta_AbaixoDe80Cm_GeraBarreira(double largura, bool esperaBarreira)
    {
        var avaliacao = new Avaliacao(1);
        avaliacao.RegistrarMedicao(TipoMedicao.LarguraPortaCm, largura);
        var regra = new RegraMedicaoForaDoLimite(TipoMedicao.LarguraPortaCm, TipoLimite.Minimo, 80, "cm",
            "Vão livre abaixo do mínimo", TipoBarreira.Porta, Gravidade.Alta, Especialidade.ObrasAcessibilidade, "Rec");

        Assert.Equal(esperaBarreira, regra.Avaliar(avaliacao).Any());
    }

    [Fact]
    public void MedicaoNaoInformada_NaoGeraBarreira()
    {
        var regra = new RegraMedicaoForaDoLimite(TipoMedicao.LarguraPortaCm, TipoLimite.Minimo, 80, "cm",
            "Vão livre abaixo do mínimo", TipoBarreira.Porta, Gravidade.Alta, Especialidade.ObrasAcessibilidade, "Rec");

        Assert.Empty(regra.Avaliar(new Avaliacao(1)));
    }

    [Fact]
    public void InformacaoPendente_ContaItensSemResposta()
    {
        var avaliacao = new Avaliacao(1);
        avaliacao.RegistrarResposta(ItemAcessibilidade.Rampa, Resposta.Sim);
        avaliacao.RegistrarResposta(ItemAcessibilidade.Corrimao, Resposta.Sim);
        avaliacao.RegistrarResposta(ItemAcessibilidade.Elevador, Resposta.Nao);

        var barreira = Assert.Single(new RegraInformacaoPendente().Avaliar(avaliacao));

        Assert.Equal(TipoBarreira.InformacaoPendente, barreira.Tipo);
        Assert.Contains("5 item(ns)", barreira.Descricao);
    }

    [Fact]
    public void RegrasPadrao_AvaliacaoTodaAcessivel_NaoGeraNenhumaBarreira()
    {
        var avaliacao = Dados.AvaliacaoTodaAcessivel();

        var barreiras = RegrasPadrao.ObterTodas().SelectMany(r => r.Avaliar(avaliacao)).ToList();

        Assert.Empty(barreiras);
    }

    [Fact]
    public void RegrasPadrao_SemRampaEComPortaEstreita_GeraBarreirasAltas()
    {
        var avaliacao = Dados.AvaliacaoTodaAcessivel();
        avaliacao.RegistrarResposta(ItemAcessibilidade.Rampa, Resposta.Nao);
        avaliacao.RegistrarMedicao(TipoMedicao.LarguraPortaCm, 70);

        var barreiras = RegrasPadrao.ObterTodas().SelectMany(r => r.Avaliar(avaliacao)).ToList();

        Assert.Equal(2, barreiras.Count(b => b.Gravidade == Gravidade.Alta));
        Assert.Contains(barreiras, b => b.Tipo == TipoBarreira.Rampa);
        Assert.Contains(barreiras, b => b.Tipo == TipoBarreira.Porta);
    }
}
