using CitizenTech.Domain.Exceptions;

namespace CitizenTech.Domain.Entities;

/// <summary>
/// Imagem (ex.: foto de uma barreira de acessibilidade) vinculada a um estabelecimento.
/// Os BYTES ficam no banco (coluna varbinary(max)), junto de título, nome do arquivo, MIME,
/// data UTC e identificação da equipe/executor.
/// </summary>
public class ImagemEstabelecimento
{
    public const int TituloMaximo = 100;

    private ImagemEstabelecimento()
    {
        Titulo = NomeArquivo = TipoMime = EquipeRmLider = Executor = string.Empty;
        Dados = Array.Empty<byte>();
    } // EF Core

    public ImagemEstabelecimento(int estabelecimentoId, string titulo, string nomeArquivo, string tipoMime,
                                 byte[] dados, string equipeRmLider, string executor)
    {
        if (string.IsNullOrWhiteSpace(titulo) || titulo.Trim().Length > TituloMaximo)
            throw new DomainException($"O título deve ter de 1 a {TituloMaximo} caracteres.");
        if (dados is null || dados.Length == 0) throw new DomainException("O arquivo da imagem está vazio.");
        if (string.IsNullOrWhiteSpace(tipoMime)) throw new DomainException("Tipo MIME é obrigatório.");

        EstabelecimentoId = estabelecimentoId;
        Titulo = titulo.Trim();
        NomeArquivo = string.IsNullOrWhiteSpace(nomeArquivo) ? "imagem" : Path.GetFileName(nomeArquivo.Trim());
        TipoMime = tipoMime;
        Dados = dados;
        TamanhoBytes = dados.Length;
        DataUtc = DateTime.UtcNow;
        EquipeRmLider = equipeRmLider;
        Executor = executor;
    }

    public int Id { get; private set; }
    public int EstabelecimentoId { get; private set; }
    public Estabelecimento? Estabelecimento { get; private set; }
    public string Titulo { get; private set; }
    public string NomeArquivo { get; private set; }
    public string TipoMime { get; private set; }
    public int TamanhoBytes { get; private set; }

    /// <summary>Conteúdo binário da imagem (varbinary(max) no SQL Server).</summary>
    public byte[] Dados { get; private set; }
    public DateTime DataUtc { get; private set; }

    /// <summary>RM do líder da equipe (vem do equipe.json).</summary>
    public string EquipeRmLider { get; private set; }

    /// <summary>Identificação NomeAluno_RM de quem executou o envio.</summary>
    public string Executor { get; private set; }
}
