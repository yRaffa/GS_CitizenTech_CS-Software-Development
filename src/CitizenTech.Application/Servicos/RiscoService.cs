using CitizenTech.Application.Csv;
using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Exceptions;
using CitizenTech.Domain.Interfaces;

namespace CitizenTech.Application.Servicos;

/// <summary>Consulta e importação das previsões do modelo de ML (entrega de IA &amp; ML).</summary>
public class RiscoService : IRiscoService
{
    private const int TamanhoLote = 5000;
    private readonly IPrevisaoRiscoRepository _previsoes;

    public RiscoService(IPrevisaoRiscoRepository previsoes) => _previsoes = previsoes;

    public async Task<IReadOnlyList<ResumoRiscoResponse>> ResumoPorRegiaoAsync(CancellationToken ct = default) =>
        (await _previsoes.ResumoPorRegiaoAsync(ct)).Select(r => r.ToResponse()).ToList();

    public async Task<IReadOnlyList<ResumoRiscoResponse>> ResumoPorUfAsync(string? regiao, CancellationToken ct = default) =>
        (await _previsoes.ResumoPorUfAsync(regiao, ct)).Select(r => r.ToResponse()).ToList();

    public async Task<PrevisaoEscolaResponse> ObterEscolaAsync(long codigoInep, CancellationToken ct = default)
    {
        var previsao = await _previsoes.ObterPorInepAsync(codigoInep, ct)
            ?? throw new NaoEncontradoException("Previsão de risco da escola", codigoInep);
        return previsao.ToResponse();
    }

    public Task<int> ContarAsync(CancellationToken ct = default) => _previsoes.ContarAsync(ct);

    public async Task<ImportacaoResponse> ImportarAsync(string caminhoCsv, string? uf, CancellationToken ct = default)
    {
        if (!File.Exists(caminhoCsv)) throw new DomainException($"Arquivo não encontrado: {caminhoCsv}");
        var ufFiltro = uf?.Trim().ToUpperInvariant();

        await _previsoes.LimparAsync(ct);

        int importadas = 0, ignoradas = 0;
        var lote = new List<PrevisaoRisco>(TamanhoLote);

        using var leitor = new StreamReader(caminhoCsv);
        await leitor.ReadLineAsync(ct); // descarta o cabeçalho

        string? linha;
        while ((linha = await leitor.ReadLineAsync(ct)) is not null)
        {
            var previsao = PrevisaoCsvParser.Converter(linha);
            if (previsao is null || (ufFiltro is not null && previsao.Uf != ufFiltro)) { ignoradas++; continue; }

            lote.Add(previsao);
            importadas++;
            if (lote.Count >= TamanhoLote)
            {
                await _previsoes.InserirLoteAsync(lote, ct);
                lote.Clear();
            }
        }
        if (lote.Count > 0) await _previsoes.InserirLoteAsync(lote, ct);

        return new ImportacaoResponse(importadas, ignoradas, Path.GetFileName(caminhoCsv));
    }
}
