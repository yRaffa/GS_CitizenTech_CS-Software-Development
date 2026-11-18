# Índice de evidências — CitizenTech (C#)

Mapa **requisito → teste → evidência → arquivo**. Todas as capturas abaixo são **reais** (execução da equipe em 16–18/11/2026). Onde não há captura, a situação está **pendente**; nada foi simulado. Executor das capturas: RafaelOliveira_RM554736 (líder RM554736).

| R | T | E | Arquivo | O que comprova | Situação |
|---|---|---|---|---|---|
| R1, R2 | T01 | E01 | `E01_01_Equipe_API.png` | `GET /api/equipe` 200 com a equipe real (CitizenTech, ODS, executor/líder) | anexada |
| R8, R9 | T01 | E02 | `E02_01_Ambiente.png` | Versão do SDK (`dotnet --version` = 10.0.300, alvo net8.0) | anexada |
| R8, R9 | T01 | E02 | `E02_02_SQLServer_Versao.png` | `SELECT @@VERSION`: SQL Server 2025 Express 17.0.1000.7, DBeaver 26.2.1 | anexada |
| R8 | T01 | E02 | `E02_03_API_Iniciada.png` | API iniciada (porta 5000) e conexão com o SQL Server verificada | anexada |
| R3 | T09 | E03 | `E03_01_Modelo_No_Banco.png` | Diagrama ER do banco no DBeaver (estrutura física, PK/FK) | anexada (parcial) |
| R3 | T09 | E03 | captura do `modelo_editavel.drawio` aberto no diagrams.net | Modelo na ferramenta de modelagem | **pendente** |
| R4, R5 | T01 | E04 | `E04_01_Imagem_Upload.png`, `E04_02_Imagem_Upload.png` | POST de imagem → 201, equipe e executor, tamanho 35418 bytes | anexada |
| R4, R5 | T01 | E04 | `E04_03_Bytes_No_Banco.png` | Linha em `ImagensEstabelecimento` com `DATALENGTH(Dados)` e SHA-256 `D8DF1F34…53554D` | anexada |
| R4, R5 | T02 | E05 | `E05_01_Recuperacao_Apos_Reinicio.png` | Imagem recuperada por ID após reiniciar | anexada |
| R4, R5 | T02 | E05 | `E05_02_Conteudo_Swagger.png` | `GET /api/imagens/{id}/conteudo` devolvendo os bytes | anexada |
| R6 | T07 | E06 | `E06_01_Antes.png` | Antes: parceiro 1 com saldo 10; 0 encaminhamentos; 0 reservas | anexada |
| R6 | T07 | E06 | `E06_02_Chamada.png` | `POST /api/reservas` (parceiro 1, quantidade 3) → 201 | anexada |
| R6 | T07 | E06 | `E06_03_Depois.png` | Depois: saldo 7; 1 encaminhamento; 1 reserva (COMMIT) | anexada |
| R6 | T06 (preparo) | E06 | `E06_04_Diagnostico_Origem.png`, `E06_05_Diagnostico_Barreiras.png` | Diagnóstico 1 (risco Alto, 5 barreiras) usado na reserva | anexada |
| R6 | T08 | E07 | `E07_01_Falha.png` | `POST /api/reservas` parceiro 5 com falha simulada → 500 “Transação revertida (ROLLBACK)” 18/11/2026 09:42:04 | anexada |
| R6 | T08 | E07 | `E07_02_Depois.png` | Depois: parceiro 5 continua com saldo 15; contagens inalteradas | anexada |
| R7 | T10 | E08 | `E08_02_API_Parada.png` | App com API parada: “FALHA DE COMUNICAÇÃO” (09:43:47) | anexada |
| R7 | T03–T06 | E08 | validações 400/404/409/413 (log do app ou Swagger) | Rejeições sem efeito indevido | **pendente** |
| R1–R9 | T01–T10 | E09 | `E09_01_Endpoints.png`, `E09_02_Endpoints.png` · `docs/relatorio.docx` §8–10 | Lista de endpoints do Swagger e rastreabilidade | anexada / relatório preenchido |
| R8 | — | E02 | captura de `dotnet build` / `dotnet test` | Build e testes xUnit aprovados | **pendente** |

## Renomeações em relação aos nomes dados pelo aluno
| Nome original | Nome final | Motivo |
|---|---|---|
| E09_01_API_Parada | `E08_02_API_Parada` | indisponibilidade da API é E08 (R7/T10) |
| E00_01 / E00_02_Endpoints | `E09_01` / `E09_02_Endpoints` | categoria E00 não existe no D01 |
| E04_02_Imagem_Upload | mantido | 2ª captura do upload |
| E04_03_Bytes_No_Banco | mantido | — |
| E05_02_Reinicio_Processos | `E05_02_Conteudo_Swagger` | descreve o conteúdo real da captura |
| E02_03_API_Iniciada | mantido | — |
| E03_01_Modelo_No_Banco | mantido | — |
| E06_02_Chamada, E06_03_Depois, E07_01_Falha, E07_02_Depois | mantidos | — |
| E08_01_Diagnostico / E08_02_Diagnostico | `E06_04_Diagnostico_Origem` / `E06_05_Diagnostico_Barreiras` | são o preparo da reserva (E06), não validações (E08) |

## Observação sobre IDs
Nas respostas do upload e da reserva e na linha do banco apareceu `Id = 0`. O script `sql/01` cria as colunas com `IDENTITY(1,1)` e o mapeamento agora declara `ValueGeneratedOnAdd()`. Se o banco foi criado por outro script, rode a consulta do relatório §6 e recrie o banco com `sql/01`; depois repita o upload e a reserva e substitua as capturas.

## Pasta `anteriores_sqlite/`
Capturas da **versão anterior** (SQLite). Não comprovam esta entrega; remover do ZIP.
