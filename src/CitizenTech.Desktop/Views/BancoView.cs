using CitizenTech.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CitizenTech.Desktop.Views;

/// <summary>Mostra onde está o banco e quantos registros existem (para conferir no DBeaver).</summary>
public class BancoView : ViewBase
{
    private readonly InformacoesBanco _banco;
    private readonly DataGridView _contagens = UI.Grade("Registros por tabela");

    public BancoView(IServiceScopeFactory fabrica, InformacoesBanco banco) : base(fabrica)
    {
        _banco = banco;
        var caminho = new TextBox { Text = banco.Caminho, ReadOnly = true, Dock = DockStyle.Top, AccessibleName = "Caminho do arquivo SQLite" };
        var botoes = UI.Barra(
            UI.Botao("Copiar caminho", (_, _) => { Clipboard.SetText(banco.Caminho); Info("Caminho copiado."); }),
            UI.Botao("Abrir pasta do banco", (_, _) => System.Diagnostics.Process.Start("explorer.exe", $"\"{Path.GetDirectoryName(banco.Caminho)}\"")),
            UI.Botao("Atualizar contagens", async (_, _) => await ExecutarAsync(CarregarAsync)));

        var ajuda = new Label
        {
            Dock = DockStyle.Top, AutoSize = false, Height = 118, Padding = new Padding(6),
            Text = "Como ver o banco no DBeaver:\n" +
                   "1) Novo banco de dados (Ctrl+Shift+N) → SQLite → Avançar.\n" +
                   "2) Em \"Path\", cole o caminho acima (o botão \"Copiar caminho\" ajuda) → Finalizar (baixe o driver se o DBeaver pedir).\n" +
                   "3) Abra Tables: Estabelecimentos, Avaliacoes, Diagnosticos, Barreiras, Parceiros, Encaminhamentos, PrevisoesRisco.\n" +
                   "Dica: com o app aberto, use o DBeaver apenas para consultar (SELECT)."
        };

        var grupo = new GroupBox { Text = "Registros por tabela", Dock = DockStyle.Fill };
        grupo.Controls.Add(_contagens);

        Controls.Add(grupo);
        Controls.Add(ajuda);
        Controls.Add(botoes);
        Controls.Add(caminho);
    }

    public override Task AtualizarAsync() => ExecutarAsync(CarregarAsync);

    private async Task CarregarAsync()
    {
        var linhas = await NoEscopoAsync(async sp =>
        {
            var db = sp.GetRequiredService<AppDbContext>();
            return new List<(string Tabela, int Registros)>
            {
                ("Estabelecimentos", await db.Estabelecimentos.CountAsync()),
                ("Avaliacoes", await db.Avaliacoes.CountAsync()),
                ("Diagnosticos", await db.Diagnosticos.CountAsync()),
                ("Barreiras", await db.Barreiras.CountAsync()),
                ("Parceiros", await db.Parceiros.CountAsync()),
                ("Encaminhamentos", await db.Encaminhamentos.CountAsync()),
                ("PrevisoesRisco", await db.Previsoes.CountAsync())
            };
        });
        _contagens.DataSource = linhas.Select(l => new { l.Tabela, l.Registros }).ToList();
    }
}
