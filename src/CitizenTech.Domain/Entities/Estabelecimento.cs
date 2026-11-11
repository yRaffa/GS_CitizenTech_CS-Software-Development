using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Exceptions;
using CitizenTech.Domain.Geografia;

namespace CitizenTech.Domain.Entities;

/// <summary>Local (escola, loja, unidade de saúde...) que será avaliado pela plataforma.</summary>
public class Estabelecimento
{
    private Estabelecimento() { Nome = string.Empty; Endereco = null!; } // EF Core

    public Estabelecimento(string nome, TipoEstabelecimento tipo, Endereco endereco, long? codigoInep = null, string? email = null)
    {
        Nome = ValidarNome(nome);
        Tipo = tipo;
        Endereco = endereco ?? throw new DomainException("Endereço é obrigatório.");
        CodigoInep = codigoInep;
        Email = email?.Trim();
        DataCadastro = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public string Nome { get; private set; }
    public TipoEstabelecimento Tipo { get; private set; }
    public Endereco Endereco { get; private set; }

    /// <summary>Código INEP (somente escolas). Permite cruzar com a previsão do modelo de ML.</summary>
    public long? CodigoInep { get; private set; }
    public string? Email { get; private set; }
    public DateTime DataCadastro { get; private set; }

    /// <summary>Região derivada da UF (propriedade calculada, não vai para o banco).</summary>
    public string Regiao => Regioes.DaUf(Endereco.Uf);

    public void Atualizar(string nome, TipoEstabelecimento tipo, Endereco endereco, long? codigoInep, string? email)
    {
        Nome = ValidarNome(nome);
        Tipo = tipo;
        Endereco = endereco ?? throw new DomainException("Endereço é obrigatório.");
        CodigoInep = codigoInep;
        Email = email?.Trim();
    }

    private static string ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("Nome do estabelecimento é obrigatório.");
        if (nome.Trim().Length > 150) throw new DomainException("Nome deve ter no máximo 150 caracteres.");
        return nome.Trim();
    }
}
