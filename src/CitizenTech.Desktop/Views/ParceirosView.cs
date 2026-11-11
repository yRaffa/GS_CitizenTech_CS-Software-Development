using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace CitizenTech.Desktop.Views;

public class ParceirosView : ViewBase
{
    private readonly DataGridView _grade = UI.Grade("Lista de parceiros");
    private readonly TextBox _nome = new(), _descricao = new(), _ufs = new() { Text = "*" }, _email = new(), _telefone = new();
    private readonly CheckedListBox _especialidades = new() { Height = 110, CheckOnClick = true };

    public ParceirosView(IServiceScopeFactory fabrica) : base(fabrica)
    {
        foreach (var e in Enum.GetValues<Especialidade>().Where(e => e != Especialidade.Nenhuma)) _especialidades.Items.Add(e);

        var form = UI.Formulario();
        UI.Linha(form, "Nome", _nome);
        UI.Linha(form, "Descrição", _descricao);
        UI.Linha(form, "Especialidades", _especialidades);
        UI.Linha(form, "UFs atendidas (ex.: SP,RJ ou *)", _ufs);
        UI.Linha(form, "E-mail", _email);
        UI.Linha(form, "Telefone", _telefone);

        var esquerda = new Panel { Dock = DockStyle.Left, Width = 430, Padding = new Padding(6) };
        esquerda.Controls.Add(UI.Barra(UI.Botao("Adicionar parceiro", async (_, _) => await ExecutarAsync(AdicionarAsync))));
        esquerda.Controls.Add(form);

        Controls.Add(_grade);
        Controls.Add(esquerda);
    }

    public override Task AtualizarAsync() => ExecutarAsync(CarregarAsync);

    private async Task CarregarAsync()
    {
        var lista = await NoEscopoAsync(sp => sp.GetRequiredService<IParceiroService>().ListarAsync(null, null));
        _grade.DataSource = lista.Select(p => new { p.Id, p.Nome, Especialidades = p.Especialidades.ToString(), UFs = p.UfsAtendidas, p.Email }).ToList();
    }

    private async Task AdicionarAsync()
    {
        var especialidades = _especialidades.CheckedItems.Cast<Especialidade>().Aggregate(Especialidade.Nenhuma, (a, e) => a | e);
        var request = new ParceiroRequest
        {
            Nome = _nome.Text, Descricao = _descricao.Text, Especialidades = especialidades,
            UfsAtendidas = _ufs.Text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            Email = _email.Text, Telefone = _telefone.Text
        };
        await ExecutarNoEscopoAsync(sp => sp.GetRequiredService<IParceiroService>().CriarAsync(request));
        await CarregarAsync();
        Info("Parceiro adicionado.");
    }
}
