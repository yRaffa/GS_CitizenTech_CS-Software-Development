using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CitizenTech.Api.Controllers;

[ApiController]
[Route("api/parceiros")]
[Produces("application/json")]
public class ParceirosController : ControllerBase
{
    private readonly IParceiroService _parceiros;
    public ParceirosController(IParceiroService parceiros) => _parceiros = parceiros;

    /// <summary>Lista parceiros ativos (filtros opcionais: especialidade e UF atendida).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ParceiroResponse>>> Listar(
        [FromQuery] Especialidade? especialidade, [FromQuery] string? uf, CancellationToken ct)
        => Ok(await _parceiros.ListarAsync(especialidade, uf, ct));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ParceiroResponse>> Criar([FromBody] ParceiroRequest request, CancellationToken ct)
    {
        var criado = await _parceiros.CriarAsync(request, ct);
        return Created($"/api/parceiros/{criado.Id}", criado);
    }
}
