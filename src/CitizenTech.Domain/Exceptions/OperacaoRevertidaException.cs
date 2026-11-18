namespace CitizenTech.Domain.Exceptions;

/// <summary>
/// Falha CONTROLADA lançada depois das três escritas e antes do COMMIT (prova de ROLLBACK, requisito R6/T08).
/// A transação é revertida e a API responde HTTP 500 identificando o ROLLBACK.
/// </summary>
public class OperacaoRevertidaException : Exception
{
    public OperacaoRevertidaException(string mensagem) : base(mensagem) { }
}
