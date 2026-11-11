using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Exceptions;

namespace CitizenTech.Domain.Entities;

/// <summary>Possível barreira de acessibilidade identificada na triagem.</summary>
public class Barreira
{
    private Barreira() { Descricao = Recomendacao = string.Empty; } // EF Core

    public Barreira(TipoBarreira tipo, string descricao, Gravidade gravidade, Especialidade especialidadeSugerida, string recomendacao)
    {
        if (string.IsNullOrWhiteSpace(descricao)) throw new DomainException("Descrição da barreira é obrigatória.");
        Tipo = tipo;
        Descricao = descricao.Trim();
        Gravidade = gravidade;
        EspecialidadeSugerida = especialidadeSugerida;
        Recomendacao = recomendacao?.Trim() ?? string.Empty;
    }

    public int Id { get; private set; }
    public int DiagnosticoId { get; private set; }
    public TipoBarreira Tipo { get; private set; }
    public string Descricao { get; private set; }
    public Gravidade Gravidade { get; private set; }
    public Especialidade EspecialidadeSugerida { get; private set; }
    public string Recomendacao { get; private set; }
}
