using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CitizenTech.Api.Controllers;

[ApiController]
[Route("api/imagens")]
public class ImagensController : ControllerBase
{
    private readonly IImagemService _imagens;
    public ImagensController(IImagemService imagens) => _imagens = imagens;

    /// <summary>
    /// Envia uma imagem (PNG/JPEG, até 2 MiB) vinculada a um estabelecimento. Os bytes são gravados no banco (varbinary(max)).
    /// Campos multipart: arquivo, titulo, estabelecimentoId, executor (NomeAluno_RM).
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [RequestSizeLimit(6 * 1024 * 1024)] // corte de segurança do transporte; o limite funcional de 2 MiB é validado no serviço (413)
    [ProducesResponseType(typeof(ImagemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<ImagemResponse>> Enviar(
        IFormFile? arquivo, [FromForm] string? titulo, [FromForm] int estabelecimentoId, [FromForm] string? executor, CancellationToken ct)
    {
        byte[] dados = Array.Empty<byte>();
        if (arquivo is not null && arquivo.Length > 0)
        {
            await using var stream = arquivo.OpenReadStream();   // stream do arquivo recebido
            using var memoria = new MemoryStream();
            await stream.CopyToAsync(memoria, ct);               // copia assíncrona para um buffer de bytes
            dados = memoria.ToArray();
        }

        var criada = await _imagens.EnviarAsync(new ImagemUpload(
            titulo ?? string.Empty, estabelecimentoId, executor ?? string.Empty,
            arquivo?.FileName ?? string.Empty, arquivo?.ContentType ?? string.Empty, dados), ct);
        return Created($"/api/imagens/{criada.Id}", criada);
    }

    /// <summary>Metadados da imagem em JSON (sem os bytes).</summary>
    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImagemResponse>> ObterMetadados(int id, CancellationToken ct)
        => Ok(await _imagens.ObterMetadadosAsync(id, ct));

    /// <summary>Lista os metadados das imagens de um estabelecimento.</summary>
    [HttpGet]
    [Produces("application/json")]
    public async Task<ActionResult<IReadOnlyList<ImagemResponse>>> Listar([FromQuery] int estabelecimentoId, CancellationToken ct)
        => Ok(await _imagens.ListarPorEstabelecimentoAsync(estabelecimentoId, ct));

    /// <summary>Conteúdo (bytes) da imagem, com o MIME apropriado (image/png ou image/jpeg).</summary>
    [HttpGet("{id:int}/conteudo")]
    [Produces("image/png", "image/jpeg")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterConteudo(int id, CancellationToken ct)
    {
        var conteudo = await _imagens.ObterConteudoAsync(id, ct);
        return File(conteudo.Dados, conteudo.TipoMime);
    }
}
