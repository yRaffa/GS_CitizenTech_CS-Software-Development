using CitizenTech.Application.Dtos;
using CitizenTech.Application.Equipe;
using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Models;

namespace CitizenTech.Application.Interfaces;

public interface IEstabelecimentoService
{
    Task<ResultadoPaginado<EstabelecimentoResponse>> ListarAsync(string? uf, TipoEstabelecimento? tipo, int pagina, int tamanhoPagina, CancellationToken ct = default);
    Task<EstabelecimentoResponse> ObterAsync(int id, CancellationToken ct = default);
    Task<EstabelecimentoResponse> CriarAsync(EstabelecimentoRequest request, CancellationToken ct = default);
    Task<EstabelecimentoResponse> AtualizarAsync(int id, EstabelecimentoRequest request, CancellationToken ct = default);
    Task RemoverAsync(int id, CancellationToken ct = default);
}

public interface IAvaliacaoService
{
    Task<AvaliacaoResponse> CriarAsync(int estabelecimentoId, AvaliacaoRequest request, CancellationToken ct = default);
    Task<AvaliacaoResponse> ObterAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<AvaliacaoResponse>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default);
}

public interface IDiagnosticoService
{
    Task<DiagnosticoResponse> GerarAsync(int avaliacaoId, CancellationToken ct = default);
    Task<DiagnosticoResponse> ObterAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<DiagnosticoResponse>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default);
}

public interface IParceiroService
{
    Task<IReadOnlyList<ParceiroResponse>> ListarAsync(Especialidade? especialidades, string? uf, CancellationToken ct = default);
    Task<ParceiroResponse> CriarAsync(ParceiroRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ParceiroSugeridoResponse>> SugerirParaDiagnosticoAsync(int diagnosticoId, CancellationToken ct = default);
}

public interface IEncaminhamentoService
{
    Task<EncaminhamentoResponse> CriarAsync(EncaminhamentoRequest request, CancellationToken ct = default);
    Task<EncaminhamentoResponse> AlterarStatusAsync(int id, AlterarStatusRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<EncaminhamentoResponse>> ListarAsync(StatusEncaminhamento? status, CancellationToken ct = default);
}

public interface IRiscoService
{
    Task<IReadOnlyList<ResumoRiscoResponse>> ResumoPorRegiaoAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ResumoRiscoResponse>> ResumoPorUfAsync(string? regiao, CancellationToken ct = default);
    Task<PrevisaoEscolaResponse> ObterEscolaAsync(long codigoInep, CancellationToken ct = default);
    Task<int> ContarAsync(CancellationToken ct = default);
    Task<ImportacaoResponse> ImportarAsync(string caminhoCsv, string? uf, CancellationToken ct = default);
}

public interface IImagemService
{
    /// <summary>Valida (R7) e grava a imagem com os bytes no banco.</summary>
    Task<ImagemResponse> EnviarAsync(ImagemUpload upload, CancellationToken ct = default);
    Task<ImagemResponse> ObterMetadadosAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ImagemResponse>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default);
    Task<ImagemConteudo> ObterConteudoAsync(int id, CancellationToken ct = default);
}

public interface IReservaService
{
    /// <summary>Operação transacional: baixa o saldo do parceiro e insere a reserva (COMMIT) ou reverte tudo (ROLLBACK).</summary>
    Task<ReservaResponse> ReservarAsync(ReservaRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ReservaResponse>> ListarAsync(int? parceiroId, CancellationToken ct = default);
}
