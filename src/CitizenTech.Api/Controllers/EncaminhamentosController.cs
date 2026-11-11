using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CitizenTech.Api.Controllers;

[ApiController]
[Route("api/encaminhamentos")]
[Produces("application/json")]
public class EncaminhamentosController : ControllerBase
{
    private readonly IEncaminhamentoService _encaminhamentos;
    public EncaminhamentosController(IEncaminhamentoService encaminhamentos) => _encaminhamentos = encaminhamentos;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EncaminhamentoResponse>>> Listar([FromQuery] StatusEncaminhamento? status, CancellationToken ct)
        => Ok(await _encaminhamentos.ListarAsync(status, ct));

    /// <summary>Conecta um diagnóstico a um parceiro (a plataforma apenas conecta; não executa o serviço).</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EncaminhamentoResponse>> Criar([FromBody] EncaminhamentoRequest request, CancellationToken ct)
    {
        var criado = await _encaminhamentos.CriarAsync(request, ct);
        return Created($"/api/encaminhamentos/{criado.Id}", criado);
    }

    /// <summary>Altera o status (Pendente → Enviado → EmAnalise → Concluido; Cancelado a qualquer momento antes do fim).</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EncaminhamentoResponse>> AlterarStatus(int id, [FromBody] AlterarStatusRequest request, CancellationToken ct)
        => Ok(await _encaminhamentos.AlterarStatusAsync(id, request, ct));
}
