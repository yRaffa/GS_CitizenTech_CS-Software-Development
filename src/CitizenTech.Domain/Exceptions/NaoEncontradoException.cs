namespace CitizenTech.Domain.Exceptions;

/// <summary>Registro inexistente (vira HTTP 404 na API).</summary>
public class NaoEncontradoException : Exception
{
    public NaoEncontradoException(string entidade, object id)
        : base($"{entidade} com identificador '{id}' não foi encontrado(a).") { }
}
