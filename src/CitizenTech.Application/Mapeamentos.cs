using CitizenTech.Application.Dtos;
using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Models;

namespace CitizenTech.Application;

/// <summary>Conversões entidade -> DTO (métodos de extensão).</summary>
public static class Mapeamentos
{
    public static EstabelecimentoResponse ToResponse(this Estabelecimento e) => new(
        e.Id, e.Nome, e.Tipo, e.Endereco.Logradouro, e.Endereco.Numero, e.Endereco.Bairro,
        e.Endereco.Cidade, e.Endereco.Uf, e.Regiao, e.CodigoInep, e.Email, e.DataCadastro);

    public static AvaliacaoResponse ToResponse(this Avaliacao a) => new(
        a.Id, a.EstabelecimentoId, a.DataAvaliacao, a.Observacoes,
        a.Respostas.Select(r => new RespostaDto(r.Item, r.Valor)).ToList(),
        a.Medicoes.Select(m => new MedicaoDto(m.Tipo, m.Valor)).ToList());

    public static BarreiraDto ToDto(this Barreira b) =>
        new(b.Id, b.Tipo, b.Descricao, b.Gravidade, b.EspecialidadeSugerida, b.Recomendacao);

    public static DiagnosticoResponse ToResponse(this Diagnostico d, int estabelecimentoId, string estabelecimentoNome) => new(
        d.Id, d.AvaliacaoId, estabelecimentoId, estabelecimentoNome, d.DataGeracao, d.NivelRisco,
        d.ProbabilidadeSemRampaMl, d.FaixaRiscoMl, d.ContextoRegional, d.Resumo, Diagnostico.AvisoTriagem,
        d.Barreiras.OrderByDescending(b => b.Gravidade).Select(b => b.ToDto()).ToList());

    public static ParceiroResponse ToResponse(this Parceiro p) =>
        new(p.Id, p.Nome, p.Descricao, p.Especialidades, p.UfsAtendidas, p.Email, p.Telefone, p.Ativo);

    public static EncaminhamentoResponse ToResponse(this Encaminhamento e, string parceiroNome) =>
        new(e.Id, e.DiagnosticoId, e.ParceiroId, parceiroNome, e.Status, e.Observacao, e.DataCriacao, e.DataAtualizacao);

    public static ResumoRiscoResponse ToResponse(this ResumoRisco r) =>
        new(r.Chave, r.TotalEscolas, r.PercentualSemRampaDeclarado, r.ProbabilidadeMedia, r.EscolasRiscoAlto);

    public static PrevisaoEscolaResponse ToResponse(this PrevisaoRisco p) =>
        new(p.CodigoInep, p.Regiao, p.Uf, p.CodigoMunicipio, p.ProbSemRampa, p.SemRampaDeclarado, p.Faixa);
}
