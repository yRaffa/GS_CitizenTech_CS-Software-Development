using CitizenTech.Application;
using CitizenTech.Application.Dtos;
using CitizenTech.Application.Equipe;
using Microsoft.AspNetCore.Mvc;

namespace CitizenTech.Api.Controllers;

[ApiController]
[Route("api/equipe")]
[Produces("application/json")]
public class EquipeController : ControllerBase
{
    private readonly EquipeInfo _equipe;
    public EquipeController(EquipeInfo equipe) => _equipe = equipe;

    /// <summary>Identificação acadêmica da equipe (lida do equipe.json). Não é autenticação.</summary>
    [HttpGet]
    public ActionResult<EquipeResponse> Obter() => Ok(_equipe.ToResponse());
}
