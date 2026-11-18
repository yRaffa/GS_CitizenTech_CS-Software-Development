using CitizenTech.Domain.Exceptions;

namespace CitizenTech.Application.Equipe;

public record IntegranteInfo(string NomeCompleto, string Rm, string Turma, string NomeAlunoRm, bool Lider);

/// <summary>Identificação acadêmica da equipe (lida do equipe.json). Não é autenticação.</summary>
public record EquipeInfo(string CitizenTech, string Turma, string RmLider, IReadOnlyList<IntegranteInfo> Integrantes)
{
    /// <summary>Confere se o executor (NomeAluno_RM) está cadastrado na equipe. Lança 400 caso contrário.</summary>
    public string ValidarExecutor(string? executor)
    {
        if (string.IsNullOrWhiteSpace(executor))
            throw new DomainException("Informe o executor (NomeAluno_RM).");
        var achado = Integrantes.FirstOrDefault(i => string.Equals(i.NomeAlunoRm, executor.Trim(), StringComparison.OrdinalIgnoreCase));
        if (achado is null)
            throw new DomainException($"Executor '{executor}' não está cadastrado na equipe (consulte GET /api/equipe).");
        return achado.NomeAlunoRm;
    }
}
