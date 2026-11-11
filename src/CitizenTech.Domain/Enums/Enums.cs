namespace CitizenTech.Domain.Enums;

public enum TipoEstabelecimento { Escola = 1, Saude = 2, Comercio = 3, Servico = 4, Cultura = 5, Outro = 9 }

/// <summary>Itens do questionário de acessibilidade respondidos pelo estabelecimento.</summary>
public enum ItemAcessibilidade
{
    Rampa = 1, Corrimao = 2, BanheiroAdaptado = 3, PisoTatil = 4,
    SinalizacaoVisual = 5, Elevador = 6, VagaPreferencial = 7, AtendimentoLibras = 8
}

public enum Resposta { Sim = 1, Nao = 2, NaoSei = 3 }

/// <summary>Medições guiadas (a plataforma orienta o usuário a medir e informar o valor).</summary>
public enum TipoMedicao { InclinacaoRampaPercentual = 1, LarguraPortaCm = 2, LarguraCorredorCm = 3 }

public enum TipoBarreira
{
    Rampa = 1, InclinacaoRampa = 2, Porta = 3, Corredor = 4, Banheiro = 5, Corrimao = 6,
    PisoTatil = 7, Sinalizacao = 8, Elevador = 9, Vaga = 10, AtendimentoLibras = 11, InformacaoPendente = 12
}

public enum Gravidade { Baixa = 1, Media = 2, Alta = 3 }

public enum NivelRisco { Baixo = 1, Medio = 2, Alto = 3 }

/// <summary>Faixa de risco calculada pelo modelo de ML (entrega de IA &amp; ML).</summary>
public enum FaixaRisco { Baixa = 1, Media = 2, Alta = 3 }

/// <summary>Especialidades dos parceiros. A plataforma NÃO executa obras nem cursos: apenas conecta.</summary>
[Flags]
public enum Especialidade
{
    Nenhuma = 0,
    ObrasAcessibilidade = 1,
    ConsultoriaTecnica = 2,
    TecnologiaAssistiva = 4,
    TreinamentoAtendimento = 8,
    Sinalizacao = 16
}

public enum StatusEncaminhamento { Pendente = 1, Enviado = 2, EmAnalise = 3, Concluido = 4, Cancelado = 5 }
