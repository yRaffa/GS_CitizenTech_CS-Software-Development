# Dicionário de dados — CitizenTech (SQL Server)

Fonte da estrutura: `sql/01_Criar_Estrutura.sql`. Modelo: `modelo/modelo_editavel.drawio` (editável) e `modelo/modelo.png` (exportação). `NN` = NOT NULL. Tipos enumerados são guardados como `int` (valores no código, `src/CitizenTech.Domain/Enums/Enums.cs`).

O modelo tem **11 tabelas**; as **7 do fluxo principal** (R3, em azul no diagrama) são `Estabelecimentos`, `Avaliacoes`, `Diagnosticos`, `Parceiros`, `Encaminhamentos`, `ImagensEstabelecimento` e `ReservasAtendimento`. As outras 4 (`RespostasItens`, `Medicoes`, `Barreiras`, `PrevisoesRisco`) detalham o diagnóstico e guardam as previsões do modelo de ML.

Fluxo: o estabelecimento é avaliado → o diagnóstico lista as barreiras → a foto da barreira (imagem em bytes) fica ligada ao estabelecimento → o diagnóstico é encaminhado a um parceiro e as vagas são reservadas na mesma transação (baixa em `Parceiros`, linha em `Encaminhamentos`, linha em `ReservasAtendimento`).

## Relacionamentos (todos 1:N)
| Pai (1) | Filha (N) | Chave estrangeira | Exclusão |
|---|---|---|---|
| Estabelecimentos | Avaliacoes | Avaliacoes.EstabelecimentoId → Estabelecimentos.Id | CASCADE |
| Estabelecimentos | ImagensEstabelecimento | ImagensEstabelecimento.EstabelecimentoId → Estabelecimentos.Id | CASCADE |
| Estabelecimentos | ReservasAtendimento | ReservasAtendimento.EstabelecimentoId → Estabelecimentos.Id | NO ACTION |
| Parceiros | ReservasAtendimento | ReservasAtendimento.ParceiroId → Parceiros.Id | NO ACTION |
| Diagnosticos | ReservasAtendimento | ReservasAtendimento.DiagnosticoId → Diagnosticos.Id | NO ACTION |
| Encaminhamentos | ReservasAtendimento | ReservasAtendimento.EncaminhamentoId → Encaminhamentos.Id | NO ACTION |
| Avaliacoes | RespostasItens | RespostasItens.AvaliacaoId → Avaliacoes.Id | CASCADE |
| Avaliacoes | Medicoes | Medicoes.AvaliacaoId → Avaliacoes.Id | CASCADE |
| Avaliacoes | Diagnosticos | Diagnosticos.AvaliacaoId → Avaliacoes.Id | CASCADE |
| Diagnosticos | Barreiras | Barreiras.DiagnosticoId → Diagnosticos.Id | CASCADE |
| Diagnosticos | Encaminhamentos | Encaminhamentos.DiagnosticoId → Diagnosticos.Id | CASCADE |
| Parceiros | Encaminhamentos | Encaminhamentos.ParceiroId → Parceiros.Id | NO ACTION |

## Restrições adicionais
- `Parceiros`: `CK_Parceiros_Vagas` (VagasAtendimento >= 0) — impede saldo negativo mesmo fora da API.
- `ReservasAtendimento`: `CK_Reservas_Quantidade` (Quantidade > 0).
- Índices únicos: `RespostasItens(AvaliacaoId, Item)`, `Medicoes(AvaliacaoId, Tipo)`, `Encaminhamentos(DiagnosticoId, ParceiroId)`, `PrevisoesRisco(CodigoInep)`.
- Modelo lógico × físico: no SQL Server `string` do C# vira `nvarchar(n)`, `DateTime` vira `datetime2`, `bool` vira `bit`, `byte[]` vira `varbinary(max)` e enums viram `int`.

## Estabelecimentos (fluxo principal)
Local avaliado (escola, comércio, saúde...). O endereço é um objeto de valor guardado nas colunas Logradouro..Uf.

| Coluna | Tipo | Nulo | Chave | Observação |
|---|---|---|---|---|
| Id | int identity | NN | PK | |
| Nome | nvarchar(150) | NN |  | |
| Tipo | int | NN |  | |
| CodigoInep | bigint | sim |  | |
| Email | nvarchar(150) | sim |  | |
| DataCadastro | datetime2 | NN |  | |
| Logradouro | nvarchar(200) | NN |  | |
| Numero | nvarchar(20) | sim |  | |
| Bairro | nvarchar(100) | NN |  | |
| Cidade | nvarchar(100) | NN |  | |
| Uf | nvarchar(2) | NN |  | |

## Avaliacoes (fluxo principal)
Questionário respondido por um estabelecimento (1 estabelecimento : N avaliações).

| Coluna | Tipo | Nulo | Chave | Observação |
|---|---|---|---|---|
| Id | int identity | NN | PK | |
| EstabelecimentoId | int | NN | FK → Estabelecimentos(Id) ON DELETE CASCADE | |
| DataAvaliacao | datetime2 | NN |  | |
| Observacoes | nvarchar(500) | sim |  | |

## Parceiros (fluxo principal)
Organização especializada que atende; VagasAtendimento é o saldo baixado pela transação.

| Coluna | Tipo | Nulo | Chave | Observação |
|---|---|---|---|---|
| Id | int identity | NN | PK | |
| Nome | nvarchar(150) | NN |  | |
| Descricao | nvarchar(500) | NN |  | |
| Especialidades | int (flags) | NN |  | |
| UfsAtendidas | nvarchar(200) | NN |  | |
| Email | nvarchar(150) | NN |  | |
| Telefone | nvarchar(30) | sim |  | |
| Ativo | bit | NN |  | |
| VagasAtendimento | int  CHECK >= 0 | NN |  | |

## ImagensEstabelecimento (fluxo principal)
Imagem (foto de barreira) vinculada a um estabelecimento; os bytes ficam em Dados (varbinary(max)).

| Coluna | Tipo | Nulo | Chave | Observação |
|---|---|---|---|---|
| Id | int identity | NN | PK | |
| EstabelecimentoId | int | NN | FK → Estabelecimentos(Id) ON DELETE CASCADE | |
| Titulo | nvarchar(100) | NN |  | |
| NomeArquivo | nvarchar(255) | NN |  | |
| TipoMime | nvarchar(50) | NN |  | |
| TamanhoBytes | int | NN |  | |
| Dados | varbinary(max) | NN |  | |
| DataUtc | datetime2 | NN |  | |
| EquipeRmLider | nvarchar(30) | NN |  | |
| Executor | nvarchar(100) | NN |  | |

## ReservasAtendimento (fluxo principal)
Reserva de vagas de um parceiro para tratar um diagnóstico; é a 3ª escrita da transação "encaminhar com reserva" (1ª: baixa em `Parceiros`; 2ª: linha em `Encaminhamentos`).

| Coluna | Tipo | Nulo | Chave | Observação |
|---|---|---|---|---|
| Id | int identity | NN | PK | |
| EncaminhamentoId | int | NN | FK → Encaminhamentos(Id) NO ACTION | |
| DiagnosticoId | int | NN | FK → Diagnosticos(Id) NO ACTION | |
| ParceiroId | int | NN | FK → Parceiros(Id) NO ACTION | |
| EstabelecimentoId | int | NN | FK → Estabelecimentos(Id) NO ACTION | |
| Quantidade | int  CHECK > 0 | NN |  | |
| DataUtc | datetime2 | NN |  | |
| EquipeRmLider | nvarchar(30) | NN |  | |
| Executor | nvarchar(100) | NN |  | |

## RespostasItens
Respostas Sim/Não/Não sei de cada item do questionário.

| Coluna | Tipo | Nulo | Chave | Observação |
|---|---|---|---|---|
| Id | int identity | NN | PK | |
| AvaliacaoId | int | NN | FK → Avaliacoes(Id) ON DELETE CASCADE | |
| Item | int | NN |  | |
| Valor | int | NN |  | |

## Medicoes
Medições guiadas (inclinação, porta, corredor).

| Coluna | Tipo | Nulo | Chave | Observação |
|---|---|---|---|---|
| Id | int identity | NN | PK | |
| AvaliacaoId | int | NN | FK → Avaliacoes(Id) ON DELETE CASCADE | |
| Tipo | int | NN |  | |
| Valor | float | NN |  | |

## Diagnosticos (fluxo principal)
Triagem gerada a partir de uma avaliação (nível de risco, contexto do ML).

| Coluna | Tipo | Nulo | Chave | Observação |
|---|---|---|---|---|
| Id | int identity | NN | PK | |
| AvaliacaoId | int | NN | FK → Avaliacoes(Id) ON DELETE CASCADE | |
| DataGeracao | datetime2 | NN |  | |
| NivelRisco | int | NN |  | |
| ProbabilidadeSemRampaMl | float | sim |  | |
| FaixaRiscoMl | int | sim |  | |
| ContextoRegional | nvarchar(500) | sim |  | |
| Resumo | nvarchar(500) | NN |  | |

## Barreiras
Barreiras detectadas em um diagnóstico.

| Coluna | Tipo | Nulo | Chave | Observação |
|---|---|---|---|---|
| Id | int identity | NN | PK | |
| DiagnosticoId | int | NN | FK → Diagnosticos(Id) ON DELETE CASCADE | |
| Tipo | int | NN |  | |
| Descricao | nvarchar(300) | NN |  | |
| Gravidade | int | NN |  | |
| EspecialidadeSugerida | int | NN |  | |
| Recomendacao | nvarchar(500) | NN |  | |

## Encaminhamentos (fluxo principal)
Conexão de um diagnóstico com um parceiro (ciclo de status).

| Coluna | Tipo | Nulo | Chave | Observação |
|---|---|---|---|---|
| Id | int identity | NN | PK | |
| DiagnosticoId | int | NN | FK → Diagnosticos(Id) ON DELETE CASCADE | |
| ParceiroId | int | NN | FK → Parceiros(Id) NO ACTION | |
| Status | int | NN |  | |
| Observacao | nvarchar(500) | sim |  | |
| DataCriacao | datetime2 | NN |  | |
| DataAtualizacao | datetime2 | NN |  | |

## PrevisoesRisco
Previsões do modelo de ML por escola (CSV importado pela API).

| Coluna | Tipo | Nulo | Chave | Observação |
|---|---|---|---|---|
| Id | int identity | NN | PK | |
| CodigoInep | bigint  UNIQUE | NN |  | |
| Regiao | nvarchar(20) | NN |  | |
| Uf | nvarchar(2) | NN |  | |
| CodigoMunicipio | int | NN |  | |
| TpDependencia | int | NN |  | |
| ProbSemRampa | float | NN |  | |
| SemRampaDeclarado | bit | NN |  | |
| Faixa | int | NN |  | |
