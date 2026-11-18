namespace CitizenTech.Domain.Exceptions;

/// <summary>Arquivo acima do limite permitido. Vira HTTP 413 na API.</summary>
public class ArquivoGrandeDemaisException : Exception
{
    public ArquivoGrandeDemaisException(string mensagem) : base(mensagem) { }
}
