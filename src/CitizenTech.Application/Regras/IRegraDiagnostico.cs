using CitizenTech.Domain.Entities;

namespace CitizenTech.Application.Regras;

/// <summary>Contrato de uma regra de triagem: lê a avaliação e devolve zero ou mais barreiras prováveis.</summary>
public interface IRegraDiagnostico
{
    IEnumerable<Barreira> Avaliar(Avaliacao avaliacao);
}
