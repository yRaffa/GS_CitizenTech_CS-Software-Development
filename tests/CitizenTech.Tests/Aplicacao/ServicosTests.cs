using CitizenTech.Application.Dtos;
using CitizenTech.Application.Regras;
using CitizenTech.Application.Servicos;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Exceptions;
using CitizenTech.Tests.Fakes;
using Xunit;

namespace CitizenTech.Tests.Aplicacao;

/// <summary>Monta os serviços reais sobre repositórios em memória.</summary>
public class Ambiente
{
    public BancoEmMemoria Banco { get; } = new();
    public EstabelecimentoService Estabelecimentos { get; }
    public AvaliacaoService Avaliacoes { get; }
    public DiagnosticoService Diagnosticos { get; }
    public ParceiroService Parceiros { get; }
    public EncaminhamentoService Encaminhamentos { get; }
    public RiscoService Risco { get; }

    public Ambiente()
    {
        var uow = new UnitOfWorkEmMemoria();
        var est = new EstabelecimentoRepositorioEmMemoria(Banco);
        var ava = new AvaliacaoRepositorioEmMemoria(Banco);
        var dia = new DiagnosticoRepositorioEmMemoria(Banco);
        var par = new ParceiroRepositorioEmMemoria(Banco);
        var enc = new EncaminhamentoRepositorioEmMemoria(Banco);
        var pre = new PrevisaoRiscoRepositorioEmMemoria(Banco);

        Estabelecimentos = new EstabelecimentoService(est, uow);
        Avaliacoes = new AvaliacaoService(est, ava, uow);
        Diagnosticos = new DiagnosticoService(ava, dia, pre, RegrasPadrao.ObterTodas(), uow);
        Parceiros = new ParceiroService(par, dia, uow);
        Encaminhamentos = new EncaminhamentoService(enc, dia, par, uow);
        Risco = new RiscoService(pre);
    }

    public static EstabelecimentoRequest RequestEscola(string uf = "SP", long? inep = null) => new()
    {
        Nome = "Escola Exemplo", Tipo = TipoEstabelecimento.Escola, Logradouro = "Rua A", Numero = "10",
        Bairro = "Centro", Cidade = "Cidade", Uf = uf, CodigoInep = inep
    };

    public static AvaliacaoRequest AvaliacaoSemRampaEPortaEstreita() => new()
    {
        Respostas = new List<RespostaDto>
        {
            new(ItemAcessibilidade.Rampa, Resposta.Nao),
            new(ItemAcessibilidade.PisoTatil, Resposta.Nao),
            new(ItemAcessibilidade.Corrimao, Resposta.Sim)
        },
        Medicoes = new List<MedicaoDto> { new(TipoMedicao.LarguraPortaCm, 70) }
    };
}

public class EstabelecimentoServiceTests
{
    [Fact]
    public async Task Criar_PersisteEDevolveRegiao()
    {
        var amb = new Ambiente();
        var criado = await amb.Estabelecimentos.CriarAsync(Ambiente.RequestEscola("AM"));

        Assert.True(criado.Id > 0);
        Assert.Equal("Norte", criado.Regiao);
        Assert.Single(amb.Banco.Estabelecimentos);
    }

    [Fact]
    public async Task Listar_FiltraPorUf_EPagina()
    {
        var amb = new Ambiente();
        for (var i = 0; i < 3; i++) await amb.Estabelecimentos.CriarAsync(Ambiente.RequestEscola("SP"));
        await amb.Estabelecimentos.CriarAsync(Ambiente.RequestEscola("RJ"));

        var pagina = await amb.Estabelecimentos.ListarAsync("sp", null, 1, 2);

        Assert.Equal(3, pagina.Total);
        Assert.Equal(2, pagina.Itens.Count);
    }

    [Fact]
    public async Task ObterInexistente_LancaNaoEncontrado()
    {
        await Assert.ThrowsAsync<NaoEncontradoException>(() => new Ambiente().Estabelecimentos.ObterAsync(999));
    }

    [Fact]
    public async Task Remover_ExcluiORegistro()
    {
        var amb = new Ambiente();
        var criado = await amb.Estabelecimentos.CriarAsync(Ambiente.RequestEscola());
        await amb.Estabelecimentos.RemoverAsync(criado.Id);
        Assert.Empty(amb.Banco.Estabelecimentos);
    }
}

public class DiagnosticoServiceTests
{
    [Fact]
    public async Task Gerar_AplicaRegras_EOrdenaBarreirasPorGravidade()
    {
        var amb = new Ambiente();
        var est = await amb.Estabelecimentos.CriarAsync(Ambiente.RequestEscola());
        var av = await amb.Avaliacoes.CriarAsync(est.Id, Ambiente.AvaliacaoSemRampaEPortaEstreita());

        var diag = await amb.Diagnosticos.GerarAsync(av.Id);

        Assert.Contains(diag.Barreiras, b => b.Tipo == TipoBarreira.Rampa);
        Assert.Contains(diag.Barreiras, b => b.Tipo == TipoBarreira.Porta);
        Assert.Equal(Gravidade.Alta, diag.Barreiras[0].Gravidade);
        Assert.Equal(NivelRisco.Alto, diag.NivelRisco);   // 2 barreiras altas
        Assert.Equal(Diagnostico.AvisoTriagem, diag.Aviso);
    }

    [Fact]
    public async Task Gerar_ComCodigoInep_UsaAPrevisaoDoModeloDeMl()
    {
        var amb = new Ambiente();
        amb.Banco.Previsoes.Add(Dados.NovaPrevisao(35000001, prob: 0.95, faixa: FaixaRisco.Alta));
        amb.Banco.Previsoes.Add(Dados.NovaPrevisao(35000002, prob: 0.10, declarado: false, faixa: FaixaRisco.Baixa));
        var est = await amb.Estabelecimentos.CriarAsync(Ambiente.RequestEscola("SP", 35000001));
        var av = await amb.Avaliacoes.CriarAsync(est.Id, new AvaliacaoRequest());

        var diag = await amb.Diagnosticos.GerarAsync(av.Id);

        Assert.Equal(0.95, diag.ProbabilidadeSemRampaMl!.Value, 2);
        Assert.Equal(FaixaRisco.Alta, diag.FaixaRiscoMl);
        Assert.Contains("SP", diag.ContextoRegional!);
        Assert.Contains("50", diag.ContextoRegional!);   // 1 de 2 escolas sem rampa declarada = 50%
    }

    [Fact]
    public async Task Gerar_AvaliacaoInexistente_LancaNaoEncontrado()
    {
        await Assert.ThrowsAsync<NaoEncontradoException>(() => new Ambiente().Diagnosticos.GerarAsync(123));
    }
}

public class ParceiroEEncaminhamentoServiceTests
{
    private static async Task<(Ambiente amb, DiagnosticoResponse diag)> CenarioAsync()
    {
        var amb = new Ambiente();
        var est = await amb.Estabelecimentos.CriarAsync(Ambiente.RequestEscola("SP"));
        var av = await amb.Avaliacoes.CriarAsync(est.Id, Ambiente.AvaliacaoSemRampaEPortaEstreita());
        var diag = await amb.Diagnosticos.GerarAsync(av.Id);
        return (amb, diag);
    }

    private static ParceiroRequest Parceiro(string nome, Especialidade esp, params string[] ufs) =>
        new() { Nome = nome, Especialidades = esp, UfsAtendidas = ufs.ToList(), Email = "contato@exemplo.com" };

    [Fact]
    public async Task Sugerir_FiltraPorUf_EOrdenaPorAderencia()
    {
        var (amb, diag) = await CenarioAsync();
        await amb.Parceiros.CriarAsync(Parceiro("B Obras e Consultoria", Especialidade.ObrasAcessibilidade | Especialidade.ConsultoriaTecnica, "SP"));
        await amb.Parceiros.CriarAsync(Parceiro("A Sinalização", Especialidade.Sinalizacao, "SP"));
        await amb.Parceiros.CriarAsync(Parceiro("C Obras (outra UF)", Especialidade.ObrasAcessibilidade, "RJ"));
        await amb.Parceiros.CriarAsync(Parceiro("D Treinamento", Especialidade.TreinamentoAtendimento, "SP"));

        var sugeridos = await amb.Parceiros.SugerirParaDiagnosticoAsync(diag.Id);

        Assert.Equal(2, sugeridos.Count);                         // C (outra UF) e D (não é necessário) ficam de fora
        Assert.Equal("B Obras e Consultoria", sugeridos[0].Parceiro.Nome);   // maior aderência primeiro
        Assert.Equal("A Sinalização", sugeridos[1].Parceiro.Nome);
        Assert.True(sugeridos[0].Aderencia > sugeridos[1].Aderencia);
    }

    [Fact]
    public async Task Encaminhar_NaoPermiteDuplicidade()
    {
        var (amb, diag) = await CenarioAsync();
        var parceiro = await amb.Parceiros.CriarAsync(Parceiro("Obras", Especialidade.ObrasAcessibilidade, "SP"));
        var req = new EncaminhamentoRequest { DiagnosticoId = diag.Id, ParceiroId = parceiro.Id };

        var criado = await amb.Encaminhamentos.CriarAsync(req);

        Assert.Equal(StatusEncaminhamento.Pendente, criado.Status);
        Assert.Equal("Obras", criado.ParceiroNome);
        await Assert.ThrowsAsync<DomainException>(() => amb.Encaminhamentos.CriarAsync(req));
    }

    [Fact]
    public async Task AlterarStatus_SegueOFluxo_EBloqueiaTransicaoInvalida()
    {
        var (amb, diag) = await CenarioAsync();
        var parceiro = await amb.Parceiros.CriarAsync(Parceiro("Obras", Especialidade.ObrasAcessibilidade, "SP"));
        var enc = await amb.Encaminhamentos.CriarAsync(new EncaminhamentoRequest { DiagnosticoId = diag.Id, ParceiroId = parceiro.Id });

        var enviado = await amb.Encaminhamentos.AlterarStatusAsync(enc.Id, new AlterarStatusRequest { Status = StatusEncaminhamento.Enviado });

        Assert.Equal(StatusEncaminhamento.Enviado, enviado.Status);
        await Assert.ThrowsAsync<DomainException>(() =>
            amb.Encaminhamentos.AlterarStatusAsync(enc.Id, new AlterarStatusRequest { Status = StatusEncaminhamento.Concluido }));
    }

    [Fact]
    public async Task Encaminhar_DiagnosticoInexistente_LancaNaoEncontrado()
    {
        var amb = new Ambiente();
        await Assert.ThrowsAsync<NaoEncontradoException>(() =>
            amb.Encaminhamentos.CriarAsync(new EncaminhamentoRequest { DiagnosticoId = 1, ParceiroId = 1 }));
    }
}

public class RiscoServiceTests
{
    private static string CriarCsvTemporario()
    {
        var caminho = Path.Combine(Path.GetTempPath(), $"previsoes_{Guid.NewGuid():N}.csv");
        File.WriteAllLines(caminho, new[]
        {
            "co_entidade,regiao,sg_uf,co_municipio,tp_dependencia,prob_sem_rampa,sem_rampa_declarado,faixa_risco",
            "1,Sudeste,SP,3550308,3,0.9000,1,ALTA",
            "2,Sudeste,SP,3550308,2,0.1000,0,BAIXA",
            "3,Norte,AM,1302603,3,0.8000,1,ALTA",
            "linha invalida"
        });
        return caminho;
    }

    [Fact]
    public async Task Importar_LeCsv_EIgnoraLinhasInvalidas()
    {
        var amb = new Ambiente();
        var caminho = CriarCsvTemporario();
        try
        {
            var resultado = await amb.Risco.ImportarAsync(caminho, null);

            Assert.Equal(3, resultado.Importadas);
            Assert.Equal(1, resultado.Ignoradas);
            Assert.Equal(3, await amb.Risco.ContarAsync());
        }
        finally { File.Delete(caminho); }
    }

    [Fact]
    public async Task Importar_ComFiltroDeUf_ImportaSomenteAUf()
    {
        var amb = new Ambiente();
        var caminho = CriarCsvTemporario();
        try
        {
            var resultado = await amb.Risco.ImportarAsync(caminho, "am");
            Assert.Equal(1, resultado.Importadas);
        }
        finally { File.Delete(caminho); }
    }

    [Fact]
    public async Task ResumoPorRegiao_CalculaPercentuais()
    {
        var amb = new Ambiente();
        var caminho = CriarCsvTemporario();
        try
        {
            await amb.Risco.ImportarAsync(caminho, null);
            var resumo = await amb.Risco.ResumoPorRegiaoAsync();

            var sudeste = Assert.Single(resumo, r => r.Chave == "Sudeste");
            Assert.Equal(2, sudeste.TotalEscolas);
            Assert.Equal(50.0, sudeste.PercentualSemRampaDeclarado, 1);
            Assert.Equal(1, sudeste.EscolasRiscoAlto);
        }
        finally { File.Delete(caminho); }
    }

    [Fact]
    public async Task ImportarArquivoInexistente_LancaDomainException()
    {
        await Assert.ThrowsAsync<DomainException>(() => new Ambiente().Risco.ImportarAsync("/nao/existe.csv", null));
    }
}
