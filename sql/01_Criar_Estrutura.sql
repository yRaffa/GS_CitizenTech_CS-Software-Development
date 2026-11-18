/* =====================================================================
   CitizenTech (Acessibilidade) - 01_Criar_Estrutura.sql
   Banco: Microsoft SQL Server (testado com a instância LocalDB / Express; T-SQL).
   Cria o banco CitizenTechDb e as 11 tabelas. É a ÚNICA fonte da estrutura:
   a API (EF Core) não cria tabelas. Execute este script ANTES de iniciar a API.
   Reexecutável: apaga e recria as tabelas (apenas dados fictícios).
   ===================================================================== */
IF DB_ID(N'CitizenTechDb') IS NULL
    CREATE DATABASE CitizenTechDb;
GO
USE CitizenTechDb;
GO

/* ---- remove na ordem inversa das dependências ---- */
DROP TABLE IF EXISTS dbo.ReservasAtendimento;
DROP TABLE IF EXISTS dbo.ImagensEstabelecimento;
DROP TABLE IF EXISTS dbo.Encaminhamentos;
DROP TABLE IF EXISTS dbo.Barreiras;
DROP TABLE IF EXISTS dbo.Diagnosticos;
DROP TABLE IF EXISTS dbo.RespostasItens;
DROP TABLE IF EXISTS dbo.Medicoes;
DROP TABLE IF EXISTS dbo.Avaliacoes;
DROP TABLE IF EXISTS dbo.Parceiros;
DROP TABLE IF EXISTS dbo.PrevisoesRisco;
DROP TABLE IF EXISTS dbo.Estabelecimentos;
GO

/* ---- 1. Estabelecimentos (local avaliado; o endereço é um objeto de valor mapeado nas colunas abaixo) ---- */
CREATE TABLE dbo.Estabelecimentos (
    Id           INT            IDENTITY(1,1) NOT NULL,
    Nome         NVARCHAR(150)  NOT NULL,
    Tipo         INT            NOT NULL,   -- 1 Escola, 2 Saude, 3 Comercio, 4 Servico, 5 Cultura, 9 Outro
    CodigoInep   BIGINT         NULL,       -- somente escolas (cruza com a previsão do modelo de ML)
    Email        NVARCHAR(150)  NULL,
    DataCadastro DATETIME2      NOT NULL,
    Logradouro   NVARCHAR(200)  NOT NULL,
    Numero       NVARCHAR(20)   NULL,
    Bairro       NVARCHAR(100)  NOT NULL,
    Cidade       NVARCHAR(100)  NOT NULL,
    Uf           NVARCHAR(2)    NOT NULL,
    CONSTRAINT PK_Estabelecimentos PRIMARY KEY (Id)
);
CREATE INDEX IX_Estabelecimentos_CodigoInep ON dbo.Estabelecimentos (CodigoInep);
GO

/* ---- 2. Avaliacoes (questionário respondido por um estabelecimento) ---- */
CREATE TABLE dbo.Avaliacoes (
    Id               INT           IDENTITY(1,1) NOT NULL,
    EstabelecimentoId INT          NOT NULL,
    DataAvaliacao    DATETIME2     NOT NULL,
    Observacoes      NVARCHAR(500) NULL,
    CONSTRAINT PK_Avaliacoes PRIMARY KEY (Id),
    CONSTRAINT FK_Avaliacoes_Estabelecimentos FOREIGN KEY (EstabelecimentoId) REFERENCES dbo.Estabelecimentos (Id) ON DELETE CASCADE
);
CREATE INDEX IX_Avaliacoes_EstabelecimentoId ON dbo.Avaliacoes (EstabelecimentoId);
GO

CREATE TABLE dbo.RespostasItens (
    Id          INT IDENTITY(1,1) NOT NULL,
    AvaliacaoId INT NOT NULL,
    Item        INT NOT NULL,   -- ItemAcessibilidade (1 Rampa ... 8 AtendimentoLibras)
    Valor       INT NOT NULL,   -- 1 Sim, 2 Nao, 3 NaoSei
    CONSTRAINT PK_RespostasItens PRIMARY KEY (Id),
    CONSTRAINT FK_RespostasItens_Avaliacoes FOREIGN KEY (AvaliacaoId) REFERENCES dbo.Avaliacoes (Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IX_RespostasItens_AvaliacaoId_Item ON dbo.RespostasItens (AvaliacaoId, Item);
GO

CREATE TABLE dbo.Medicoes (
    Id          INT   IDENTITY(1,1) NOT NULL,
    AvaliacaoId INT   NOT NULL,
    Tipo        INT   NOT NULL,   -- 1 inclinação da rampa (%), 2 largura da porta (cm), 3 largura do corredor (cm)
    Valor       FLOAT NOT NULL,
    CONSTRAINT PK_Medicoes PRIMARY KEY (Id),
    CONSTRAINT FK_Medicoes_Avaliacoes FOREIGN KEY (AvaliacaoId) REFERENCES dbo.Avaliacoes (Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IX_Medicoes_AvaliacaoId_Tipo ON dbo.Medicoes (AvaliacaoId, Tipo);
GO

/* ---- Diagnosticos e Barreiras (resultado da triagem) ---- */
CREATE TABLE dbo.Diagnosticos (
    Id                      INT           IDENTITY(1,1) NOT NULL,
    AvaliacaoId             INT           NOT NULL,
    DataGeracao             DATETIME2     NOT NULL,
    NivelRisco              INT           NOT NULL,   -- 1 Baixo, 2 Medio, 3 Alto
    ProbabilidadeSemRampaMl FLOAT         NULL,
    FaixaRiscoMl            INT           NULL,       -- 1 Baixa, 2 Media, 3 Alta
    ContextoRegional        NVARCHAR(500) NULL,
    Resumo                  NVARCHAR(500) NOT NULL,
    CONSTRAINT PK_Diagnosticos PRIMARY KEY (Id),
    CONSTRAINT FK_Diagnosticos_Avaliacoes FOREIGN KEY (AvaliacaoId) REFERENCES dbo.Avaliacoes (Id) ON DELETE CASCADE
);
CREATE INDEX IX_Diagnosticos_AvaliacaoId ON dbo.Diagnosticos (AvaliacaoId);
GO

CREATE TABLE dbo.Barreiras (
    Id                   INT           IDENTITY(1,1) NOT NULL,
    DiagnosticoId        INT           NOT NULL,
    Tipo                 INT           NOT NULL,
    Descricao            NVARCHAR(300) NOT NULL,
    Gravidade            INT           NOT NULL,   -- 1 Baixa, 2 Media, 3 Alta
    EspecialidadeSugerida INT          NOT NULL,   -- flags de Especialidade
    Recomendacao         NVARCHAR(500) NOT NULL,
    CONSTRAINT PK_Barreiras PRIMARY KEY (Id),
    CONSTRAINT FK_Barreiras_Diagnosticos FOREIGN KEY (DiagnosticoId) REFERENCES dbo.Diagnosticos (Id) ON DELETE CASCADE
);
CREATE INDEX IX_Barreiras_DiagnosticoId ON dbo.Barreiras (DiagnosticoId);
GO

/* ---- 3. Parceiros (organizações especializadas; VagasAtendimento é o SALDO usado na transação) ---- */
CREATE TABLE dbo.Parceiros (
    Id               INT           IDENTITY(1,1) NOT NULL,
    Nome             NVARCHAR(150) NOT NULL,
    Descricao        NVARCHAR(500) NOT NULL,
    Especialidades   INT           NOT NULL,   -- flags: 1 Obras, 2 Consultoria, 4 TecAssistiva, 8 Treinamento, 16 Sinalizacao
    UfsAtendidas     NVARCHAR(200) NOT NULL,   -- ';SP;RJ;' ou '*'
    Email            NVARCHAR(150) NOT NULL,
    Telefone         NVARCHAR(30)  NULL,
    Ativo            BIT           NOT NULL,
    VagasAtendimento INT           NOT NULL,
    CONSTRAINT PK_Parceiros PRIMARY KEY (Id),
    CONSTRAINT CK_Parceiros_Vagas CHECK (VagasAtendimento >= 0)
);
GO

CREATE TABLE dbo.Encaminhamentos (
    Id               INT           IDENTITY(1,1) NOT NULL,
    DiagnosticoId    INT           NOT NULL,
    ParceiroId       INT           NOT NULL,
    Status           INT           NOT NULL,   -- 1 Pendente ... 5 Cancelado
    Observacao       NVARCHAR(500) NULL,
    DataCriacao      DATETIME2     NOT NULL,
    DataAtualizacao  DATETIME2     NOT NULL,
    CONSTRAINT PK_Encaminhamentos PRIMARY KEY (Id),
    CONSTRAINT FK_Encaminhamentos_Diagnosticos FOREIGN KEY (DiagnosticoId) REFERENCES dbo.Diagnosticos (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Encaminhamentos_Parceiros FOREIGN KEY (ParceiroId) REFERENCES dbo.Parceiros (Id)
);
CREATE UNIQUE INDEX IX_Encaminhamentos_DiagnosticoId_ParceiroId ON dbo.Encaminhamentos (DiagnosticoId, ParceiroId);
CREATE INDEX IX_Encaminhamentos_ParceiroId ON dbo.Encaminhamentos (ParceiroId);
GO

/* ---- 4. ImagensEstabelecimento (R5): os BYTES ficam em Dados (VARBINARY(MAX)) ---- */
CREATE TABLE dbo.ImagensEstabelecimento (
    Id               INT            IDENTITY(1,1) NOT NULL,
    EstabelecimentoId INT           NOT NULL,
    Titulo           NVARCHAR(100)  NOT NULL,
    NomeArquivo      NVARCHAR(255)  NOT NULL,
    TipoMime         NVARCHAR(50)   NOT NULL,   -- image/png ou image/jpeg
    TamanhoBytes     INT            NOT NULL,
    Dados            VARBINARY(MAX) NOT NULL,
    DataUtc          DATETIME2      NOT NULL,
    EquipeRmLider    NVARCHAR(30)   NOT NULL,
    Executor         NVARCHAR(100)  NOT NULL,   -- NomeAluno_RM
    CONSTRAINT PK_ImagensEstabelecimento PRIMARY KEY (Id),
    CONSTRAINT FK_Imagens_Estabelecimentos FOREIGN KEY (EstabelecimentoId) REFERENCES dbo.Estabelecimentos (Id) ON DELETE CASCADE
);
CREATE INDEX IX_ImagensEstabelecimento_EstabelecimentoId ON dbo.ImagensEstabelecimento (EstabelecimentoId);
GO

/* ---- 5. ReservasAtendimento (R6): escrita 3 da transação "encaminhar com reserva" (1: UPDATE Parceiros.VagasAtendimento; 2: INSERT Encaminhamentos) ---- */
CREATE TABLE dbo.ReservasAtendimento (
    Id               INT           IDENTITY(1,1) NOT NULL,
    EncaminhamentoId INT           NOT NULL,
    DiagnosticoId    INT           NOT NULL,
    ParceiroId       INT           NOT NULL,
    EstabelecimentoId INT          NOT NULL,
    Quantidade       INT           NOT NULL,
    DataUtc          DATETIME2     NOT NULL,
    EquipeRmLider    NVARCHAR(30)  NOT NULL,
    Executor         NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_ReservasAtendimento PRIMARY KEY (Id),
    CONSTRAINT CK_Reservas_Quantidade CHECK (Quantidade > 0),
    CONSTRAINT FK_Reservas_Encaminhamentos FOREIGN KEY (EncaminhamentoId) REFERENCES dbo.Encaminhamentos (Id),
    CONSTRAINT FK_Reservas_Diagnosticos FOREIGN KEY (DiagnosticoId) REFERENCES dbo.Diagnosticos (Id),
    CONSTRAINT FK_Reservas_Parceiros FOREIGN KEY (ParceiroId) REFERENCES dbo.Parceiros (Id),
    CONSTRAINT FK_Reservas_Estabelecimentos FOREIGN KEY (EstabelecimentoId) REFERENCES dbo.Estabelecimentos (Id)
);
CREATE INDEX IX_Reservas_EncaminhamentoId ON dbo.ReservasAtendimento (EncaminhamentoId);
CREATE INDEX IX_Reservas_DiagnosticoId ON dbo.ReservasAtendimento (DiagnosticoId);
CREATE INDEX IX_Reservas_ParceiroId ON dbo.ReservasAtendimento (ParceiroId);
CREATE INDEX IX_Reservas_EstabelecimentoId ON dbo.ReservasAtendimento (EstabelecimentoId);
GO

/* ---- Previsões do modelo de ML (preenchida por POST /api/risco/importar; 180.540 linhas) ---- */
CREATE TABLE dbo.PrevisoesRisco (
    Id                INT          IDENTITY(1,1) NOT NULL,
    CodigoInep        BIGINT       NOT NULL,
    Regiao            NVARCHAR(20) NOT NULL,
    Uf                NVARCHAR(2)  NOT NULL,
    CodigoMunicipio   INT          NOT NULL,
    TpDependencia     INT          NOT NULL,
    ProbSemRampa      FLOAT        NOT NULL,
    SemRampaDeclarado BIT          NOT NULL,
    Faixa             INT          NOT NULL,   -- 1 Baixa, 2 Media, 3 Alta
    CONSTRAINT PK_PrevisoesRisco PRIMARY KEY (Id)
);
CREATE UNIQUE INDEX IX_PrevisoesRisco_CodigoInep ON dbo.PrevisoesRisco (CodigoInep);
CREATE INDEX IX_PrevisoesRisco_Uf ON dbo.PrevisoesRisco (Uf);
GO

PRINT 'Estrutura criada em CitizenTechDb. Próximo passo: 02_Carga_Ficticia.sql';
GO
