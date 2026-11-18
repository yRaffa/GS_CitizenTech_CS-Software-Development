using CitizenTech.Application.Dtos;
using CitizenTech.Application.Equipe;
using CitizenTech.Application.Servicos;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Exceptions;
using CitizenTech.Tests.Fakes;
using Xunit;

namespace CitizenTech.Tests.Aplicacao;

/// <summary>Testes de imagem (R5/R7) e da operação transacional (R6) sobre repositórios em memória.</summary>
public class AmbienteImagemReserva
{
    public const string Executor = "AnaExemplo_RM000001";

    public BancoEmMemoria Banco { get; } = new();
    public UnitOfWorkEmMemoria Uow { get; } = new();
    public ImagemService Imagens { get; }
    public ReservaService Reservas { get; }
    public Estabelecimento Estabelecimento { get; }
    public Parceiro Parceiro { get; }
    public Diagnostico Diagnostico { get; }

    public AmbienteImagemReserva(bool permitirFalhaSimulada = true)
    {
        var equipe = new EquipeInfo("CitizenTech Teste", "3ESPH", "RM000001",
            new[] { new IntegranteInfo("Ana Exemplo", "RM000001", "3ESPH", Executor, true) });

        var est = new EstabelecimentoRepositorioEmMemoria(Banco);
        var par = new ParceiroRepositorioEmMemoria(Banco);
        Estabelecimento = new Estabelecimento("Escola Exemplo", TipoEstabelecimento.Escola, new Endereco("Rua A", "1", "Centro", "São Paulo", "SP"));
        Parceiro = new Parceiro("Rampa Certa (exemplo)", "Obras", Especialidade.ObrasAcessibilidade, new[] { "SP" }, "a@b.com", null, 20);
        est.AdicionarAsync(Estabelecimento).GetAwaiter().GetResult();
        par.AdicionarAsync(Parceiro).GetAwaiter().GetResult();

        // Fluxo da CitizenTech: estabelecimento -> avaliação -> diagnóstico (com barreira) -> encaminhamento com reserva.
        var avaliacao = new Avaliacao(Estabelecimento.Id);
        new AvaliacaoRepositorioEmMemoria(Banco).AdicionarAsync(avaliacao).GetAwaiter().GetResult();
        Diagnostico = new Diagnostico(avaliacao.Id);
        Diagnostico.AdicionarBarreira(Dados.NovaBarreira(Gravidade.Alta));
        Diagnostico.Concluir();
        var diag = new DiagnosticoRepositorioEmMemoria(Banco);
        diag.AdicionarAsync(Diagnostico).GetAwaiter().GetResult();

        Imagens = new ImagemService(new ImagemRepositorioEmMemoria(Banco), est, Uow, equipe);
        Reservas = new ReservaService(par, diag, new EncaminhamentoRepositorioEmMemoria(Banco), new ReservaRepositorioEmMemoria(Banco), Uow, equipe, permitirFalhaSimulada);
    }

    public static byte[] Png(int tamanho = 64)
    {
        var b = new byte[tamanho];
        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }.CopyTo(b, 0);
        return b;
    }

    public static byte[] Jpeg(int tamanho = 64)
    {
        var b = new byte[tamanho];
        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }.CopyTo(b, 0);
        return b;
    }

    public ImagemUpload Upload(byte[]? dados = null, string titulo = "Rampa sem corrimão", string mime = "image/png",
                               string nome = "rampa.png", int? estabelecimentoId = null, string? executor = null) =>
        new(titulo, estabelecimentoId ?? Estabelecimento.Id, executor ?? Executor, nome, mime, dados ?? Png());
}

public class ImagemServiceTests
{
    [Fact]
    public async Task Envia_png_valido_e_guarda_bytes_com_metadados_da_equipe()
    {
        var a = new AmbienteImagemReserva();
        var bytes = AmbienteImagemReserva.Png(200);

        var resposta = await a.Imagens.EnviarAsync(a.Upload(bytes));

        Assert.True(resposta.Id > 0);
        Assert.Equal("image/png", resposta.TipoMime);
        Assert.Equal(200, resposta.TamanhoBytes);
        Assert.Equal("RM000001", resposta.EquipeRmLider);
        Assert.Equal(AmbienteImagemReserva.Executor, resposta.Executor);
        Assert.Single(a.Banco.Imagens);
        Assert.Equal(bytes, a.Banco.Imagens[0].Dados);
    }

    [Fact]
    public async Task Recupera_conteudo_por_id_com_o_mime_correto()
    {
        var a = new AmbienteImagemReserva();
        var criada = await a.Imagens.EnviarAsync(a.Upload(AmbienteImagemReserva.Jpeg(80), mime: "image/jpeg", nome: "foto.jpg"));

        var conteudo = await a.Imagens.ObterConteudoAsync(criada.Id);

        Assert.Equal("image/jpeg", conteudo.TipoMime);
        Assert.Equal(80, conteudo.Dados.Length);
    }

    [Fact]
    public async Task Rejeita_arquivo_vazio_sem_gravar()
    {
        var a = new AmbienteImagemReserva();
        await Assert.ThrowsAsync<DomainException>(() => a.Imagens.EnviarAsync(a.Upload(Array.Empty<byte>())));
        Assert.Empty(a.Banco.Imagens);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Rejeita_titulo_vazio_sem_gravar(string titulo)
    {
        var a = new AmbienteImagemReserva();
        await Assert.ThrowsAsync<DomainException>(() => a.Imagens.EnviarAsync(a.Upload(titulo: titulo)));
        Assert.Empty(a.Banco.Imagens);
    }

    [Fact]
    public async Task Rejeita_titulo_com_mais_de_100_caracteres()
    {
        var a = new AmbienteImagemReserva();
        await Assert.ThrowsAsync<DomainException>(() => a.Imagens.EnviarAsync(a.Upload(titulo: new string('x', 101))));
        Assert.Empty(a.Banco.Imagens);
    }

    [Fact]
    public async Task Rejeita_png_disfarçado_com_bytes_de_texto()
    {
        var a = new AmbienteImagemReserva();
        var texto = System.Text.Encoding.UTF8.GetBytes("isto é apenas texto, não uma imagem");
        await Assert.ThrowsAsync<DomainException>(() => a.Imagens.EnviarAsync(a.Upload(texto, nome: "falso.png")));
        Assert.Empty(a.Banco.Imagens);
    }

    [Fact]
    public async Task Rejeita_mime_informado_diferente_do_conteudo()
    {
        var a = new AmbienteImagemReserva();
        await Assert.ThrowsAsync<DomainException>(() => a.Imagens.EnviarAsync(a.Upload(AmbienteImagemReserva.Png(), mime: "image/jpeg")));
        Assert.Empty(a.Banco.Imagens);
    }

    [Fact]
    public async Task Rejeita_arquivo_acima_de_2_MiB_com_excecao_de_413()
    {
        var a = new AmbienteImagemReserva();
        var grande = AmbienteImagemReserva.Png(ImagemService.TamanhoMaximoBytes + 1);
        await Assert.ThrowsAsync<ArquivoGrandeDemaisException>(() => a.Imagens.EnviarAsync(a.Upload(grande)));
        Assert.Empty(a.Banco.Imagens);
    }

    [Fact]
    public async Task Aceita_exatamente_2_MiB()
    {
        var a = new AmbienteImagemReserva();
        var limite = AmbienteImagemReserva.Png(ImagemService.TamanhoMaximoBytes);
        var criada = await a.Imagens.EnviarAsync(a.Upload(limite));
        Assert.Equal(ImagemService.TamanhoMaximoBytes, criada.TamanhoBytes);
    }

    [Fact]
    public async Task Rejeita_vinculo_inexistente_com_404()
    {
        var a = new AmbienteImagemReserva();
        await Assert.ThrowsAsync<NaoEncontradoException>(() => a.Imagens.EnviarAsync(a.Upload(estabelecimentoId: 9999)));
        Assert.Empty(a.Banco.Imagens);
    }

    [Fact]
    public async Task Rejeita_executor_fora_da_equipe()
    {
        var a = new AmbienteImagemReserva();
        await Assert.ThrowsAsync<DomainException>(() => a.Imagens.EnviarAsync(a.Upload(executor: "Estranho_RM999999")));
        Assert.Empty(a.Banco.Imagens);
    }

    [Fact]
    public async Task Consulta_de_id_inexistente_lanca_nao_encontrado()
    {
        var a = new AmbienteImagemReserva();
        await Assert.ThrowsAsync<NaoEncontradoException>(() => a.Imagens.ObterMetadadosAsync(12345));
        await Assert.ThrowsAsync<NaoEncontradoException>(() => a.Imagens.ObterConteudoAsync(12345));
    }

    [Fact]
    public void Detecta_mime_pela_assinatura_e_nao_pela_extensao()
    {
        Assert.Equal("image/png", ImagemService.DetectarMime(AmbienteImagemReserva.Png()));
        Assert.Equal("image/jpeg", ImagemService.DetectarMime(AmbienteImagemReserva.Jpeg()));
        Assert.Null(ImagemService.DetectarMime(new byte[] { 1, 2, 3, 4 }));
    }
}

public class ReservaServiceTests
{
    private static ReservaRequest Pedido(AmbienteImagemReserva a, int quantidade = 3, bool falha = false, string? executor = null) => new()
    {
        DiagnosticoId = a.Diagnostico.Id, ParceiroId = a.Parceiro.Id, Quantidade = quantidade,
        Executor = executor ?? AmbienteImagemReserva.Executor, SimularFalhaAposEscritas = falha
    };

    [Fact]
    public async Task Commit_baixa_o_saldo_cria_encaminhamento_e_reserva_juntos()
    {
        var a = new AmbienteImagemReserva();
        Assert.Equal(20, a.Parceiro.VagasAtendimento);

        var resposta = await a.Reservas.ReservarAsync(Pedido(a, 3));

        Assert.Equal(17, a.Parceiro.VagasAtendimento);
        Assert.Single(a.Banco.Reservas);
        Assert.Single(a.Banco.Encaminhamentos);
        Assert.Equal(a.Banco.Encaminhamentos[0].Id, resposta.EncaminhamentoId);
        Assert.Equal(a.Diagnostico.Id, resposta.DiagnosticoId);
        Assert.Equal(a.Estabelecimento.Id, resposta.EstabelecimentoId);
        Assert.Equal(17, resposta.VagasRestantesParceiro);
        Assert.True(a.Uow.UltimaTransacao!.Confirmada);
        Assert.False(a.Uow.UltimaTransacao.Revertida);
    }

    [Fact]
    public async Task Falha_apos_as_escritas_dispara_rollback_e_nao_confirma()
    {
        var a = new AmbienteImagemReserva();

        var ex = await Assert.ThrowsAsync<OperacaoRevertidaException>(() => a.Reservas.ReservarAsync(Pedido(a, 2, falha: true)));

        Assert.Contains("ROLLBACK", ex.Message);
        Assert.True(a.Uow.UltimaTransacao!.Revertida);
        Assert.False(a.Uow.UltimaTransacao.Confirmada);
    }

    [Fact]
    public async Task Falha_simulada_e_ignorada_quando_desabilitada_por_configuracao()
    {
        var a = new AmbienteImagemReserva(permitirFalhaSimulada: false);
        await a.Reservas.ReservarAsync(Pedido(a, 1, falha: true));
        Assert.True(a.Uow.UltimaTransacao!.Confirmada);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Rejeita_quantidade_zero_ou_negativa_sem_abrir_transacao(int quantidade)
    {
        var a = new AmbienteImagemReserva();
        await Assert.ThrowsAsync<DomainException>(() => a.Reservas.ReservarAsync(Pedido(a, quantidade)));
        Assert.Equal(20, a.Parceiro.VagasAtendimento);
        Assert.Empty(a.Banco.Reservas);
        Assert.Empty(a.Banco.Encaminhamentos);
        Assert.Null(a.Uow.UltimaTransacao);
    }

    [Fact]
    public async Task Saldo_insuficiente_lanca_conflito_e_nao_altera_nada()
    {
        var a = new AmbienteImagemReserva();
        await Assert.ThrowsAsync<ConflitoException>(() => a.Reservas.ReservarAsync(Pedido(a, 21)));
        Assert.Equal(20, a.Parceiro.VagasAtendimento);
        Assert.Empty(a.Banco.Reservas);
    }

    [Fact]
    public async Task Rejeita_executor_nao_cadastrado()
    {
        var a = new AmbienteImagemReserva();
        await Assert.ThrowsAsync<DomainException>(() => a.Reservas.ReservarAsync(Pedido(a, 1, executor: "Estranho_RM999999")));
        Assert.Equal(20, a.Parceiro.VagasAtendimento);
        Assert.Empty(a.Banco.Reservas);
    }

    [Fact]
    public async Task Parceiro_ou_diagnostico_inexistente_lanca_nao_encontrado()
    {
        var a = new AmbienteImagemReserva();
        await Assert.ThrowsAsync<NaoEncontradoException>(() =>
            a.Reservas.ReservarAsync(new ReservaRequest { DiagnosticoId = a.Diagnostico.Id, ParceiroId = 999, Quantidade = 1, Executor = AmbienteImagemReserva.Executor }));
        await Assert.ThrowsAsync<NaoEncontradoException>(() =>
            a.Reservas.ReservarAsync(new ReservaRequest { DiagnosticoId = 999, ParceiroId = a.Parceiro.Id, Quantidade = 1, Executor = AmbienteImagemReserva.Executor }));
    }

    [Fact]
    public async Task Segundo_encaminhamento_do_mesmo_diagnostico_ao_mesmo_parceiro_lanca_conflito()
    {
        var a = new AmbienteImagemReserva();
        await a.Reservas.ReservarAsync(Pedido(a, 1));
        await Assert.ThrowsAsync<ConflitoException>(() => a.Reservas.ReservarAsync(Pedido(a, 1)));
        Assert.Equal(19, a.Parceiro.VagasAtendimento);
        Assert.Single(a.Banco.Reservas);
    }
}
