using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Geografia;
using Microsoft.Extensions.DependencyInjection;

namespace CitizenTech.Desktop.Views;

public class EstabelecimentosView : ViewBase
{
    private readonly DataGridView _grade = UI.Grade("Lista de estabelecimentos");
    private readonly TextBox _nome = new(), _logradouro = new(), _numero = new(), _bairro = new(), _cidade = new(), _inep = new(), _email = new();
    private readonly ComboBox _tipo = UI.Combo(Enum.GetValues<TipoEstabelecimento>());
    private readonly ComboBox _uf = UI.Combo(Regioes.Ufs.OrderBy(u => u));
    private readonly ComboBox _filtroUf = UI.Combo(new[] { "(todas)" }.Concat(Regioes.Ufs.OrderBy(u => u)));
    private List<EstabelecimentoResponse> _itens = new();
    private int? _idEmEdicao;

    public EstabelecimentosView(IServiceScopeFactory fabrica) : base(fabrica)
    {
        var form = UI.Formulario();
        UI.Linha(form, "Nome", _nome);
        UI.Linha(form, "Tipo", _tipo);
        UI.Linha(form, "Logradouro", _logradouro);
        UI.Linha(form, "Número", _numero);
        UI.Linha(form, "Bairro", _bairro);
        UI.Linha(form, "Cidade", _cidade);
        UI.Linha(form, "UF", _uf);
        UI.Linha(form, "Código INEP (escolas)", _inep);
        UI.Linha(form, "E-mail", _email);

        var botoes = UI.Barra(
            UI.Botao("Salvar", async (_, _) => await ExecutarAsync(SalvarAsync)),
            UI.Botao("Novo", (_, _) => LimparFormulario()),
            UI.Botao("Excluir", async (_, _) => await ExecutarAsync(ExcluirAsync)));
        botoes.Dock = DockStyle.Top;

        var esquerda = new Panel { Dock = DockStyle.Left, Width = 400, Padding = new Padding(6) };
        esquerda.Controls.Add(botoes);
        esquerda.Controls.Add(form);

        var filtro = UI.Barra(UI.Rotulo("Filtrar por UF:"), _filtroUf,
            UI.Botao("Atualizar lista", async (_, _) => await ExecutarAsync(CarregarAsync)));
        filtro.Dock = DockStyle.Top;

        var direita = new Panel { Dock = DockStyle.Fill };
        direita.Controls.Add(_grade);   // Fill primeiro
        direita.Controls.Add(filtro);

        Controls.Add(direita);
        Controls.Add(esquerda);
        _grade.SelectionChanged += (_, _) => CarregarSelecionado();
    }

    public override Task AtualizarAsync() => ExecutarAsync(CarregarAsync);

    private async Task CarregarAsync()
    {
        var uf = _filtroUf.SelectedIndex <= 0 ? null : (string?)_filtroUf.SelectedItem;
        var pagina = await NoEscopoAsync(sp => sp.GetRequiredService<IEstabelecimentoService>().ListarAsync(uf, null, 1, 100));
        _itens = pagina.Itens.ToList();
        _grade.DataSource = _itens.Select(e => new { e.Id, e.Nome, Tipo = e.Tipo.ToString(), e.Cidade, e.Uf, e.Regiao, INEP = e.CodigoInep }).ToList();
    }

    private void CarregarSelecionado()
    {
        if (_grade.CurrentRow is null || _grade.CurrentRow.Index < 0 || _grade.CurrentRow.Index >= _itens.Count) return;
        var e = _itens[_grade.CurrentRow.Index];
        _idEmEdicao = e.Id;
        _nome.Text = e.Nome; _tipo.SelectedItem = e.Tipo; _logradouro.Text = e.Logradouro; _numero.Text = e.Numero;
        _bairro.Text = e.Bairro; _cidade.Text = e.Cidade; _uf.SelectedItem = e.Uf;
        _inep.Text = e.CodigoInep?.ToString() ?? string.Empty; _email.Text = e.Email ?? string.Empty;
    }

    private EstabelecimentoRequest LerFormulario() => new()
    {
        Nome = _nome.Text, Tipo = (TipoEstabelecimento)_tipo.SelectedItem!, Logradouro = _logradouro.Text,
        Numero = _numero.Text, Bairro = _bairro.Text, Cidade = _cidade.Text, Uf = (string)_uf.SelectedItem!,
        CodigoInep = long.TryParse(_inep.Text, out var inep) ? inep : null,
        Email = string.IsNullOrWhiteSpace(_email.Text) ? null : _email.Text
    };

    private async Task SalvarAsync()
    {
        var request = LerFormulario();
        var id = _idEmEdicao;
        await ExecutarNoEscopoAsync(sp => id is null
            ? sp.GetRequiredService<IEstabelecimentoService>().CriarAsync(request)
            : sp.GetRequiredService<IEstabelecimentoService>().AtualizarAsync(id.Value, request));
        LimparFormulario();
        await CarregarAsync();
        Info("Estabelecimento salvo.");
    }

    private async Task ExcluirAsync()
    {
        if (_idEmEdicao is null) { Aviso("Selecione um estabelecimento na lista."); return; }
        var resposta = MessageBox.Show("Excluir o estabelecimento e suas avaliações/diagnósticos?", "Confirmar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (resposta != DialogResult.Yes) return;
        var id = _idEmEdicao.Value;
        await ExecutarNoEscopoAsync(sp => sp.GetRequiredService<IEstabelecimentoService>().RemoverAsync(id));
        LimparFormulario();
        await CarregarAsync();
    }

    private void LimparFormulario()
    {
        _idEmEdicao = null;
        foreach (var t in new[] { _nome, _logradouro, _numero, _bairro, _cidade, _inep, _email }) t.Clear();
        _nome.Focus();
    }
}
