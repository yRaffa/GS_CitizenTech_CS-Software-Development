using CitizenTech.Domain.Enums;

namespace CitizenTech.Domain.Entities;

/// <summary>Coleta de informações (questionário + medições) sobre um estabelecimento.</summary>
public class Avaliacao
{
    private readonly List<RespostaItem> _respostas = new();
    private readonly List<Medicao> _medicoes = new();

    private Avaliacao() { } // EF Core

    public Avaliacao(int estabelecimentoId, string? observacoes = null)
    {
        EstabelecimentoId = estabelecimentoId;
        Observacoes = observacoes?.Trim();
        DataAvaliacao = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public int EstabelecimentoId { get; private set; }
    public Estabelecimento? Estabelecimento { get; private set; }
    public DateTime DataAvaliacao { get; private set; }
    public string? Observacoes { get; private set; }

    public IReadOnlyCollection<RespostaItem> Respostas => _respostas;
    public IReadOnlyCollection<Medicao> Medicoes => _medicoes;

    /// <summary>Registra (ou substitui) a resposta de um item do questionário.</summary>
    public void RegistrarResposta(ItemAcessibilidade item, Resposta valor)
    {
        var existente = _respostas.FirstOrDefault(r => r.Item == item);
        if (existente is null) _respostas.Add(new RespostaItem(item, valor));
        else existente.Alterar(valor);
    }

    /// <summary>Registra (ou substitui) uma medição guiada.</summary>
    public void RegistrarMedicao(TipoMedicao tipo, double valor)
    {
        var existente = _medicoes.FirstOrDefault(m => m.Tipo == tipo);
        if (existente is null) _medicoes.Add(new Medicao(tipo, valor));
        else existente.Alterar(valor);
    }

    /// <summary>Itens não respondidos contam como "Não sei".</summary>
    public Resposta ObterResposta(ItemAcessibilidade item) =>
        _respostas.FirstOrDefault(r => r.Item == item)?.Valor ?? Resposta.NaoSei;

    public double? ObterMedicao(TipoMedicao tipo) =>
        _medicoes.FirstOrDefault(m => m.Tipo == tipo)?.Valor;
}
