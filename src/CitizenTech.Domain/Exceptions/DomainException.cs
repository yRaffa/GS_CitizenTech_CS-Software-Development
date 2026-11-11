namespace CitizenTech.Domain.Exceptions;

/// <summary>Violação de uma regra de negócio (vira HTTP 400 na API).</summary>
public class DomainException : Exception
{
    public DomainException(string mensagem) : base(mensagem) { }
}
