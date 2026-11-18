using CitizenTech.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace CitizenTech.Infrastructure.Persistencia;

/// <summary>Adapta a transação local do EF Core/SQL Server à interface do domínio.</summary>
public sealed class TransacaoEf : ITransacaoBanco
{
    private readonly IDbContextTransaction _transacao;
    public TransacaoEf(IDbContextTransaction transacao) => _transacao = transacao;

    public Task CommitAsync(CancellationToken ct = default) => _transacao.CommitAsync(ct);
    public Task RollbackAsync(CancellationToken ct = default) => _transacao.RollbackAsync(ct);
    public ValueTask DisposeAsync() => _transacao.DisposeAsync(); // sem commit, o dispose também reverte
}
