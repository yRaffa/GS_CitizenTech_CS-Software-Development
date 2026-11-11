using CitizenTech.Domain.Enums;

namespace CitizenTech.Domain.Entities;

/// <summary>Previsão do modelo de ML (entrega de IA): probabilidade de uma escola NÃO ter rampa.</summary>
public class PrevisaoRisco
{
    private PrevisaoRisco() { Regiao = Uf = string.Empty; } // EF Core

    public PrevisaoRisco(long codigoInep, string regiao, string uf, int codigoMunicipio, int tpDependencia,
                         double probSemRampa, bool semRampaDeclarado, FaixaRisco faixa)
    {
        CodigoInep = codigoInep; Regiao = regiao; Uf = uf; CodigoMunicipio = codigoMunicipio;
        TpDependencia = tpDependencia; ProbSemRampa = probSemRampa; SemRampaDeclarado = semRampaDeclarado; Faixa = faixa;
    }

    public int Id { get; private set; }
    public long CodigoInep { get; private set; }
    public string Regiao { get; private set; }
    public string Uf { get; private set; }
    public int CodigoMunicipio { get; private set; }
    public int TpDependencia { get; private set; }
    public double ProbSemRampa { get; private set; }
    public bool SemRampaDeclarado { get; private set; }
    public FaixaRisco Faixa { get; private set; }
}
