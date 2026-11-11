using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace CitizenTech.Api.Controllers;

[ApiController]
[Route("api/estabelecimentos")]
[Produces("application/json")]
public class EstabelecimentosController : ControllerBase
{
    private readonly IEstabelecimentoService _estabelecimentos;
    private readonly IAvaliacaoService _avaliacoes;
    private readonly IDiagnosticoService _diagnosticos;

    public EstabelecimentosController(IEstabelecimentoService estabelecimentos, IAvaliacaoService avaliacoes, IDiagnosticoService diagnosticos)
    {
        _estabelecimentos = estabelecimentos;
        _avaliacoes = avaliacoes;
        _diagnosticos = diagnosticos;
    }

    /// <summary>Lista estabelecimentos (filtros por UF e tipo, com paginação).</summary>
    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<EstabelecimentoResponse>>> Listar(
        [FromQuery] string? uf, [FromQuery] TipoEstabelecimento? tipo,
        [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken ct = default)
        => Ok(await _estabelecimentos.ListarAsync(uf, tipo, pagina, tamanhoPagina, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstabelecimentoResponse>> Obter(int id, CancellationToken ct)
        => Ok(await _estabelecimentos.ObterAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EstabelecimentoResponse>> Criar([FromBody] EstabelecimentoRequest request, CancellationToken ct)
    {
        var criado = await _estabelecimentos.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EstabelecimentoResponse>> Atualizar(int id, [FromBody] EstabelecimentoRequest request, CancellationToken ct)
        => Ok(await _estabelecimentos.AtualizarAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        await _estabelecimentos.RemoverAsync(id, ct);
        return NoContent();
    }

    /// <summary>Registra uma avaliação (questionário + medições guiadas) do estabelecimento.</summary>
    [HttpPost("{id:int}/avaliacoes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<AvaliacaoResponse>> CriarAvaliacao(int id, [FromBody] AvaliacaoRequest request, CancellationToken ct)
    {
        var criada = await _avaliacoes.CriarAsync(id, request, ct);
        return Created($"/api/avaliacoes/{criada.Id}", criada);
    }

    [HttpGet("{id:int}/avaliacoes")]
    public async Task<ActionResult<IReadOnlyList<AvaliacaoResponse>>> ListarAvaliacoes(int id, CancellationToken ct)
        => Ok(await _avaliacoes.ListarPorEstabelecimentoAsync(id, ct));

    [HttpGet("{id:int}/diagnosticos")]
    public async Task<ActionResult<IReadOnlyList<DiagnosticoResponse>>> ListarDiagnosticos(int id, CancellationToken ct)
        => Ok(await _diagnosticos.ListarPorEstabelecimentoAsync(id, ct));
}
