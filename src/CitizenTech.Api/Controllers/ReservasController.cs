using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CitizenTech.Api.Controllers;

[ApiController]
[Route("api/reservas")]
[Produces("application/json")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservas;
    public ReservasController(IReservaService reservas) => _reservas = reservas;

    /// <summary>
    /// Encaminha um diagnóstico a um parceiro reservando vagas: reduz o saldo do parceiro, cria o encaminhamento e insere a reserva, na mesma transação.
    /// Com <c>simularFalhaAposEscritas = true</c> a API falha depois das três escritas e faz ROLLBACK (teste T08).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReservaResponse>> Reservar([FromBody] ReservaRequest request, CancellationToken ct)
    {
        var criada = await _reservas.ReservarAsync(request, ct);
        return Created($"/api/reservas/{criada.Id}", criada);
    }

    /// <summary>Lista as reservas (filtro opcional por parceiro) para conferir o estado antes/depois.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReservaResponse>>> Listar([FromQuery] int? parceiroId, CancellationToken ct)
        => Ok(await _reservas.ListarAsync(parceiroId, ct));
}
