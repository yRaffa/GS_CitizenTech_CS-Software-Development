using System.Numerics;
using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Exceptions;
using CitizenTech.Domain.Interfaces;

namespace CitizenTech.Application.Servicos;

public class ParceiroService : IParceiroService
{
    private readonly IParceiroRepository _parceiros;
    private readonly IDiagnosticoRepository _diagnosticos;
    private readonly IUnitOfWork _uow;

    public ParceiroService(IParceiroRepository parceiros, IDiagnosticoRepository diagnosticos, IUnitOfWork uow)
    {
        _parceiros = parceiros;
        _diagnosticos = diagnosticos;
        _uow = uow;
    }

    public async Task<IReadOnlyList<ParceiroResponse>> ListarAsync(Especialidade? especialidades, string? uf, CancellationToken ct = default)
    {
        var lista = await _parceiros.ListarAsync(especialidades, uf?.Trim().ToUpperInvariant(), ct);
        return lista.Select(p => p.ToResponse()).ToList();
    }

    public async Task<ParceiroResponse> CriarAsync(ParceiroRequest request, CancellationToken ct = default)
    {
        var parceiro = new Parceiro(request.Nome, request.Descricao ?? string.Empty, request.Especialidades,
                                    request.UfsAtendidas, request.Email, request.Telefone);
        await _parceiros.AdicionarAsync(parceiro, ct);
        await _uow.SalvarAsync(ct);
        return parceiro.ToResponse();
    }

    /// <summary>
    /// Sugere parceiros que atendem a UF do estabelecimento e cobrem as especialidades exigidas pelas barreiras.
    /// Ordena pela aderência (quantidade de especialidades necessárias que o parceiro cobre).
    /// </summary>
    public async Task<IReadOnlyList<ParceiroSugeridoResponse>> SugerirParaDiagnosticoAsync(int diagnosticoId, CancellationToken ct = default)
    {
        var diagnostico = await _diagnosticos.ObterCompletoAsync(diagnosticoId, ct)
            ?? throw new NaoEncontradoException(nameof(Diagnostico), diagnosticoId);

        var necessarias = diagnostico.EspecialidadesNecessarias;
        if (necessarias == Especialidade.Nenhuma) return Array.Empty<ParceiroSugeridoResponse>();

        var uf = diagnostico.Avaliacao?.Estabelecimento?.Endereco.Uf;
        var candidatos = await _parceiros.ListarAsync(necessarias, uf, ct);

        return candidatos
            .Select(p =>
            {
                var compativeis = p.Especialidades & necessarias;
                return new ParceiroSugeridoResponse(p.ToResponse(), compativeis, BitOperations.PopCount((uint)(int)compativeis));
            })
            .OrderByDescending(s => s.Aderencia)
            .ThenBy(s => s.Parceiro.Nome)
            .ToList();
    }
}
