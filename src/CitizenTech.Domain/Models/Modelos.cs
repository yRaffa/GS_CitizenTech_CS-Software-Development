namespace CitizenTech.Domain.Models;

public record ResultadoPaginado<T>(IReadOnlyList<T> Itens, int Total, int Pagina, int TamanhoPagina);

/// <summary>Agregado das previsões do ML por região ou UF.</summary>
public record ResumoRisco(string Chave, int TotalEscolas, double PercentualSemRampaDeclarado, double ProbabilidadeMedia, int EscolasRiscoAlto);
