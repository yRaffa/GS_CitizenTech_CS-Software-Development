# ♿ CitizenTech · Acessibilidade Urbana ♿ 

## 🌐 Global Solution 2026.2 · FIAP 🌐
## 💻 Engenharia de Software · C# Software Development 💻

### 👥 Integrantes
| Nome | RM |
|---|---|
| André Luiz Fernandes de Queiroz | 554503 |
| Marcos Vinicius da Silva Costa | 555490 |
| Paulo Poças | 556080 |
| Rafael Bocchi | 557603 |
| Rafael Federici de Oliveira | 554736 |

## 📋 Ficha da CitizenTech
| Campo | Conteúdo |
|---|---|
| Cidadão beneficiado | Pessoas com deficiência ou mobilidade reduzida |
| Problema em uma frase | Estabelecimentos têm barreiras de acessibilidade e não sabem quais são nem a quem recorrer |
| Como funciona | Questionário e medições guiadas → **triagem** de possíveis barreiras → indicação de parceiros especializados |
| O que **não** fazemos | Obras, adaptações ou cursos; o diagnóstico é triagem e **não substitui** avaliação técnica presencial |
| ODS | 4, 10 e 11 |

## 	📦 O que esta entrega contém
Backend em **C#/.NET 8** com **API REST**, **banco SQLite (EF Core)** e um **app Windows Forms** que usam as **mesmas regras e o mesmo banco**.
Ele se integra à entrega de **IA & ML**: a API importa o CSV de previsões do modelo (`data/ml/previsoes_escolas_oof.csv`) e o diagnóstico passa a informar a probabilidade de a escola não ter rampa e o contexto da UF.

## 🏗️ Arquitetura
```
src/
 ├─ CitizenTech.Domain          entidades (POO/encapsulamento), enums, exceções e interfaces dos repositórios
 ├─ CitizenTech.Application     serviços, DTOs, regras de diagnóstico (herança + polimorfismo), leitor do CSV de ML
 ├─ CitizenTech.Infrastructure  EF Core + SQLite, configurações das tabelas, repositórios (LINQ), injeção de dependência
 ├─ CitizenTech.Api             Web API (controllers, Swagger, tratamento de erros)
 └─ CitizenTech.Desktop         Windows Forms (6 abas), usa os mesmos serviços e o mesmo banco
tests/CitizenTech.Tests         testes unitários xUnit (57 casos)
data/ml/                        CSV gerado pelo notebook de IA & ML
evidencias/                     prints e documentos das evidências
```
Dependências: `Domain ← Application ← Infrastructure ← Api / Desktop`

## 🎯 Como atende cada critério
| Critério | Onde ver |
|---|---|
| **POO** | `Domain/Entities` (construtores validando regras, `private set`, coleções somente leitura, objeto de valor `Endereco`, máquina de estados em `Encaminhamento`); `Application/Regras` (interface `IRegraDiagnostico`, classe base abstrata `RegraDiagnosticoBase`, 3 regras concretas, polimorfismo); interfaces de repositório |
| **Web API** | `Api/Controllers` (REST, verbos e códigos HTTP corretos, `[ApiController]`, DTOs, validação, `ProblemDetails`) + `docs/ENDPOINTS.md` |
| **EF Core + LINQ** | `Infrastructure/Persistencia` (DbContext, `IEntityTypeConfiguration`, owned type, índices, cascata) e `Repositorios` (filtros, paginação, `Include`, `GROUP BY` com `Count/Average`); LINQ também nos serviços (ranking de parceiros) |
| **Async/Await** | Todos os repositórios, serviços e endpoints são assíncronos com `CancellationToken`; importação do CSV com `ReadLineAsync`; app Windows Forms não trava a tela |
| **Qualidade** | Camadas, injeção de dependência, middleware de erros, Swagger, 57 casos de teste |

## 🚀 Como rodar
Pré-requisito: **.NET 8 SDK** (`dotnet --version`). O app Windows Forms só roda no **Windows**. (Com apenas .NET 9/10 instalado, o projeto roda graças ao `RollForward`, mas o ideal é o SDK 8.)
```bash
dotnet restore
dotnet test                                  # Testes Unitários
dotnet run --project src/CitizenTech.Api     # http://localhost:5080/swagger
dotnet run --project src/CitizenTech.Desktop # Aplicação Windows Forms
```
O banco é criado sozinho na primeira execução, com **7 parceiros fictícios** de exemplo (marcados "(exemplo)").
Para carregar o modelo de ML: Swagger → `POST /api/risco/importar` (ou aba **Risco regional (ML)** no app). Leva cerca de 1 minuto para o Brasil inteiro (use `?uf=SP` importar UFs separadamente).

## 🗄️ Onde fica o banco
Por padrão: `%LOCALAPPDATA%\CitizenTech\citizentech.db` (API e app usam **o mesmo arquivo**; a aba **Banco de dados** do app mostra o caminho e tem o botão "Copiar caminho"). Para mudar, edite `Banco:CaminhoSqlite` no `appsettings.json`.
1. DBeaver → **Novo banco de dados** (Ctrl+Shift+N) → **SQLite** → Avançar.
2. Em **Path**, cole o caminho do arquivo → Finalizar.
3. Abra `Tables`: `Estabelecimentos`, `Avaliacoes`, `RespostasItens`, `Medicoes`, `Diagnosticos`, `Barreiras`, `Parceiros`, `Encaminhamentos`, `PrevisoesRisco`.

## 📏 Regras do diagnóstico
Aplicadas por `DiagnosticoService` (cada regra implementa `IRegraDiagnostico`). Os limites são **valores de referência inspirados na NBR 9050**.
- **Itens do questionário** respondidos "Não" geram barreira (rampa, banheiro adaptado, corrimão, piso tátil, sinalização, elevador, vaga, Libras), cada uma com gravidade e especialidade sugerida.
- **Medições fora do limite** (inclinação, porta, corredor) geram barreira.
- **Informação pendente**: itens sem resposta ou "Não sei" geram um alerta de baixa gravidade.
- **Nível de risco**: Alto (2+ barreiras altas ou pontuação ≥ 7), Médio (1 alta ou pontuação ≥ 3), Baixo (demais). Se o modelo de ML indicar faixa **Alta**, o nível sobe um grau.
- **Parceiros sugeridos**: mesma UF + especialidade necessária, ordenados por aderência.

## 🔌 Endpoints

Swagger interativo: `http://localhost:5080/swagger`

Erros seguem o padrão `ProblemDetails`: **400** (validação ou regra de negócio), **404** (não encontrado), **500** (erro inesperado).

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/estabelecimentos?uf=&tipo=&pagina=&tamanhoPagina=` | Lista paginada com filtros |
| GET | `/api/estabelecimentos/{id}` | Detalhe |
| POST | `/api/estabelecimentos` | Cadastra (201) |
| PUT | `/api/estabelecimentos/{id}` | Atualiza |
| DELETE | `/api/estabelecimentos/{id}` | Remove (204) e, em cascata, avaliações e diagnósticos |
| POST | `/api/estabelecimentos/{id}/avaliacoes` | Registra questionário + medições (201) |
| GET | `/api/estabelecimentos/{id}/avaliacoes` | Lista avaliações do estabelecimento |
| GET | `/api/estabelecimentos/{id}/diagnosticos` | Lista diagnósticos do estabelecimento |
| GET | `/api/avaliacoes/{id}` | Detalhe da avaliação |
| POST | `/api/avaliacoes/{id}/diagnosticos` | **Gera a triagem** (201) |
| GET | `/api/diagnosticos/{id}` | Detalhe do diagnóstico |
| GET | `/api/diagnosticos/{id}/parceiros-sugeridos` | Parceiros por UF e especialidade, ordenados por aderência |
| GET | `/api/parceiros?especialidade=&uf=` | Lista parceiros ativos |
| POST | `/api/parceiros` | Cadastra parceiro (201) |
| POST | `/api/encaminhamentos` | Conecta diagnóstico a parceiro (201). Bloqueia duplicidade |
| GET | `/api/encaminhamentos?status=` | Lista (filtro por status) |
| PATCH | `/api/encaminhamentos/{id}/status` | Pendente → Enviado → EmAnalise → Concluido (Cancelado antes do fim) |
| POST | `/api/risco/importar?uf=` | Importa o CSV do modelo de ML (**apaga e recarrega** a tabela) |
| GET | `/api/risco/regioes` | % sem rampa e risco médio por região |
| GET | `/api/risco/ufs?regiao=` | Idem por UF |
| GET | `/api/risco/escolas/{codigoInep}` | Previsão do modelo para uma escola |
