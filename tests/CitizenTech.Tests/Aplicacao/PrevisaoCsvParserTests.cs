using CitizenTech.Application.Csv;
using CitizenTech.Domain.Enums;
using Xunit;

namespace CitizenTech.Tests.Aplicacao;

public class PrevisaoCsvParserTests
{
    [Fact]
    public void LinhaValida_ConverteTodosOsCampos()
    {
        var p = PrevisaoCsvParser.Converter("11022558,Norte,RO,1100015,2,0.9507,1,ALTA");

        Assert.NotNull(p);
        Assert.Equal(11022558L, p!.CodigoInep);
        Assert.Equal("Norte", p.Regiao);
        Assert.Equal("RO", p.Uf);
        Assert.Equal(0.9507, p.ProbSemRampa, 4);
        Assert.True(p.SemRampaDeclarado);
        Assert.Equal(FaixaRisco.Alta, p.Faixa);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc,Norte,RO,1,2,0.5,1,ALTA")]
    [InlineData("1,Norte,RO,1,2,0.5,1")]
    [InlineData("1,Norte,RO,1,2,0.5,1,DESCONHECIDA")]
    public void LinhaInvalida_RetornaNulo(string linha)
    {
        Assert.Null(PrevisaoCsvParser.Converter(linha));
    }
}
