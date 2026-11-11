using System.Globalization;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;

namespace CitizenTech.Application.Csv;

/// <summary>
/// Converte uma linha do arquivo gerado pelo notebook de ML (saidas/previsoes_escolas_oof.csv):
/// co_entidade,regiao,sg_uf,co_municipio,tp_dependencia,prob_sem_rampa,sem_rampa_declarado,faixa_risco
/// </summary>
public static class PrevisaoCsvParser
{
    public static PrevisaoRisco? Converter(string? linha)
    {
        if (string.IsNullOrWhiteSpace(linha)) return null;
        var c = linha.Split(',');
        if (c.Length < 8) return null;

        var inv = CultureInfo.InvariantCulture;
        if (!long.TryParse(c[0], NumberStyles.Integer, inv, out var inep) ||
            !int.TryParse(c[3], NumberStyles.Integer, inv, out var municipio) ||
            !int.TryParse(c[4], NumberStyles.Integer, inv, out var dependencia) ||
            !double.TryParse(c[5], NumberStyles.Float, inv, out var prob))
            return null;

        var declarado = c[6].Trim() is "1" or "true" or "True";

        FaixaRisco? faixa = c[7].Trim().ToUpperInvariant() switch
        {
            "ALTA" => FaixaRisco.Alta,
            "MEDIA" => FaixaRisco.Media,
            "BAIXA" => FaixaRisco.Baixa,
            _ => null
        };
        if (faixa is null) return null;

        return new PrevisaoRisco(inep, c[1].Trim(), c[2].Trim().ToUpperInvariant(), municipio, dependencia, prob, declarado, faixa.Value);
    }
}
