using CitizenTech.Domain.Enums;

namespace CitizenTech.Domain.Entities;

public class RespostaItem
{
    private RespostaItem() { } // EF Core

    public RespostaItem(ItemAcessibilidade item, Resposta valor)
    {
        Item = item;
        Valor = valor;
    }

    public int Id { get; private set; }
    public int AvaliacaoId { get; private set; }
    public ItemAcessibilidade Item { get; private set; }
    public Resposta Valor { get; private set; }

    public void Alterar(Resposta valor) => Valor = valor;
}
