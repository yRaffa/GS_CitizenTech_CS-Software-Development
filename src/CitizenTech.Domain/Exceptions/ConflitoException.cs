namespace CitizenTech.Domain.Exceptions;

/// <summary>Conflito com o estado atual (ex.: saldo insuficiente). Vira HTTP 409 na API.</summary>
public class ConflitoException : Exception
{
    public ConflitoException(string mensagem) : base(mensagem) { }
}
