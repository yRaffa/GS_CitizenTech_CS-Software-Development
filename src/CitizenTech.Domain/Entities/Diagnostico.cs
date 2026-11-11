using CitizenTech.Domain.Enums;

namespace CitizenTech.Domain.Entities;

/// <summary>Triagem preliminar gerada a partir de uma avaliação. NÃO substitui avaliação técnica presencial.</summary>
public class Diagnostico
{
    public const string AvisoTriagem =
        "Este diagnóstico é uma triagem preliminar baseada nas informações fornecidas e não substitui a avaliação técnica presencial.";

    private readonly List<Barreira> _barreiras = new();

    private Diagnostico() { Resumo = string.Empty; } // EF Core

    public Diagnostico(int avaliacaoId)
    {
        AvaliacaoId = avaliacaoId;
        DataGeracao = DateTime.UtcNow;
        Resumo = string.Empty;
        NivelRisco = NivelRisco.Baixo;
    }

    public int Id { get; private set; }
    public int AvaliacaoId { get; private set; }
    public Avaliacao? Avaliacao { get; private set; }
    public DateTime DataGeracao { get; private set; }
    public NivelRisco NivelRisco { get; private set; }

    /// <summary>Probabilidade de faltar rampa estimada pelo modelo de ML (somente escolas com código INEP).</summary>
    public double? ProbabilidadeSemRampaMl { get; private set; }
    public FaixaRisco? FaixaRiscoMl { get; private set; }

    /// <summary>Contexto territorial (percentual de escolas sem rampa na UF).</summary>
    public string? ContextoRegional { get; private set; }
    public string Resumo { get; private set; }

    public IReadOnlyCollection<Barreira> Barreiras => _barreiras;

    /// <summary>União das especialidades necessárias para tratar as barreiras (propriedade calculada).</summary>
    public Especialidade EspecialidadesNecessarias =>
        _barreiras.Aggregate(Especialidade.Nenhuma, (acc, b) => acc | b.EspecialidadeSugerida);

    public void AdicionarBarreira(Barreira barreira) => _barreiras.Add(barreira);

    public void DefinirContextoMl(double probabilidade, FaixaRisco faixa)
    {
        ProbabilidadeSemRampaMl = probabilidade;
        FaixaRiscoMl = faixa;
    }

    public void DefinirContextoRegional(string? contexto) => ContextoRegional = contexto;

    /// <summary>
    /// Regra do nível de risco:
    /// Alto  = 2+ barreiras de gravidade Alta ou pontuação >= 7 (Alta=3, Média=2, Baixa=1);
    /// Médio = 1 barreira Alta ou pontuação >= 3;
    /// Baixo = demais casos. Se o modelo de ML indicar faixa Alta, o nível sobe um grau (máx. Alto).
    /// </summary>
    public NivelRisco CalcularNivel()
    {
        var altas = _barreiras.Count(b => b.Gravidade == Gravidade.Alta);
        var pontos = _barreiras.Sum(b => (int)b.Gravidade);

        var nivel = (altas >= 2 || pontos >= 7) ? NivelRisco.Alto
                  : (altas == 1 || pontos >= 3) ? NivelRisco.Medio
                  : NivelRisco.Baixo;

        if (FaixaRiscoMl == FaixaRisco.Alta && nivel < NivelRisco.Alto) nivel = (NivelRisco)((int)nivel + 1);
        return nivel;
    }

    public void Concluir()
    {
        NivelRisco = CalcularNivel();
        Resumo = _barreiras.Count == 0
            ? "Nenhuma barreira provável identificada com as informações fornecidas."
            : $"{_barreiras.Count} possível(is) barreira(s) identificada(s); nível de risco {NivelRisco}.";
    }
}
