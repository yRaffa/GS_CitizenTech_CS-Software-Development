using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Exceptions;
using Xunit;

namespace CitizenTech.Tests.Dominio;

public class DiagnosticoTests
{
    [Fact]
    public void SemBarreiras_NivelBaixo()
    {
        var d = new Diagnostico(1);
        d.Concluir();
        Assert.Equal(NivelRisco.Baixo, d.NivelRisco);
    }

    [Fact]
    public void UmaBarreiraAlta_NivelMedio()
    {
        var d = new Diagnostico(1);
        d.AdicionarBarreira(Dados.NovaBarreira(Gravidade.Alta));
        Assert.Equal(NivelRisco.Medio, d.CalcularNivel());
    }

    [Fact]
    public void DuasBarreirasAltas_NivelAlto()
    {
        var d = new Diagnostico(1);
        d.AdicionarBarreira(Dados.NovaBarreira(Gravidade.Alta));
        d.AdicionarBarreira(Dados.NovaBarreira(Gravidade.Alta));
        Assert.Equal(NivelRisco.Alto, d.CalcularNivel());
    }

    [Fact]
    public void FaixaAltaDoModeloDeMl_SobeUmGrau_MasNaoPassaDeAlto()
    {
        var d = new Diagnostico(1);
        d.DefinirContextoMl(0.95, FaixaRisco.Alta);
        Assert.Equal(NivelRisco.Medio, d.CalcularNivel());   // Baixo -> Médio

        d.AdicionarBarreira(Dados.NovaBarreira(Gravidade.Alta));
        d.AdicionarBarreira(Dados.NovaBarreira(Gravidade.Alta));
        Assert.Equal(NivelRisco.Alto, d.CalcularNivel());    // já era Alto
    }

    [Fact]
    public void EspecialidadesNecessarias_EhAUniaoDasBarreiras()
    {
        var d = new Diagnostico(1);
        d.AdicionarBarreira(Dados.NovaBarreira(Gravidade.Alta, Especialidade.ObrasAcessibilidade));
        d.AdicionarBarreira(Dados.NovaBarreira(Gravidade.Baixa, Especialidade.Sinalizacao));

        Assert.Equal(Especialidade.ObrasAcessibilidade | Especialidade.Sinalizacao, d.EspecialidadesNecessarias);
    }
}

public class EncaminhamentoTests
{
    [Fact]
    public void FluxoValido_PercorreTodosOsStatus()
    {
        var e = new Encaminhamento(1, 1);
        Assert.Equal(StatusEncaminhamento.Pendente, e.Status);

        e.AlterarStatus(StatusEncaminhamento.Enviado);
        e.AlterarStatus(StatusEncaminhamento.EmAnalise);
        e.AlterarStatus(StatusEncaminhamento.Concluido, "Concluído com sucesso");

        Assert.Equal(StatusEncaminhamento.Concluido, e.Status);
        Assert.Equal("Concluído com sucesso", e.Observacao);
    }

    [Fact]
    public void PularEtapa_LancaDomainException()
    {
        var e = new Encaminhamento(1, 1);
        Assert.Throws<DomainException>(() => { e.AlterarStatus(StatusEncaminhamento.Concluido); });
    }

    [Fact]
    public void StatusFinal_NaoPodeMudar()
    {
        var e = new Encaminhamento(1, 1);
        e.AlterarStatus(StatusEncaminhamento.Cancelado);
        Assert.Throws<DomainException>(() => { e.AlterarStatus(StatusEncaminhamento.Enviado); });
    }
}

public class EstabelecimentoTests
{
    [Fact]
    public void UfInvalida_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => { _ = new Endereco("Rua A", "1", "Centro", "Cidade", "XX"); });
    }

    [Theory]
    [InlineData("SP", "Sudeste")]
    [InlineData("AM", "Norte")]
    [InlineData("go", "Centro-Oeste")]
    [InlineData("BA", "Nordeste")]
    [InlineData("RS", "Sul")]
    public void Regiao_EDerivadaDaUf(string uf, string regiaoEsperada)
    {
        Assert.Equal(regiaoEsperada, Dados.NovoEstabelecimento(uf).Regiao);
    }

    [Fact]
    public void NomeVazio_LancaDomainException()
    {
        var endereco = new Endereco("Rua A", "1", "Centro", "Cidade", "SP");
        Assert.Throws<DomainException>(() => { _ = new Estabelecimento("  ", TipoEstabelecimento.Comercio, endereco); });
    }
}

public class ParceiroTests
{
    [Fact]
    public void Atende_SomenteAsUfsInformadas()
    {
        var p = new Parceiro("Parceiro", "desc", Especialidade.Sinalizacao, new[] { "sp", "RJ" }, "a@b.com");
        Assert.True(p.Atende("SP"));
        Assert.True(p.Atende("rj"));
        Assert.False(p.Atende("MG"));
    }

    [Fact]
    public void TodoBrasil_AtendeQualquerUf()
    {
        var p = new Parceiro("Parceiro", "desc", Especialidade.Sinalizacao, new[] { "*" }, "a@b.com");
        Assert.True(p.Atende("AM"));
    }

    [Fact]
    public void SemEspecialidade_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => { _ = new Parceiro("P", "d", Especialidade.Nenhuma, new[] { "SP" }, "a@b.com"); });
    }

    [Fact]
    public void ParceiroInativo_NaoAtende()
    {
        var p = new Parceiro("Parceiro", "desc", Especialidade.Sinalizacao, new[] { "SP" }, "a@b.com");
        p.Desativar();
        Assert.False(p.Atende("SP"));
    }
}

public class AvaliacaoTests
{
    [Fact]
    public void RegistrarResposta_SubstituiAResostaExistente()
    {
        var a = new Avaliacao(1);
        a.RegistrarResposta(ItemAcessibilidade.Rampa, Resposta.Nao);
        a.RegistrarResposta(ItemAcessibilidade.Rampa, Resposta.Sim);

        Assert.Single(a.Respostas);
        Assert.Equal(Resposta.Sim, a.ObterResposta(ItemAcessibilidade.Rampa));
    }

    [Fact]
    public void ItemNaoRespondido_ContaComoNaoSei()
    {
        Assert.Equal(Resposta.NaoSei, new Avaliacao(1).ObterResposta(ItemAcessibilidade.Elevador));
    }

    [Fact]
    public void MedicaoNaoPositiva_LancaDomainException()
    {
        var a = new Avaliacao(1);
        Assert.Throws<DomainException>(() => { a.RegistrarMedicao(TipoMedicao.LarguraPortaCm, 0); });
    }
}
