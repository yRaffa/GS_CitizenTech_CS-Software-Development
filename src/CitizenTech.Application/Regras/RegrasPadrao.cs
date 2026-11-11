using CitizenTech.Domain.Enums;

namespace CitizenTech.Application.Regras;

/// <summary>
/// Conjunto de regras da triagem. Os limites são VALORES DE REFERÊNCIA inspirados na NBR 9050
/// (rampa até 8,33%; vão livre de porta mínimo de 80 cm; corredor mínimo de 90 cm).
/// Confirme sempre a versão vigente da norma: a triagem não substitui avaliação técnica presencial.
/// </summary>
public static class RegrasPadrao
{
    public static IReadOnlyList<IRegraDiagnostico> ObterTodas() => new List<IRegraDiagnostico>
    {
        // Questionário
        new RegraItemAusente(ItemAcessibilidade.Rampa, TipoBarreira.Rampa, Gravidade.Alta,
            Especialidade.ObrasAcessibilidade | Especialidade.ConsultoriaTecnica,
            "Ausência de rampa de acesso.",
            "Solicite a um parceiro a avaliação de uma rampa de acesso conforme a NBR 9050."),
        new RegraItemAusente(ItemAcessibilidade.BanheiroAdaptado, TipoBarreira.Banheiro, Gravidade.Alta,
            Especialidade.ObrasAcessibilidade | Especialidade.ConsultoriaTecnica,
            "Ausência de banheiro adaptado.",
            "Solicite avaliação técnica para adaptação de banheiro acessível."),
        new RegraItemAusente(ItemAcessibilidade.Corrimao, TipoBarreira.Corrimao, Gravidade.Media,
            Especialidade.ObrasAcessibilidade,
            "Ausência de corrimão em rampas ou escadas.",
            "Verifique com um parceiro a instalação de corrimãos."),
        new RegraItemAusente(ItemAcessibilidade.PisoTatil, TipoBarreira.PisoTatil, Gravidade.Media,
            Especialidade.Sinalizacao,
            "Ausência de piso tátil (direcional/alerta).",
            "Avalie a instalação de sinalização tátil de piso."),
        new RegraItemAusente(ItemAcessibilidade.SinalizacaoVisual, TipoBarreira.Sinalizacao, Gravidade.Baixa,
            Especialidade.Sinalizacao,
            "Ausência de sinalização visual acessível.",
            "Avalie a adequação da sinalização visual."),
        new RegraItemAusente(ItemAcessibilidade.Elevador, TipoBarreira.Elevador, Gravidade.Baixa,
            Especialidade.ObrasAcessibilidade | Especialidade.ConsultoriaTecnica,
            "Ausência de elevador (relevante se houver mais de um pavimento).",
            "Se houver mais de um pavimento, solicite avaliação de alternativas de acesso vertical."),
        new RegraItemAusente(ItemAcessibilidade.VagaPreferencial, TipoBarreira.Vaga, Gravidade.Baixa,
            Especialidade.Sinalizacao,
            "Ausência de vaga preferencial sinalizada.",
            "Avalie a sinalização de vagas preferenciais."),
        new RegraItemAusente(ItemAcessibilidade.AtendimentoLibras, TipoBarreira.AtendimentoLibras, Gravidade.Media,
            Especialidade.TreinamentoAtendimento | Especialidade.TecnologiaAssistiva,
            "Atendimento sem suporte em Libras.",
            "Busque parceiros de capacitação de atendimento inclusivo ou de tecnologia assistiva."),

        // Medições guiadas
        new RegraMedicaoForaDoLimite(TipoMedicao.InclinacaoRampaPercentual, TipoLimite.Maximo, 8.33, "%",
            "Inclinação da rampa acima do limite de referência", TipoBarreira.InclinacaoRampa, Gravidade.Alta,
            Especialidade.ObrasAcessibilidade | Especialidade.ConsultoriaTecnica,
            "Solicite avaliação técnica da inclinação da rampa."),
        new RegraMedicaoForaDoLimite(TipoMedicao.LarguraPortaCm, TipoLimite.Minimo, 80, "cm",
            "Vão livre da porta abaixo do mínimo de referência", TipoBarreira.Porta, Gravidade.Alta,
            Especialidade.ObrasAcessibilidade | Especialidade.ConsultoriaTecnica,
            "Solicite avaliação técnica para ampliação do vão da porta."),
        new RegraMedicaoForaDoLimite(TipoMedicao.LarguraCorredorCm, TipoLimite.Minimo, 90, "cm",
            "Largura do corredor abaixo do mínimo de referência", TipoBarreira.Corredor, Gravidade.Media,
            Especialidade.ObrasAcessibilidade | Especialidade.ConsultoriaTecnica,
            "Avalie com um parceiro a reorganização do espaço ou a adequação do corredor."),

        // Qualidade da informação
        new RegraInformacaoPendente()
    };
}
