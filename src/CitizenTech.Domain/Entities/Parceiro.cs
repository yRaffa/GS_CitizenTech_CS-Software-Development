using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Exceptions;
using CitizenTech.Domain.Geografia;

namespace CitizenTech.Domain.Entities;

/// <summary>Organização especializada que avalia e atende as necessidades identificadas.</summary>
public class Parceiro
{
    public const string TodoBrasil = "*";

    private Parceiro() { Nome = Descricao = Email = UfsAtendidas = string.Empty; } // EF Core

    public Parceiro(string nome, string descricao, Especialidade especialidades, IEnumerable<string> ufsAtendidas, string email, string? telefone = null)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("Nome do parceiro é obrigatório.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("E-mail do parceiro é obrigatório.");
        if (especialidades == Especialidade.Nenhuma) throw new DomainException("Informe ao menos uma especialidade.");

        Nome = nome.Trim();
        Descricao = descricao?.Trim() ?? string.Empty;
        Especialidades = especialidades;
        UfsAtendidas = NormalizarUfs(ufsAtendidas);
        Email = email.Trim();
        Telefone = telefone?.Trim();
        Ativo = true;
    }

    public int Id { get; private set; }
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public Especialidade Especialidades { get; private set; }

    /// <summary>Formato ";SP;RJ;" (permite busca exata por UF) ou "*" para todo o Brasil.</summary>
    public string UfsAtendidas { get; private set; }
    public string Email { get; private set; }
    public string? Telefone { get; private set; }
    public bool Ativo { get; private set; }

    public static string ChaveUf(string uf) => $";{uf.Trim().ToUpperInvariant()};";

    public bool Atende(string uf) =>
        Ativo && (UfsAtendidas == TodoBrasil || UfsAtendidas.Contains(ChaveUf(uf), StringComparison.Ordinal));

    public void Desativar() => Ativo = false;

    private static string NormalizarUfs(IEnumerable<string> ufs)
    {
        var lista = ufs?.Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u.Trim().ToUpperInvariant()).Distinct().ToList()
                    ?? new List<string>();
        if (lista.Contains(TodoBrasil)) return TodoBrasil;
        if (lista.Count == 0) throw new DomainException("Informe ao menos uma UF atendida (ou '*' para todo o Brasil).");
        var invalida = lista.FirstOrDefault(u => !Regioes.UfValida(u));
        if (invalida is not null) throw new DomainException($"UF inválida: '{invalida}'.");
        return ";" + string.Join(";", lista) + ";";
    }
}
