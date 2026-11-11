using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Application.Utilitarios;
using CitizenTech.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CitizenTech.Api.Controllers;

/// <summary>Integração com a entrega de IA &amp; ML: previsões de risco de falta de rampa por escola, UF e região.</summary>
[ApiController]
[Route("api/risco")]
[Produces("application/json")]
public class RiscoController : ControllerBase
{
    private readonly IRiscoService _risco;
    private readonly IConfiguration _configuracao;

    public RiscoController(IRiscoService risco, IConfiguration configuracao)
    {
        _risco = risco;
        _configuracao = configuracao;
    }

    [HttpGet("regioes")]
    public async Task<ActionResult<IReadOnlyList<ResumoRiscoResponse>>> PorRegiao(CancellationToken ct)
        => Ok(await _risco.ResumoPorRegiaoAsync(ct));

    [HttpGet("ufs")]
    public async Task<ActionResult<IReadOnlyList<ResumoRiscoResponse>>> PorUf([FromQuery] string? regiao, CancellationToken ct)
        => Ok(await _risco.ResumoPorUfAsync(regiao, ct));

    /// <summary>Previsão do modelo de ML para uma escola (pelo código INEP).</summary>
    [HttpGet("escolas/{codigoInep:long}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrevisaoEscolaResponse>> Escola(long codigoInep, CancellationToken ct)
        => Ok(await _risco.ObterEscolaAsync(codigoInep, ct));

    /// <summary>Importa o CSV gerado pelo notebook de ML (APAGA e recarrega a tabela de previsões). Use ?uf=SP para importar só uma UF.</summary>
    [HttpPost("importar")]
    public async Task<ActionResult<ImportacaoResponse>> Importar([FromQuery] string? uf, CancellationToken ct)
    {
        var alvo = _configuracao["Ml:CsvPath"] ?? "data/ml/previsoes_escolas_oof.csv";
        var caminho = LocalizadorArquivos.Localizar(alvo)
            ?? throw new DomainException($"Arquivo de previsões não encontrado: {alvo}");
        return Ok(await _risco.ImportarAsync(caminho, uf, ct));
    }
}
