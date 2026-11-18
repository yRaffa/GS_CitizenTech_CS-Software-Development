using CitizenTech.Application.Dtos;
using CitizenTech.Application.Equipe;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Exceptions;
using CitizenTech.Domain.Interfaces;

namespace CitizenTech.Application.Servicos;

/// <summary>Envio e recuperação de imagens guardadas como bytes no banco (R4, R5, R7).</summary>
public class ImagemService : IImagemService
{
    public const int TamanhoMaximoBytes = 2 * 1024 * 1024; // 2 MiB

    private static readonly byte[] AssinaturaPng = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] AssinaturaJpeg = { 0xFF, 0xD8, 0xFF };

    private readonly IImagemRepository _imagens;
    private readonly IEstabelecimentoRepository _estabelecimentos;
    private readonly IUnitOfWork _uow;
    private readonly EquipeInfo _equipe;

    public ImagemService(IImagemRepository imagens, IEstabelecimentoRepository estabelecimentos, IUnitOfWork uow, EquipeInfo equipe)
    {
        _imagens = imagens;
        _estabelecimentos = estabelecimentos;
        _uow = uow;
        _equipe = equipe;
    }

    public async Task<ImagemResponse> EnviarAsync(ImagemUpload upload, CancellationToken ct = default)
    {
        // 1) Validações de entrada: tudo é rejeitado ANTES de qualquer escrita no banco.
        if (upload.Dados is null || upload.Dados.Length == 0)
            throw new DomainException("Arquivo ausente ou vazio (0 bytes).");
        if (string.IsNullOrWhiteSpace(upload.Titulo) || upload.Titulo.Trim().Length > ImagemEstabelecimento.TituloMaximo)
            throw new DomainException($"O título deve ter de 1 a {ImagemEstabelecimento.TituloMaximo} caracteres.");
        if (upload.Dados.Length > TamanhoMaximoBytes)
            throw new ArquivoGrandeDemaisException(
                $"Arquivo com {upload.Dados.Length} bytes excede o limite de {TamanhoMaximoBytes} bytes (2 MiB).");

        var executor = _equipe.ValidarExecutor(upload.Executor);
        var mimeReal = DetectarMime(upload.Dados)
            ?? throw new DomainException("Formato não aceito: o conteúdo não é PNG nem JPEG (a extensão sozinha não é considerada).");
        if (!string.Equals(upload.TipoMime, mimeReal, StringComparison.OrdinalIgnoreCase))
            throw new DomainException($"O tipo informado ('{upload.TipoMime}') não corresponde ao conteúdo real ({mimeReal}).");

        _ = await _estabelecimentos.ObterPorIdAsync(upload.EstabelecimentoId, ct)
            ?? throw new NaoEncontradoException(nameof(Estabelecimento), upload.EstabelecimentoId);

        // 2) Gravação: os bytes vão para a coluna varbinary(max).
        var imagem = new ImagemEstabelecimento(upload.EstabelecimentoId, upload.Titulo, upload.NomeArquivo,
            mimeReal, upload.Dados, _equipe.RmLider, executor);
        await _imagens.AdicionarAsync(imagem, ct);
        await _uow.SalvarAsync(ct);

        var metadados = await _imagens.ObterMetadadosAsync(imagem.Id, ct)
            ?? throw new NaoEncontradoException(nameof(ImagemEstabelecimento), imagem.Id);
        return metadados.ToResponse();
    }

    public async Task<ImagemResponse> ObterMetadadosAsync(int id, CancellationToken ct = default)
    {
        var m = await _imagens.ObterMetadadosAsync(id, ct)
            ?? throw new NaoEncontradoException(nameof(ImagemEstabelecimento), id);
        return m.ToResponse();
    }

    public async Task<IReadOnlyList<ImagemResponse>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default)
    {
        var lista = await _imagens.ListarPorEstabelecimentoAsync(estabelecimentoId, ct);
        return lista.Select(m => m.ToResponse()).ToList();
    }

    public async Task<ImagemConteudo> ObterConteudoAsync(int id, CancellationToken ct = default)
    {
        var imagem = await _imagens.ObterComConteudoAsync(id, ct)
            ?? throw new NaoEncontradoException(nameof(ImagemEstabelecimento), id);
        return new ImagemConteudo(imagem.Dados, imagem.TipoMime, imagem.NomeArquivo);
    }

    /// <summary>Identifica PNG/JPEG pelos primeiros bytes (assinatura), e não pela extensão.</summary>
    public static string? DetectarMime(byte[] dados)
    {
        if (dados.Length >= AssinaturaPng.Length && dados.AsSpan(0, AssinaturaPng.Length).SequenceEqual(AssinaturaPng)) return "image/png";
        if (dados.Length >= AssinaturaJpeg.Length && dados.AsSpan(0, AssinaturaJpeg.Length).SequenceEqual(AssinaturaJpeg)) return "image/jpeg";
        return null;
    }
}
