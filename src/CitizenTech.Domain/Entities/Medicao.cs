using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Exceptions;

namespace CitizenTech.Domain.Entities;

public class Medicao
{
    private Medicao() { } // EF Core

    public Medicao(TipoMedicao tipo, double valor)
    {
        if (valor <= 0) throw new DomainException("O valor da medição deve ser maior que zero.");
        Tipo = tipo;
        Valor = valor;
    }

    public int Id { get; private set; }
    public int AvaliacaoId { get; private set; }
    public TipoMedicao Tipo { get; private set; }
    public double Valor { get; private set; }

    public void Alterar(double valor)
    {
        if (valor <= 0) throw new DomainException("O valor da medição deve ser maior que zero.");
        Valor = valor;
    }
}
