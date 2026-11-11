using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace CitizenTech.Desktop.Views;

public class DiagnosticoView : ViewBase
{
    private static readonly Dictionary<ItemAcessibilidade, string> Rotulos = new()
    {
        [ItemAcessibilidade.Rampa] = "Rampa de acesso",
        [ItemAcessibilidade.Corrimao] = "Corrimão",
        [ItemAcessibilidade.BanheiroAdaptado] = "Banheiro adaptado",
        [ItemAcessibilidade.PisoTatil] = "Piso tátil",
        [ItemAcessibilidade.SinalizacaoVisual] = "Sinalização visual",
        [ItemAcessibilidade.Elevador] = "Elevador",
        [ItemAcessibilidade.VagaPreferencial] = "Vaga preferencial",
        [ItemAcessibilidade.AtendimentoLibras] = "Atendimento em Libras"
    };

    private readonly ComboBox _estabelecimento = new() { DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Nome" };
    private readonly Dictionary<ItemAcessibilidade, ComboBox> _respostas = new();
    private readonly Dictionary<TipoMedicao, (CheckBox Usar, NumericUpDown Valor)> _medicoes = new();
    private readonly TextBox _observacoes = new();
    private readonly Label _nivel = new() { AutoSize = true, Font = new Font("Segoe UI", 12f, FontStyle.Bold), Padding = new Padding(8, 4, 8, 4) };
    private readonly Label _contexto = new() { AutoSize = false, Height = 54, Dock = DockStyle.Top };
    private readonly Label _aviso = new() { AutoSize = false, Height = 40, Dock = DockStyle.Bottom, ForeColor = Color.DimGray };
    private readonly DataGridView _gradeBarreiras = UI.Grade("Barreiras prováveis");
    private readonly DataGridView _gradeParceiros = UI.Grade("Parceiros sugeridos");
    private List<EstabelecimentoResponse> _estabelecimentos = new();
    private List<ParceiroSugeridoResponse> _sugeridos = new();
    private DiagnosticoResponse? _diagnostico;

    public DiagnosticoView(IServiceScopeFactory fabrica) : base(fabrica)
    {
        var form = UI.Formulario();
        UI.Linha(form, "Estabelecimento", _estabelecimento);
        foreach (var item in Enum.GetValues<ItemAcessibilidade>())
        {
            var combo = UI.Combo(new[] { "Não sei", "Sim", "Não" });
            _respostas[item] = combo;
            UI.Linha(form, Rotulos[item], combo);
        }
        AdicionarMedicao(form, TipoMedicao.InclinacaoRampaPercentual, "Inclinação da rampa (%)");
        AdicionarMedicao(form, TipoMedicao.LarguraPortaCm, "Largura da porta (cm)");
        AdicionarMedicao(form, TipoMedicao.LarguraCorredorCm, "Largura do corredor (cm)");
        UI.Linha(form, "Observações", _observacoes);

        var botoes = UI.Barra(
            UI.Botao("Gerar diagnóstico", async (_, _) => await ExecutarAsync(GerarAsync)),
            UI.Botao("Recarregar estabelecimentos", async (_, _) => await ExecutarAsync(CarregarEstabelecimentosAsync)));

        var esquerda = new Panel { Dock = DockStyle.Left, Width = 430, AutoScroll = true, Padding = new Padding(6) };
        esquerda.Controls.Add(botoes);
        esquerda.Controls.Add(form);

        var topo = new Panel { Dock = DockStyle.Top, Height = 64 };
        topo.Controls.Add(_contexto);
        var cabecalho = UI.Barra(UI.Rotulo("Nível de risco da triagem:"), _nivel);
        topo.Controls.Add(cabecalho);

        var meio = new GroupBox { Text = "Barreiras prováveis", Dock = DockStyle.Fill };
        meio.Controls.Add(_gradeBarreiras);

        var baixo = new GroupBox { Text = "Parceiros sugeridos (mesma UF e especialidade necessária)", Dock = DockStyle.Bottom, Height = 230 };
        baixo.Controls.Add(_gradeParceiros);
        baixo.Controls.Add(UI.Barra(UI.Botao("Encaminhar ao parceiro selecionado", async (_, _) => await ExecutarAsync(EncaminharAsync))));

        var direita = new Panel { Dock = DockStyle.Fill };
        direita.Controls.Add(meio);
        direita.Controls.Add(baixo);
        direita.Controls.Add(_aviso);
        direita.Controls.Add(topo);

        Controls.Add(direita);
        Controls.Add(esquerda);
    }

    private void AdicionarMedicao(TableLayoutPanel form, TipoMedicao tipo, string rotulo)
    {
        var usar = new CheckBox { Text = "Informar", AutoSize = true, AccessibleName = $"Informar {rotulo}" };
        var valor = new NumericUpDown { DecimalPlaces = 2, Minimum = 0, Maximum = 1000, Width = 90, Enabled = false, AccessibleName = rotulo };
        usar.CheckedChanged += (_, _) => valor.Enabled = usar.Checked;
        _medicoes[tipo] = (usar, valor);
        var linha = new FlowLayoutPanel { AutoSize = true };
        linha.Controls.Add(usar);
        linha.Controls.Add(valor);
        UI.Linha(form, rotulo, linha);
    }

    public override Task AtualizarAsync() => ExecutarAsync(CarregarEstabelecimentosAsync);

    private async Task CarregarEstabelecimentosAsync()
    {
        var pagina = await NoEscopoAsync(sp => sp.GetRequiredService<IEstabelecimentoService>().ListarAsync(null, null, 1, 100));
        _estabelecimentos = pagina.Itens.ToList();
        _estabelecimento.DataSource = _estabelecimentos;
        _estabelecimento.DisplayMember = "Nome";
    }

    private AvaliacaoRequest LerAvaliacao()
    {
        var respostas = new List<RespostaDto>();
        foreach (var (item, combo) in _respostas)
        {
            var valor = combo.SelectedIndex switch { 1 => Resposta.Sim, 2 => Resposta.Nao, _ => Resposta.NaoSei };
            respostas.Add(new RespostaDto(item, valor));
        }
        var medicoes = _medicoes.Where(m => m.Value.Usar.Checked && m.Value.Valor.Value > 0)
            .Select(m => new MedicaoDto(m.Key, (double)m.Value.Valor.Value)).ToList();
        return new AvaliacaoRequest { Respostas = respostas, Medicoes = medicoes, Observacoes = _observacoes.Text };
    }

    private async Task GerarAsync()
    {
        if (_estabelecimento.SelectedItem is not EstabelecimentoResponse est)
        {
            Aviso("Cadastre e selecione um estabelecimento primeiro.");
            return;
        }
        var request = LerAvaliacao();
        var (diagnostico, sugeridos) = await NoEscopoAsync(async sp =>
        {
            var avaliacao = await sp.GetRequiredService<IAvaliacaoService>().CriarAsync(est.Id, request);
            var diag = await sp.GetRequiredService<IDiagnosticoService>().GerarAsync(avaliacao.Id);
            var parceiros = await sp.GetRequiredService<IParceiroService>().SugerirParaDiagnosticoAsync(diag.Id);
            return (diag, parceiros);
        });

        _diagnostico = diagnostico;
        _sugeridos = sugeridos.ToList();
        Mostrar(diagnostico);
    }

    private void Mostrar(DiagnosticoResponse d)
    {
        // O nível aparece em TEXTO e em cor (não depender só de cor)
        _nivel.Text = $"  {d.NivelRisco.ToString().ToUpperInvariant()}  ";
        _nivel.ForeColor = Color.White;
        _nivel.BackColor = d.NivelRisco switch { NivelRisco.Alto => Color.Firebrick, NivelRisco.Medio => Color.DarkOrange, _ => Color.SeaGreen };

        var ml = d.ProbabilidadeSemRampaMl is double p
            ? $"Modelo de ML: {p * 100:0}% de chance de faltar rampa (faixa {d.FaixaRiscoMl}). " : "Sem previsão individual do modelo de ML (informe o código INEP para escolas). ";
        _contexto.Text = ml + (d.ContextoRegional ?? "Importe as previsões do ML na aba \"Risco regional\" para ver o contexto da UF.");
        _aviso.Text = d.Aviso;

        _gradeBarreiras.DataSource = d.Barreiras.Select(b => new { Gravidade = b.Gravidade.ToString(), Tipo = b.Tipo.ToString(), b.Descricao, b.Recomendacao }).ToList();
        _gradeParceiros.DataSource = _sugeridos.Select(s => new
        {
            s.Parceiro.Nome, Compativel = s.EspecialidadesCompativeis.ToString(), Aderencia = s.Aderencia, UFs = s.Parceiro.UfsAtendidas, s.Parceiro.Email
        }).ToList();
    }

    private async Task EncaminharAsync()
    {
        if (_diagnostico is null) { Aviso("Gere um diagnóstico primeiro."); return; }
        var indice = _gradeParceiros.CurrentRow?.Index ?? -1;
        if (indice < 0 || indice >= _sugeridos.Count) { Aviso("Selecione um parceiro na lista."); return; }

        var request = new EncaminhamentoRequest { DiagnosticoId = _diagnostico.Id, ParceiroId = _sugeridos[indice].Parceiro.Id, Observacao = "Encaminhado pelo app Windows Forms" };
        var criado = await NoEscopoAsync(sp => sp.GetRequiredService<IEncaminhamentoService>().CriarAsync(request));
        Info($"Encaminhamento #{criado.Id} criado para {criado.ParceiroNome} (status {criado.Status}).");
    }
}
