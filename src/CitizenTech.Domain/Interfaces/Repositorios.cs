using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Models;

namespace CitizenTech.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SalvarAsync(CancellationToken ct = default);

    /// <summary>Inicia uma transação local do banco (BEGIN). Use CommitAsync/RollbackAsync; ao descartar sem commit, reverte.</summary>
    Task<ITransacaoBanco> IniciarTransacaoAsync(CancellationToken ct = default);
}

public interface ITransacaoBanco : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}

public interface IEstabelecimentoRepository
{
    Task<Estabelecimento?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<ResultadoPaginado<Estabelecimento>> ListarAsync(string? uf, TipoEstabelecimento? tipo, int pagina, int tamanhoPagina, CancellationToken ct = default);
    Task AdicionarAsync(Estabelecimento estabelecimento, CancellationToken ct = default);
    void Remover(Estabelecimento estabelecimento);
}

public interface IAvaliacaoRepository
{
    /// <summary>Carrega a avaliação com respostas, medições e estabelecimento.</summary>
    Task<Avaliacao?> ObterCompletaAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Avaliacao>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default);
    Task AdicionarAsync(Avaliacao avaliacao, CancellationToken ct = default);
}

public interface IDiagnosticoRepository
{
    /// <summary>Carrega o diagnóstico com barreiras, avaliação e estabelecimento.</summary>
    Task<Diagnostico?> ObterCompletoAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Diagnostico>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default);
    Task AdicionarAsync(Diagnostico diagnostico, CancellationToken ct = default);
}

public interface IParceiroRepository
{
    Task<Parceiro?> ObterPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Parceiros ativos. <paramref name="especialidades"/>: qualquer uma das especialidades (flags). <paramref name="uf"/>: UF atendida.</summary>
    Task<IReadOnlyList<Parceiro>> ListarAsync(Especialidade? especialidades, string? uf, CancellationToken ct = default);
    Task<int> ContarAsync(CancellationToken ct = default);
    Task AdicionarAsync(Parceiro parceiro, CancellationToken ct = default);
}

public interface IEncaminhamentoRepository
{
    Task<Encaminhamento?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Encaminhamento>> ListarAsync(StatusEncaminhamento? status, CancellationToken ct = default);
    Task<bool> ExisteAsync(int diagnosticoId, int parceiroId, CancellationToken ct = default);
    Task AdicionarAsync(Encaminhamento encaminhamento, CancellationToken ct = default);
}

public interface IPrevisaoRiscoRepository
{
    Task<PrevisaoRisco?> ObterPorInepAsync(long codigoInep, CancellationToken ct = default);
    Task<ResumoRisco?> ObterResumoUfAsync(string uf, CancellationToken ct = default);
    Task<IReadOnlyList<ResumoRisco>> ResumoPorRegiaoAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ResumoRisco>> ResumoPorUfAsync(string? regiao, CancellationToken ct = default);
    Task<int> ContarAsync(CancellationToken ct = default);

    /// <summary>Remove todas as previsões imediatamente (operação em lote).</summary>
    Task LimparAsync(CancellationToken ct = default);

    /// <summary>Insere um lote e persiste imediatamente (importação em massa, sem acumular entidades rastreadas).</summary>
    Task InserirLoteAsync(IReadOnlyList<PrevisaoRisco> lote, CancellationToken ct = default);
}

public interface IImagemRepository
{
    Task AdicionarAsync(ImagemEstabelecimento imagem, CancellationToken ct = default);

    /// <summary>Somente metadados (não carrega os bytes).</summary>
    Task<ImagemMetadados?> ObterMetadadosAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ImagemMetadados>> ListarPorEstabelecimentoAsync(int estabelecimentoId, CancellationToken ct = default);

    /// <summary>Carrega a imagem completa, com os bytes.</summary>
    Task<ImagemEstabelecimento?> ObterComConteudoAsync(int id, CancellationToken ct = default);
}

public interface IReservaRepository
{
    Task AdicionarAsync(ReservaAtendimento reserva, CancellationToken ct = default);
    Task<IReadOnlyList<ReservaAtendimento>> ListarAsync(int? parceiroId, CancellationToken ct = default);
}
