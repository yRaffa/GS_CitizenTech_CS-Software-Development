using CitizenTech.Domain.Entities;
using CitizenTech.Domain.Enums;

namespace CitizenTech.Infrastructure.Persistencia;

/// <summary>Parceiros FICTÍCIOS (dados de exemplo) criados na primeira execução para demonstrar o encaminhamento.</summary>
public static class ParceirosIniciais
{
    public static IEnumerable<Parceiro> Criar()
    {
        const string email = "contato@exemplo.com.br";
        yield return new Parceiro("Rampa Certa Obras (exemplo)", "Obras de acessibilidade e adequação de rampas.",
            Especialidade.ObrasAcessibilidade | Especialidade.ConsultoriaTecnica, new[] { "SP", "RJ", "MG", "ES" }, email, "(11) 0000-0001");
        yield return new Parceiro("Norte Acessível Engenharia (exemplo)", "Obras e consultoria técnica na região Norte.",
            Especialidade.ObrasAcessibilidade | Especialidade.ConsultoriaTecnica, new[] { "AM", "PA", "AC", "RO", "RR", "AP", "TO" }, email, "(92) 0000-0002");
        yield return new Parceiro("Nordeste Inclusivo (exemplo)", "Obras e sinalização no Nordeste.",
            Especialidade.ObrasAcessibilidade | Especialidade.Sinalizacao, new[] { "BA", "PE", "CE", "MA", "PB", "RN", "AL", "SE", "PI" }, email, "(71) 0000-0003");
        yield return new Parceiro("Sul Acessível (exemplo)", "Obras de acessibilidade na região Sul.",
            Especialidade.ObrasAcessibilidade, new[] { "PR", "SC", "RS" }, email, "(41) 0000-0004");
        yield return new Parceiro("Inclusão Sinalizada (exemplo)", "Sinalização tátil, visual e de vagas.",
            Especialidade.Sinalizacao, new[] { "*" }, email, "(11) 0000-0005");
        yield return new Parceiro("Libras em Ação (exemplo)", "Capacitação de atendimento inclusivo e tecnologia assistiva.",
            Especialidade.TreinamentoAtendimento | Especialidade.TecnologiaAssistiva, new[] { "*" }, email, "(61) 0000-0006");
        yield return new Parceiro("Consultoria NBR Brasil (exemplo)", "Laudos e consultoria técnica de acessibilidade.",
            Especialidade.ConsultoriaTecnica, new[] { "*" }, email, "(21) 0000-0007");
    }
}
