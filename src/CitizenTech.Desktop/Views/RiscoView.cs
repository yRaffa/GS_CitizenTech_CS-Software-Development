using CitizenTech.Application.Interfaces;
using CitizenTech.Application.Utilitarios;
using CitizenTech.Domain.Geografia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CitizenTech.Desktop.Views;

/// <summary>Integração com a entrega de IA &amp; ML: risco de falta de rampa por região e UF.</summary>
public class RiscoView : ViewBase
{
    private readonly IConfiguration _configuracao;
    private readonly Label _status = new() { AutoSize = true, Margin = new Padding(8, 7, 3, 3) };
    private readonly ComboBox _uf = UI.Combo(new[] { "(todas as UFs)" }.Concat(Regioes.Ufs.OrderBy(u => u)));
    private readonly DataGridView _regioes = UI.Grade("Resumo por região");
    private readonly DataGridView _ufs = UI.Grade("Resumo por UF");

    public RiscoView(IServiceScopeFactory fabrica, IConfiguration configuracao) : base(fabrica)
    {
        _configuracao = configuracao;
        var barra = UI.Barra(
            UI.Rotulo("Importar:"), _uf,
            UI.Botao("Importar previsões do ML (CSV)", async (_, _) => await ExecutarAsync(ImportarAsync)),
            UI.Botao("Atualizar", async (_, _) => await ExecutarAsync(CarregarAsync)), _status);

        var grupoRegioes = new GroupBox { Text = "Por região", Dock = DockStyle.Top, Height = 190 };
        grupoRegioes.Controls.Add(_regioes);
        var grupoUfs = new GroupBox { Text = "Por UF", Dock = DockStyle.Fill };
        grupoUfs.Controls.Add(_ufs);

        Controls.Add(grupoUfs);
        Controls.Add(grupoRegioes);
        Controls.Add(barra);
    }

    public override Task AtualizarAsync() => ExecutarAsync(CarregarAsync);

    private async Task CarregarAsync()
    {
        var (total, regioes, ufs) = await NoEscopoAsync(async sp =>
        {
            var servico = sp.GetRequiredService<IRiscoService>();
            return (await servico.ContarAsync(), await servico.ResumoPorRegiaoAsync(), await servico.ResumoPorUfAsync(null));
        });
        _status.Text = total == 0 ? "Nenhuma previsão importada ainda." : $"{total:N0} escolas com previsão do modelo.";
        _regioes.DataSource = regioes.Select(Linha).ToList();
        _ufs.DataSource = ufs.Select(Linha).ToList();
    }

    private static object Linha(CitizenTech.Application.Dtos.ResumoRiscoResponse r) => new
    {
        Local = r.Chave, Escolas = r.TotalEscolas,
        PercSemRampa = $"{r.PercentualSemRampaDeclarado:0.0}%", RiscoMedioML = $"{r.ProbabilidadeMedia * 100:0}%", RiscoAlto = r.EscolasRiscoAlto
    };

    private async Task ImportarAsync()
    {
        var alvo = _configuracao["Ml:CsvPath"] ?? "data/ml/previsoes_escolas_oof.csv";
        var caminho = LocalizadorArquivos.Localizar(alvo);
        if (caminho is null) { Aviso($"Arquivo não encontrado: {alvo}\nCopie o CSV gerado pelo notebook de ML para essa pasta."); return; }

        var uf = _uf.SelectedIndex <= 0 ? null : (string?)_uf.SelectedItem;
        var confirmar = MessageBox.Show("A importação APAGA e recarrega a tabela de previsões (pode levar cerca de 1 minuto para o Brasil inteiro). Continuar?",
            "Importar previsões", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirmar != DialogResult.Yes) return;

        var resultado = await NoEscopoAsync(sp => sp.GetRequiredService<IRiscoService>().ImportarAsync(caminho, uf));
        await CarregarAsync();
        Info($"Importação concluída: {resultado.Importadas:N0} escolas importadas, {resultado.Ignoradas:N0} ignoradas.");
    }
}
