using CitizenTech.Domain.Exceptions;

namespace CitizenTech.Domain.Geografia;

/// <summary>Mapeia UF -> região. Os nomes seguem exatamente o Censo Escolar usado no modelo de ML.</summary>
public static class Regioes
{
    private static readonly Dictionary<string, string> PorUf = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AC"] = "Norte", ["AP"] = "Norte", ["AM"] = "Norte", ["PA"] = "Norte", ["RO"] = "Norte", ["RR"] = "Norte", ["TO"] = "Norte",
        ["AL"] = "Nordeste", ["BA"] = "Nordeste", ["CE"] = "Nordeste", ["MA"] = "Nordeste", ["PB"] = "Nordeste",
        ["PE"] = "Nordeste", ["PI"] = "Nordeste", ["RN"] = "Nordeste", ["SE"] = "Nordeste",
        ["DF"] = "Centro-Oeste", ["GO"] = "Centro-Oeste", ["MT"] = "Centro-Oeste", ["MS"] = "Centro-Oeste",
        ["ES"] = "Sudeste", ["MG"] = "Sudeste", ["RJ"] = "Sudeste", ["SP"] = "Sudeste",
        ["PR"] = "Sul", ["RS"] = "Sul", ["SC"] = "Sul"
    };

    public static IReadOnlyCollection<string> Ufs => PorUf.Keys;

    public static bool UfValida(string? uf) => !string.IsNullOrWhiteSpace(uf) && PorUf.ContainsKey(uf.Trim());

    public static string DaUf(string uf) =>
        UfValida(uf) ? PorUf[uf.Trim()] : throw new DomainException($"UF inválida: '{uf}'.");
}
