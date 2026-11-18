using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CitizenTech.Desktop;

/// <summary>
/// Aplicativo Windows Forms da CitizenTech (Acessibilidade Urbana). É um CLIENTE da API: usa HttpClient e
/// NÃO acessa o banco (somente a API fala com o SQL Server). Segue o fluxo do projeto:
/// equipe → estabelecimento → avaliação/diagnóstico → foto da barreira → encaminhamento com reserva → risco regional (ML).
/// Falhas de comunicação (API parada) viram mensagem clara no log, sem derrubar o aplicativo (T10).
/// </summary>
public class MainForm : Form
{
    private static readonly JsonSerializerOptions JsonOpcoes = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };
    private static readonly string[] Itens = { "Rampa", "Corrimao", "BanheiroAdaptado", "PisoTatil", "SinalizacaoVisual", "Elevador", "VagaPreferencial", "AtendimentoLibras" };
    private static readonly string[] RotulosItens = { "Rampa de acesso", "Corrimão", "Banheiro adaptado", "Piso tátil", "Sinalização visual", "Elevador", "Vaga preferencial", "Atendimento em Libras" };

    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(20) };
    private readonly TextBox _url = new() { Width = 260, AccessibleName = "URL da API" };
    private readonly Label _lblEquipe = new() { AutoSize = true, Text = "Equipe: clique em \"Carregar equipe\"" };
    private readonly ComboBox _executor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, AccessibleName = "Executor NomeAluno_RM" };
    private readonly TextBox _log = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Font = new Font("Consolas", 10) };

    // Estabelecimentos
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    private readonly TextBox _nome = new() { Width = 220 }, _logradouro = new() { Width = 180 }, _cidade = new() { Width = 120 }, _uf = new() { Width = 40, MaxLength = 2 }, _inep = new() { Width = 100 };
    private readonly ComboBox _tipo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };

    // Avaliação / diagnóstico
    private readonly NumericUpDown _estabAvaliar = Numero(1);
    private readonly ComboBox[] _respostas = new ComboBox[8];
    private readonly NumericUpDown _rampa = Numero(8), _porta = Numero(90), _corredor = Numero(120);
    private readonly CheckBox _informarMedicoes = new() { AutoSize = true, Text = "Informar medições (rampa %, porta cm, corredor cm)", Checked = true };
    private readonly NumericUpDown _diagnosticoId = Numero(1);
    private readonly NumericUpDown _diagnosticoIdEnc = Numero(1); // mesma informação na aba 5 (um controle não pode ter dois pais)

    // Foto
    private readonly NumericUpDown _estabFoto = Numero(1), _idRecuperar = Numero(1);
    private readonly TextBox _titulo = new() { Width = 260, AccessibleName = "Título da foto (1 a 100 caracteres)" };
    private readonly Label _arquivo = new() { AutoSize = true, Text = "(nenhum arquivo)" };
    private readonly PictureBox _previaLocal = Imagem("Prévia do arquivo escolhido (local)");
    private readonly PictureBox _imagemBanco = Imagem("Imagem recuperada do banco via API");
    private readonly Label _lblMeta = new() { AutoSize = true, Text = "Metadados: -" };
    private string? _caminhoArquivo;

    // Encaminhamento com reserva
    private readonly NumericUpDown _parceiro = Numero(1), _quantidade = Numero(3);
    private readonly CheckBox _falha = new() { AutoSize = true, Text = "Simular falha depois das escritas (ROLLBACK)" };

    // Risco ML
    private readonly TextBox _inepConsulta = new() { Width = 120 };

    public MainForm()
    {
        Text = "CitizenTech · Acessibilidade Urbana (cliente da API)";
        Width = 1180; Height = 860; StartPosition = FormStartPosition.CenterScreen; Font = new Font("Segoe UI", 10);
        _url.Text = LerUrlBase();
        _tipo.Items.AddRange(new object[] { "Escola", "Saude", "Comercio", "Servico", "Cultura", "Outro" });
        _tipo.SelectedIndex = 0;

        var abas = new TabControl { Dock = DockStyle.Fill };
        abas.TabPages.Add(Aba("1 · API e equipe", MontarEquipe()));
        abas.TabPages.Add(Aba("2 · Estabelecimentos", MontarEstabelecimentos()));
        abas.TabPages.Add(Aba("3 · Avaliar e diagnosticar", MontarAvaliacao()));
        abas.TabPages.Add(Aba("4 · Foto da barreira", MontarFoto()));
        abas.TabPages.Add(Aba("5 · Encaminhar com reserva", MontarEncaminhamento()));
        abas.TabPages.Add(Aba("6 · Risco regional (ML)", MontarRisco()));

        var divisao = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 470 };
        divisao.Panel1.Controls.Add(abas);
        divisao.Panel2.Controls.Add(_log);
        Controls.Add(divisao);
        _diagnosticoId.ValueChanged += (_, _) => _diagnosticoIdEnc.Value = _diagnosticoId.Value;
    }

    // ------------------------------------------------------------- abas
    private Control MontarEquipe()
    {
        var p = Coluna();
        p.Controls.Add(Linha(Rotulo("URL da API:"), _url, Botao("Carregar equipe", CarregarEquipeAsync), Rotulo("Executor:"), _executor));
        p.Controls.Add(_lblEquipe);
        p.Controls.Add(Rotulo("A equipe vem de GET /api/equipe (arquivo equipe.json). O executor identifica quem fez cada envio/encaminhamento. Não é autenticação."));
        return p;
    }

    private Control MontarEstabelecimentos()
    {
        var p = new Panel { Dock = DockStyle.Fill };
        var topo = Linha(Botao("Listar estabelecimentos", ListarEstabelecimentosAsync), Rotulo("Nome:"), _nome, Rotulo("Tipo:"), _tipo,
            Rotulo("Rua:"), _logradouro, Rotulo("Cidade:"), _cidade, Rotulo("UF:"), _uf, Rotulo("INEP (escola):"), _inep, Botao("Cadastrar", CadastrarEstabelecimentoAsync));
        topo.Dock = DockStyle.Top;
        p.Controls.Add(_grid);
        p.Controls.Add(topo);
        return p;
    }

    private Control MontarAvaliacao()
    {
        var p = Coluna();
        p.Controls.Add(Linha(Rotulo("Estabelecimento ID:"), _estabAvaliar));
        var quadro = new FlowLayoutPanel { AutoSize = true, WrapContents = true, Width = 1100 };
        for (var i = 0; i < Itens.Length; i++)
        {
            var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 80, AccessibleName = RotulosItens[i] };
            cb.Items.AddRange(new object[] { "Sim", "Nao", "NaoSei" });
            cb.SelectedIndex = 0;
            _respostas[i] = cb;
            quadro.Controls.Add(Rotulo(RotulosItens[i] + ":")); quadro.Controls.Add(cb);
        }
        p.Controls.Add(quadro);
        p.Controls.Add(Linha(_informarMedicoes, Rotulo("Rampa %:"), _rampa, Rotulo("Porta cm:"), _porta, Rotulo("Corredor cm:"), _corredor));
        p.Controls.Add(Linha(Botao("Avaliar e gerar diagnóstico", AvaliarEDiagnosticarAsync), Rotulo("Diagnóstico ID:"), _diagnosticoId,
            Botao("Consultar diagnóstico", () => Get($"/api/diagnosticos/{(int)_diagnosticoId.Value}")),
            Botao("Parceiros sugeridos", () => Get($"/api/diagnosticos/{(int)_diagnosticoId.Value}/parceiros-sugeridos"))));
        p.Controls.Add(Rotulo("Triagem preliminar: não substitui a avaliação técnica presencial."));
        return p;
    }

    private Control MontarFoto()
    {
        var p = Coluna();
        p.Controls.Add(Linha(Rotulo("Estabelecimento ID:"), _estabFoto, Rotulo("Título:"), _titulo, Botao("Escolher arquivo...", EscolherArquivo), Botao("Enviar foto", EnviarAsync), _arquivo));
        p.Controls.Add(Linha(Rotulo("ID da foto:"), _idRecuperar, Botao("Recuperar do banco", RecuperarAsync), _lblMeta));
        p.Controls.Add(Linha(_previaLocal, _imagemBanco));
        return p;
    }

    private Control MontarEncaminhamento()
    {
        var p = Coluna();
        p.Controls.Add(Linha(Rotulo("Diagnóstico ID:"), _diagnosticoIdEnc, Rotulo("Parceiro ID:"), _parceiro, Rotulo("Vagas:"), _quantidade, _falha, Botao("Encaminhar com reserva", ReservarAsync)));
        p.Controls.Add(Linha(Botao("Listar parceiros (saldo)", () => Get("/api/parceiros")), Botao("Listar reservas", () => Get("/api/reservas")), Botao("Listar encaminhamentos", () => Get("/api/encaminhamentos"))));
        p.Controls.Add(Rotulo("Uma só transação: baixa o saldo do parceiro, cria o encaminhamento e registra a reserva (COMMIT) ou desfaz tudo (ROLLBACK)."));
        return p;
    }

    private Control MontarRisco()
    {
        var p = Coluna();
        p.Controls.Add(Linha(Botao("Resumo por região", () => Get("/api/risco/regioes")), Botao("Resumo por UF", () => Get("/api/risco/ufs")),
            Rotulo("Código INEP:"), _inepConsulta, Botao("Consultar escola", () => Get($"/api/risco/escolas/{_inepConsulta.Text.Trim()}"))));
        p.Controls.Add(Rotulo("Previsões do modelo XGBoost (Censo Escolar 2025) importadas na API: probabilidade de a escola não ter rampa."));
        return p;
    }

    // ------------------------------------------------------------- ações
    private async Task CarregarEquipeAsync()
    {
        await ExecutarAsync("GET /api/equipe", async () =>
        {
            var (resposta, corpo) = await ChamarAsync(() => _http.GetAsync(Url("/api/equipe")));
            if (!resposta.IsSuccessStatusCode) return;
            using var doc = JsonDocument.Parse(corpo);
            var r = doc.RootElement;
            _lblEquipe.Text = $"CitizenTech: {r.GetProperty("citizenTech").GetString()} · Turma: {r.GetProperty("turma").GetString()} · RM do líder: {r.GetProperty("rmLider").GetString()}";
            _executor.Items.Clear();
            foreach (var i in r.GetProperty("integrantes").EnumerateArray()) _executor.Items.Add(i.GetProperty("nomeAlunoRm").GetString()!);
            if (_executor.Items.Count > 0) _executor.SelectedIndex = 0;
        });
    }

    private async Task ListarEstabelecimentosAsync()
    {
        await ExecutarAsync("GET /api/estabelecimentos", async () =>
        {
            var (resposta, corpo) = await ChamarAsync(() => _http.GetAsync(Url("/api/estabelecimentos?tamanhoPagina=100")));
            if (!resposta.IsSuccessStatusCode) return;
            using var doc = JsonDocument.Parse(corpo);
            var tabela = new DataTable();
            foreach (var c in new[] { "Id", "Nome", "Tipo", "Cidade", "Uf", "Regiao", "CodigoInep" }) tabela.Columns.Add(c);
            foreach (var e in doc.RootElement.GetProperty("itens").EnumerateArray())
                tabela.Rows.Add(Txt(e, "id"), Txt(e, "nome"), Txt(e, "tipo"), Txt(e, "cidade"), Txt(e, "uf"), Txt(e, "regiao"), Txt(e, "codigoInep"));
            _grid.DataSource = tabela;
        });
    }

    private async Task CadastrarEstabelecimentoAsync()
    {
        await ExecutarAsync("POST /api/estabelecimentos", async () =>
        {
            var corpoEnvio = new Dictionary<string, object?>
            {
                ["nome"] = _nome.Text, ["tipo"] = _tipo.SelectedItem?.ToString(), ["logradouro"] = _logradouro.Text,
                ["cidade"] = _cidade.Text, ["uf"] = _uf.Text.ToUpperInvariant(), ["bairro"] = "",
                ["codigoInep"] = long.TryParse(_inep.Text, out var inep) ? inep : null
            };
            await ChamarAsync(() => _http.PostAsync(Url("/api/estabelecimentos"), Json(corpoEnvio)));
        });
        await ListarEstabelecimentosAsync();
    }

    private async Task AvaliarEDiagnosticarAsync()
    {
        await ExecutarAsync("Avaliar e diagnosticar", async () =>
        {
            var respostas = Itens.Select((item, i) => new { item, valor = _respostas[i].SelectedItem!.ToString() }).ToList();
            var medicoes = new List<object>();
            if (_informarMedicoes.Checked)
            {
                medicoes.Add(new { tipo = "InclinacaoRampaPercentual", valor = (double)_rampa.Value });
                medicoes.Add(new { tipo = "LarguraPortaCm", valor = (double)_porta.Value });
                medicoes.Add(new { tipo = "LarguraCorredorCm", valor = (double)_corredor.Value });
            }
            var (r1, c1) = await ChamarAsync(() => _http.PostAsync(Url($"/api/estabelecimentos/{(int)_estabAvaliar.Value}/avaliacoes"),
                Json(new { respostas, medicoes })));
            if (!r1.IsSuccessStatusCode) return;
            var avaliacaoId = JsonDocument.Parse(c1).RootElement.GetProperty("id").GetInt32();

            var (r2, c2) = await ChamarAsync(() => _http.PostAsync(Url($"/api/avaliacoes/{avaliacaoId}/diagnosticos"), null));
            if (!r2.IsSuccessStatusCode) return;
            var d = JsonDocument.Parse(c2).RootElement;
            _diagnosticoId.Value = _diagnosticoIdEnc.Value = d.GetProperty("id").GetInt32();
            Escrever($"Diagnóstico {_diagnosticoId.Value}: risco {d.GetProperty("nivelRisco").GetString()} · {d.GetProperty("resumo").GetString()}");
            foreach (var b in d.GetProperty("barreiras").EnumerateArray())
                Escrever($"  • [{b.GetProperty("gravidade").GetString()}] {b.GetProperty("descricao").GetString()}");
        });
    }

    private void EscolherArquivo()
    {
        using var dialogo = new OpenFileDialog { Filter = "Imagens e outros|*.png;*.jpg;*.jpeg;*.gif;*.txt|Todos|*.*" };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        _caminhoArquivo = dialogo.FileName;
        _arquivo.Text = Path.GetFileName(_caminhoArquivo);
        try { _previaLocal.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(_caminhoArquivo))); }
        catch { _previaLocal.Image = null; Escrever("Prévia local indisponível (não é imagem legível). O envio continua possível para testar a validação."); }
    }

    private async Task EnviarAsync()
    {
        if (_caminhoArquivo is null) { Escrever("Escolha um arquivo antes de enviar (para o teste T03, use um arquivo vazio)."); return; }
        await ExecutarAsync("POST /api/imagens", async () =>
        {
            var bytes = await File.ReadAllBytesAsync(_caminhoArquivo);
            using var form = new MultipartFormDataContent();
            var parte = new ByteArrayContent(bytes);
            parte.Headers.ContentType = new MediaTypeHeaderValue(MimePorExtensao(_caminhoArquivo));
            form.Add(parte, "arquivo", Path.GetFileName(_caminhoArquivo));
            form.Add(new StringContent(_titulo.Text), "titulo");
            form.Add(new StringContent(((int)_estabFoto.Value).ToString()), "estabelecimentoId");
            form.Add(new StringContent(_executor.SelectedItem?.ToString() ?? string.Empty), "executor");
            var (resposta, corpo) = await ChamarAsync(() => _http.PostAsync(Url("/api/imagens"), form));
            if (resposta.IsSuccessStatusCode)
            {
                _idRecuperar.Value = JsonDocument.Parse(corpo).RootElement.GetProperty("id").GetInt32();
                Escrever($"ID gravado: {_idRecuperar.Value} (anote para o teste T02).");
            }
        });
    }

    private async Task RecuperarAsync()
    {
        // Limpa o que veio do arquivo local: a imagem exibida abaixo vem SOMENTE do banco, via API.
        _previaLocal.Image = null; _caminhoArquivo = null; _arquivo.Text = "(nenhum arquivo)"; _imagemBanco.Image = null;
        await ExecutarAsync("GET /api/imagens/{id}", async () =>
        {
            var id = (int)_idRecuperar.Value;
            var (meta, corpoMeta) = await ChamarAsync(() => _http.GetAsync(Url($"/api/imagens/{id}")));
            if (!meta.IsSuccessStatusCode) return;
            var conteudo = await _http.GetAsync(Url($"/api/imagens/{id}/conteudo"));
            if (!conteudo.IsSuccessStatusCode) { Registrar(conteudo, await conteudo.Content.ReadAsStringAsync()); return; }
            var bytes = await conteudo.Content.ReadAsByteArrayAsync();
            _imagemBanco.Image = Image.FromStream(new MemoryStream(bytes));
            var r = JsonDocument.Parse(corpoMeta).RootElement;
            _lblMeta.Text = $"Do banco · ID {id} · {r.GetProperty("titulo").GetString()} · {conteudo.Content.Headers.ContentType?.MediaType} · {bytes.Length} bytes · executor {r.GetProperty("executor").GetString()}";
            Escrever($"GET /api/imagens/{id}/conteudo -> {(int)conteudo.StatusCode}, {bytes.Length} bytes recebidos.");
        });
    }

    private async Task ReservarAsync()
    {
        await ExecutarAsync("POST /api/reservas", async () =>
        {
            var corpo = new
            {
                diagnosticoId = (int)_diagnosticoIdEnc.Value, parceiroId = (int)_parceiro.Value, quantidade = (int)_quantidade.Value,
                executor = _executor.SelectedItem?.ToString() ?? string.Empty, simularFalhaAposEscritas = _falha.Checked
            };
            await ChamarAsync(() => _http.PostAsync(Url("/api/reservas"), Json(corpo)));
        });
    }

    private Task Get(string caminho) => ExecutarAsync($"GET {caminho}", () => ChamarAsync(() => _http.GetAsync(Url(caminho))));

    // ------------------------------------------------------------- utilidades
    private async Task<(HttpResponseMessage, string)> ChamarAsync(Func<Task<HttpResponseMessage>> chamada)
    {
        var resposta = await chamada();
        var corpo = await resposta.Content.ReadAsStringAsync();
        Registrar(resposta, corpo);
        return (resposta, corpo);
    }

    /// <summary>Trata a indisponibilidade da API (T10): mensagem clara, nada é simulado e o app continua aberto.</summary>
    private async Task ExecutarAsync(string rotulo, Func<Task> acao)
    {
        try { await acao(); }
        catch (HttpRequestException ex) { Escrever($"[{DateTime.Now:HH:mm:ss}] {rotulo} -> FALHA DE COMUNICAÇÃO: a API está indisponível ({ex.Message}). Nada foi enviado; inicie a API e tente de novo."); }
        catch (TaskCanceledException) { Escrever($"[{DateTime.Now:HH:mm:ss}] {rotulo} -> TEMPO ESGOTADO: a API não respondeu. Nada foi confirmado."); }
        catch (Exception ex) { Escrever($"[{DateTime.Now:HH:mm:ss}] {rotulo} -> erro inesperado no cliente: {ex.Message}"); }
    }

    private void Registrar(HttpResponseMessage resposta, string corpo)
    {
        var formatado = corpo;
        try { if (!string.IsNullOrWhiteSpace(corpo)) formatado = JsonSerializer.Serialize(JsonDocument.Parse(corpo).RootElement, JsonOpcoes); } catch { /* corpo não é JSON */ }
        Escrever($"[{DateTime.Now:HH:mm:ss}] {resposta.RequestMessage?.Method} {resposta.RequestMessage?.RequestUri?.AbsolutePath} -> HTTP {(int)resposta.StatusCode} {resposta.ReasonPhrase}\r\n{formatado}\r\n");
    }

    private void Escrever(string texto) => _log.AppendText(texto + "\r\n");
    private string Url(string caminho) => _url.Text.TrimEnd('/') + caminho;
    private static StringContent Json(object o) => new(JsonSerializer.Serialize(o), Encoding.UTF8, "application/json");
    private static string Txt(JsonElement e, string prop) => e.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null ? v.ToString() : "";

    private static string MimePorExtensao(string caminho) => Path.GetExtension(caminho).ToLowerInvariant() switch
    {
        ".png" => "image/png", ".jpg" or ".jpeg" => "image/jpeg", ".gif" => "image/gif", ".txt" => "text/plain", _ => "application/octet-stream"
    };

    private static string LerUrlBase()
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")));
            return doc.RootElement.GetProperty("ApiBaseUrl").GetString() ?? "http://localhost:5080";
        }
        catch { return "http://localhost:5080"; }
    }

    private static NumericUpDown Numero(int valor) => new() { Minimum = 0, Maximum = 1000000, Value = valor, Width = 80 };
    private static PictureBox Imagem(string legenda) => new() { Width = 300, Height = 220, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, AccessibleName = legenda };
    private static Label Rotulo(string texto) => new() { Text = texto, AutoSize = true, Padding = new Padding(0, 6, 4, 0) };
    private static TabPage Aba(string titulo, Control conteudo) { var t = new TabPage(titulo) { Padding = new Padding(8) }; conteudo.Dock = DockStyle.Fill; t.Controls.Add(conteudo); return t; }
    private static FlowLayoutPanel Coluna() => new() { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
    private static FlowLayoutPanel Linha(params Control[] controles)
    {
        var l = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = true, Width = 1100 };
        l.Controls.AddRange(controles);
        return l;
    }

    private static Button Botao(string texto, Func<Task> acao)
    {
        var b = new Button { Text = texto, AutoSize = true };
        b.Click += async (_, _) => await acao();
        return b;
    }

    private static Button Botao(string texto, Action acao)
    {
        var b = new Button { Text = texto, AutoSize = true };
        b.Click += (_, _) => acao();
        return b;
    }
}
