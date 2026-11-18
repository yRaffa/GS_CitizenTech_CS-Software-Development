namespace CitizenTech.Domain.Models;

/// <summary>Metadados da imagem SEM os bytes (a consulta LINQ não traz a coluna Dados).</summary>
public record ImagemMetadados(
    int Id, int EstabelecimentoId, string Titulo, string NomeArquivo, string TipoMime,
    int TamanhoBytes, DateTime DataUtc, string EquipeRmLider, string Executor);
