using CitizenTech.Desktop.Views;
using CitizenTech.Infrastructure.Persistencia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CitizenTech.Desktop;

public class MainForm : Form
{
    public MainForm(IServiceScopeFactory fabrica, IConfiguration configuracao, InformacoesBanco banco)
    {
        Text = "CitizenTech · Acessibilidade — triagem e conexão com parceiros";
        Font = new Font("Segoe UI", 10f);          // fonte legível (acessibilidade)
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 680);
        Size = new Size(1250, 780);

        var abas = new TabControl { Dock = DockStyle.Fill, AccessibleName = "Seções do sistema" };
        var views = new (string Titulo, ViewBase View)[]
        {
            ("Estabelecimentos", new EstabelecimentosView(fabrica)),
            ("Avaliação e diagnóstico", new DiagnosticoView(fabrica)),
            ("Parceiros", new ParceirosView(fabrica)),
            ("Encaminhamentos", new EncaminhamentosView(fabrica)),
            ("Risco regional (ML)", new RiscoView(fabrica, configuracao)),
            ("Banco de dados", new BancoView(fabrica, banco))
        };
        foreach (var (titulo, view) in views)
        {
            var aba = new TabPage(titulo);
            aba.Controls.Add(view);
            abas.TabPages.Add(aba);
        }
        abas.SelectedIndexChanged += async (_, _) =>
        {
            if (abas.SelectedTab?.Controls[0] is ViewBase v) await v.AtualizarAsync();
        };

        var status = new StatusStrip();
        status.Items.Add(new ToolStripStatusLabel($"Banco SQLite: {banco.Caminho}"));

        Controls.Add(abas);     // Fill primeiro, depois os controles encaixados nas bordas
        Controls.Add(status);

        Shown += async (_, _) => await ((ViewBase)abas.TabPages[0].Controls[0]).AtualizarAsync();
    }
}
