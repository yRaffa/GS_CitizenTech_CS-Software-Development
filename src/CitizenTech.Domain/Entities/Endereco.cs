using CitizenTech.Domain.Exceptions;
using CitizenTech.Domain.Geografia;

namespace CitizenTech.Domain.Entities;

/// <summary>Objeto de valor: endereço do estabelecimento (mapeado como "owned type" no EF Core).</summary>
public class Endereco
{
    private Endereco() { Logradouro = Bairro = Cidade = Uf = string.Empty; } // EF Core

    public Endereco(string logradouro, string? numero, string? bairro, string cidade, string uf)
    {
        if (string.IsNullOrWhiteSpace(logradouro)) throw new DomainException("Logradouro é obrigatório.");
        if (string.IsNullOrWhiteSpace(cidade)) throw new DomainException("Cidade é obrigatória.");
        if (!Regioes.UfValida(uf)) throw new DomainException($"UF inválida: '{uf}'.");

        Logradouro = logradouro.Trim();
        Numero = numero?.Trim();
        Bairro = bairro?.Trim() ?? string.Empty;
        Cidade = cidade.Trim();
        Uf = uf.Trim().ToUpperInvariant();
    }

    public string Logradouro { get; private set; }
    public string? Numero { get; private set; }
    public string Bairro { get; private set; }
    public string Cidade { get; private set; }
    public string Uf { get; private set; }
}
