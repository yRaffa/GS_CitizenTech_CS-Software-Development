using CitizenTech.Application.Dtos;
using CitizenTech.Application.Interfaces;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;
using CitizenTech.Domain.Exceptions;
using CitizenTech.Domain.Interfaces;
using CitizenTech.Domain.Models;

namespace CitizenTech.Application.Servicos;

public class EstabelecimentoService : IEstabelecimentoService
{
    private readonly IEstabelecimentoRepository _repo;
    private readonly IUnitOfWork _uow;

    public EstabelecimentoService(IEstabelecimentoRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<ResultadoPaginado<EstabelecimentoResponse>> ListarAsync(
        string? uf, TipoEstabelecimento? tipo, int pagina, int tamanhoPagina, CancellationToken ct = default)
    {
        pagina = Math.Max(1, pagina);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 100);
        var resultado = await _repo.ListarAsync(uf?.Trim().ToUpperInvariant(), tipo, pagina, tamanhoPagina, ct);
        return new ResultadoPaginado<EstabelecimentoResponse>(
            resultado.Itens.Select(e => e.ToResponse()).ToList(), resultado.Total, pagina, tamanhoPagina);
    }

    public async Task<EstabelecimentoResponse> ObterAsync(int id, CancellationToken ct = default)
        => (await BuscarAsync(id, ct)).ToResponse();

    public async Task<EstabelecimentoResponse> CriarAsync(EstabelecimentoRequest request, CancellationToken ct = default)
    {
        var estabelecimento = Montar(request);
        await _repo.AdicionarAsync(estabelecimento, ct);
        await _uow.SalvarAsync(ct);
        return estabelecimento.ToResponse();
    }

    public async Task<EstabelecimentoResponse> AtualizarAsync(int id, EstabelecimentoRequest request, CancellationToken ct = default)
    {
        var existente = await BuscarAsync(id, ct);
        var novo = Montar(request);
        existente.Atualizar(novo.Nome, novo.Tipo, novo.Endereco, novo.CodigoInep, novo.Email);
        await _uow.SalvarAsync(ct);
        return existente.ToResponse();
    }

    public async Task RemoverAsync(int id, CancellationToken ct = default)
    {
        var existente = await BuscarAsync(id, ct);
        _repo.Remover(existente);
        await _uow.SalvarAsync(ct);
    }

    private async Task<Estabelecimento> BuscarAsync(int id, CancellationToken ct) =>
        await _repo.ObterPorIdAsync(id, ct) ?? throw new NaoEncontradoException(nameof(Estabelecimento), id);

    private static Estabelecimento Montar(EstabelecimentoRequest r) => new(
        r.Nome, r.Tipo, new Endereco(r.Logradouro, r.Numero, r.Bairro, r.Cidade, r.Uf), r.CodigoInep, r.Email);
}
