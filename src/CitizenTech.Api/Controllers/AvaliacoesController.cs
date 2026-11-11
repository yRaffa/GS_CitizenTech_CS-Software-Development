using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CitizenTech.Api.Controllers;

[ApiController]
[Route("api/avaliacoes")]
[Produces("application/json")]
public class AvaliacoesController : ControllerBase
{
    private readonly IAvaliacaoService _avaliacoes;
    private readonly IDiagnosticoService _diagnosticos;

    public AvaliacoesController(IAvaliacaoService avaliacoes, IDiagnosticoService diagnosticos)
    {
        _avaliacoes = avaliacoes;
        _diagnosticos = diagnosticos;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvaliacaoResponse>> Obter(int id, CancellationToken ct)
        => Ok(await _avaliacoes.ObterAsync(id, ct));

    /// <summary>Gera o diagnóstico (triagem preliminar) a partir da avaliação.</summary>
    [HttpPost("{id:int}/diagnosticos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiagnosticoResponse>> GerarDiagnostico(int id, CancellationToken ct)
    {
        var diagnostico = await _diagnosticos.GerarAsync(id, ct);
        return Created($"/api/diagnosticos/{diagnostico.Id}", diagnostico);
    }
}
