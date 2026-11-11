using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace CitizenTech.Desktop.Views;

public class EncaminhamentosView : ViewBase
{
    private readonly DataGridView _grade = UI.Grade("Lista de encaminhamentos");
    private readonly ComboBox _filtro = UI.Combo(new[] { "(todos)" }.Concat(Enum.GetNames<StatusEncaminhamento>()));
    private readonly ComboBox _novoStatus = UI.Combo(Enum.GetValues<StatusEncaminhamento>());
    private List<EncaminhamentoResponse> _itens = new();

    public EncaminhamentosView(IServiceScopeFactory fabrica) : base(fabrica)
    {
        var barra = UI.Barra(
            UI.Rotulo("Status:"), _filtro,
            UI.Botao("Atualizar", async (_, _) => await ExecutarAsync(CarregarAsync)),
            UI.Rotulo("   Alterar o selecionado para:"), _novoStatus,
            UI.Botao("Aplicar", async (_, _) => await ExecutarAsync(AlterarAsync)));

        Controls.Add(_grade);
        Controls.Add(barra);
    }

    public override Task AtualizarAsync() => ExecutarAsync(CarregarAsync);

    private async Task CarregarAsync()
    {
        StatusEncaminhamento? status = _filtro.SelectedIndex <= 0 ? null : Enum.Parse<StatusEncaminhamento>((string)_filtro.SelectedItem!);
        _itens = (await NoEscopoAsync(sp => sp.GetRequiredService<IEncaminhamentoService>().ListarAsync(status))).ToList();
        _grade.DataSource = _itens.Select(e => new { e.Id, Parceiro = e.ParceiroNome, Diagnostico = e.DiagnosticoId, Status = e.Status.ToString(), Atualizado = e.DataAtualizacao.ToLocalTime().ToString("g"), e.Observacao }).ToList();
    }

    private async Task AlterarAsync()
    {
        var indice = _grade.CurrentRow?.Index ?? -1;
        if (indice < 0 || indice >= _itens.Count) { Aviso("Selecione um encaminhamento."); return; }
        var request = new AlterarStatusRequest { Status = (StatusEncaminhamento)_novoStatus.SelectedItem! };
        var id = _itens[indice].Id;
        await ExecutarNoEscopoAsync(sp => sp.GetRequiredService<IEncaminhamentoService>().AlterarStatusAsync(id, request));
        await CarregarAsync();
    }
}
