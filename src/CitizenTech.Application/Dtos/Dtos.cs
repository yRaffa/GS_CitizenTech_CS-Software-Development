using System.ComponentModel.DataAnnotations;
using CitizenTech.Domain.Enums;

namespace CitizenTech.Application.Dtos;

// ----------------------------- Estabelecimento -----------------------------
public record EstabelecimentoRequest
{
    [Required, StringLength(150)] public string Nome { get; init; } = string.Empty;
    public TipoEstabelecimento Tipo { get; init; } = TipoEstabelecimento.Outro;
    [Required, StringLength(200)] public string Logradouro { get; init; } = string.Empty;
    [StringLength(20)] public string? Numero { get; init; }
    [StringLength(100)] public string? Bairro { get; init; }
    [Required, StringLength(100)] public string Cidade { get; init; } = string.Empty;
    [Required, StringLength(2, MinimumLength = 2)] public string Uf { get; init; } = string.Empty;
    /// <summary>Somente escolas: permite cruzar com a previsão do modelo de ML.</summary>
    public long? CodigoInep { get; init; }
    [EmailAddress, StringLength(150)] public string? Email { get; init; }
}

public record EstabelecimentoResponse(
    int Id, string Nome, TipoEstabelecimento Tipo, string Logradouro, string? Numero, string Bairro,
    string Cidade, string Uf, string Regiao, long? CodigoInep, string? Email, DateTime DataCadastro);

// ------------------------------- Avaliação ---------------------------------
public record RespostaDto(ItemAcessibilidade Item, Resposta Valor);

public record MedicaoDto(TipoMedicao Tipo, double Valor);

public record AvaliacaoRequest
{
    public List<RespostaDto> Respostas { get; init; } = new();
    public List<MedicaoDto> Medicoes { get; init; } = new();
    [StringLength(500)] public string? Observacoes { get; init; }
}

public record AvaliacaoResponse(
    int Id, int EstabelecimentoId, DateTime DataAvaliacao, string? Observacoes,
    IReadOnlyList<RespostaDto> Respostas, IReadOnlyList<MedicaoDto> Medicoes);

// ------------------------------- Diagnóstico -------------------------------
public record BarreiraDto(
    int Id, TipoBarreira Tipo, string Descricao, Gravidade Gravidade,
    Especialidade EspecialidadeSugerida, string Recomendacao);

public record DiagnosticoResponse(
    int Id, int AvaliacaoId, int EstabelecimentoId, string EstabelecimentoNome, DateTime DataGeracao,
    NivelRisco NivelRisco, double? ProbabilidadeSemRampaMl, FaixaRisco? FaixaRiscoMl,
    string? ContextoRegional, string Resumo, string Aviso, IReadOnlyList<BarreiraDto> Barreiras);

// -------------------------------- Parceiro ---------------------------------
public record ParceiroRequest
{
    [Required, StringLength(150)] public string Nome { get; init; } = string.Empty;
    [StringLength(500)] public string? Descricao { get; init; }
    public Especialidade Especialidades { get; init; }
    /// <summary>UFs atendidas (ex.: ["SP","RJ"]) ou ["*"] para todo o Brasil.</summary>
    public List<string> UfsAtendidas { get; init; } = new();
    [Required, EmailAddress, StringLength(150)] public string Email { get; init; } = string.Empty;
    [StringLength(30)] public string? Telefone { get; init; }
    /// <summary>Saldo inicial de vagas de atendimento.</summary>
    [Range(0, 100000)] public int VagasAtendimento { get; init; }
}

public record ParceiroResponse(
    int Id, string Nome, string Descricao, Especialidade Especialidades, string UfsAtendidas,
    string Email, string? Telefone, bool Ativo, int VagasAtendimento);

public record ParceiroSugeridoResponse(ParceiroResponse Parceiro, Especialidade EspecialidadesCompativeis, int Aderencia);

// ----------------------------- Encaminhamento ------------------------------
public record EncaminhamentoRequest
{
    public int DiagnosticoId { get; init; }
    public int ParceiroId { get; init; }
    [StringLength(500)] public string? Observacao { get; init; }
}

public record AlterarStatusRequest
{
    public StatusEncaminhamento Status { get; init; }
    [StringLength(500)] public string? Observacao { get; init; }
}

public record EncaminhamentoResponse(
    int Id, int DiagnosticoId, int ParceiroId, string ParceiroNome, StatusEncaminhamento Status,
    string? Observacao, DateTime DataCriacao, DateTime DataAtualizacao);

// ------------------------ Risco regional (modelo de ML) --------------------
public record ResumoRiscoResponse(string Chave, int TotalEscolas, double PercentualSemRampaDeclarado, double ProbabilidadeMedia, int EscolasRiscoAlto);

public record PrevisaoEscolaResponse(
    long CodigoInep, string Regiao, string Uf, int CodigoMunicipio, double ProbabilidadeSemRampa,
    bool SemRampaDeclarado, FaixaRisco Faixa);

public record ImportacaoResponse(int Importadas, int Ignoradas, string Arquivo);

// ------------------------------- Equipe ------------------------------------
public record EquipeResponse(string CitizenTech, string Turma, string RmLider, IReadOnlyList<IntegranteResponse> Integrantes);
public record IntegranteResponse(string NomeCompleto, string Rm, string Turma, string NomeAlunoRm, bool Lider);

// -------------------------------- Imagem -----------------------------------
/// <summary>Dados do upload (multipart). Os bytes chegam já lidos do arquivo.</summary>
public record ImagemUpload(string Titulo, int EstabelecimentoId, string Executor, string NomeArquivo, string TipoMime, byte[] Dados);

public record ImagemResponse(
    int Id, int EstabelecimentoId, string Titulo, string NomeArquivo, string TipoMime, int TamanhoBytes,
    DateTime DataUtc, string EquipeRmLider, string Executor, string UrlConteudo);

/// <summary>Conteúdo para o GET de bytes: o controller devolve File(bytes, mime).</summary>
public record ImagemConteudo(byte[] Dados, string TipoMime, string NomeArquivo);

// -------------------------------- Reserva ----------------------------------
/// <summary>Encaminhar um diagnóstico a um parceiro reservando vagas (operação transacional).</summary>
public record ReservaRequest
{
    /// <summary>Diagnóstico a encaminhar (o estabelecimento é obtido dele: avaliação → estabelecimento).</summary>
    public int DiagnosticoId { get; init; }
    public int ParceiroId { get; init; }
    /// <summary>Vagas de atendimento a reservar (deve ser maior que zero).</summary>
    public int Quantidade { get; init; }
    /// <summary>NomeAluno_RM de quem executa (precisa estar no equipe.json).</summary>
    public string Executor { get; init; } = string.Empty;
    /// <summary>Somente para o teste T08: lança falha DEPOIS das duas escritas e ANTES do COMMIT (ROLLBACK).</summary>
    public bool SimularFalhaAposEscritas { get; init; }
}

public record ReservaResponse(
    int Id, int EncaminhamentoId, int DiagnosticoId, int ParceiroId, string ParceiroNome, int EstabelecimentoId, int Quantidade,
    int VagasRestantesParceiro, DateTime DataUtc, string EquipeRmLider, string Executor);
