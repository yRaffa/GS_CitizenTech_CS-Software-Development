using CitizenTech.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CitizenTech.Api.Middleware;

/// <summary>Converte exceções de domínio em respostas HTTP padronizadas (ProblemDetails).</summary>
public class TratamentoDeErrosMiddleware
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<TratamentoDeErrosMiddleware> _logger;

    public TratamentoDeErrosMiddleware(RequestDelegate proximo, ILogger<TratamentoDeErrosMiddleware> logger)
    {
        _proximo = proximo;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (NaoEncontradoException ex)
        {
            await ResponderAsync(contexto, StatusCodes.Status404NotFound, "Recurso não encontrado", ex.Message);
        }
        catch (DomainException ex)
        {
            await ResponderAsync(contexto, StatusCodes.Status400BadRequest, "Regra de negócio violada", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado em {Metodo} {Caminho}", contexto.Request.Method, contexto.Request.Path);
            await ResponderAsync(contexto, StatusCodes.Status500InternalServerError, "Erro interno", "Ocorreu um erro inesperado.");
        }
    }

    private static Task ResponderAsync(HttpContext contexto, int status, string titulo, string detalhe)
    {
        contexto.Response.StatusCode = status;
        return contexto.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = titulo, Detail = detalhe });
    }
}
