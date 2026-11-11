using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CitizenTech.Api.Controllers;

[ApiController]
[Route("api/diagnosticos")]
[Produces("application/json")]
public class DiagnosticosController : ControllerBase
{
    private readonly IDiagnosticoService _diagnosticos;
    private readonly IParceiroService _parceiros;

    public DiagnosticosController(IDiagnosticoService diagnosticos, IParceiroService parceiros)
    {
        _diagnosticos = diagnosticos;
        _parceiros = parceiros;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiagnosticoResponse>> Obter(int id, CancellationToken ct)
        => Ok(await _diagnosticos.ObterAsync(id, ct));

    /// <summary>Parceiros que atendem a UF do estabelecimento e cobrem as especialidades das barreiras (ordenados por aderência).</summary>
    [HttpGet("{id:int}/parceiros-sugeridos")]
    public async Task<ActionResult<IReadOnlyList<ParceiroSugeridoResponse>>> ParceirosSugeridos(int id, CancellationToken ct)
        => Ok(await _parceiros.SugerirParaDiagnosticoAsync(id, ct));
}
