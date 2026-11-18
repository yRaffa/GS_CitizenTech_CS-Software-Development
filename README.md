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
| Nome da CitizenTech | Acessibilidade Urbana |
| Cidadão beneficiado | Pessoas com deficiência ou mobilidade reduzida |
| Problema, em uma frase | Estabelecimentos têm barreiras de acessibilidade e não sabem quais são nem a quem recorrer |
| Como funciona | A pessoa **observa** o local e registra a **foto** da barreira; responde o questionário e informa medições; a plataforma gera a **triagem** (regras + contexto do modelo de ML da disciplina de IA); **encaminha** o diagnóstico a um parceiro especializado, **reservando uma vaga de atendimento** na mesma transação |
| Tecnologias | C# / .NET 8, ASP.NET Core Web API, EF Core 8 + **SQL Server**, Swagger, xUnit, app Windows Forms (HttpClient) |
| Papel de cada parte | **C#:** API, banco SQL Server, imagens, transação e app Windows Forms. **IA & ML:** probabilidade de a escola não ter rampa (CSV importado pela API e usado no diagnóstico). **Demais disciplinas:** [PREENCHER conforme a equipe] |
| Ecossistema / parceiros | Estabelecimentos (escolas, comércio, saúde), parceiros de obras, sinalização, Libras e consultoria técnica (**fictícios**) |
| ODS | ODS 10, ODS 11 e ODS 4 |

> 🚫 **O que não fazemos:** obras, adaptações ou cursos. O diagnóstico é uma **triagem** e **não substitui** a avaliação técnica presencial.

## 📦 Estrutura
```
GS_CitizenTech_CS-Software-Development/
├─ README.md                    este arquivo
├─ CitizenTech.Acessibilidade.sln
├─ src/
│  ├─ CitizenTech.Api/          API (controllers, Swagger, middleware) + equipe.json
│  ├─ CitizenTech.Application/  serviços, DTOs, regras de diagnóstico, validação de imagem, transação de encaminhamento com reserva
│  ├─ CitizenTech.Domain/       entidades, exceções, interfaces
│  ├─ CitizenTech.Infrastructure/ EF Core + SQL Server, configurações, repositórios (LINQ)
│  └─ CitizenTech.Desktop/      app Windows Forms de 6 abas (cliente HttpClient; NÃO acessa o banco)
├─ tests/CitizenTech.Tests/     testes automatizados xUnit (opcional, complementa T01–T10)
├─ data/ml/                     previsoes_escolas_oof.csv (saída da disciplina de IA & ML)
├─ modelo/                      modelo.png · DICIONARIO_DADOS.md
├─ sql/                         aruivos sql de criação do banco
├─ testes/                      api.http
├─ docs/                        relatorio.docx
├─ evidencias/                  indice.md + capturas
└─ amostras/                    imagem_de_teste.jpg · testes_invalidos/
```

## 🚀 Como executar
1. **Banco:** no SSMS, Azure Data Studio ou DBeaver conectado ao SQL Server, execute **na ordem**: `sql/01_Criar_Estrutura.sql` (cria o banco `CitizenTechDb` e as 11 tabelas) → `sql/02_Carga_Ficticia.sql` (3 estabelecimentos e 7 parceiros fictícios; o parceiro 1 começa com **20 vagas**). A API **não** cria tabelas.
   - **No DBeaver** use as versões sem `GO` em `sql/dbeaver/` (`*_DBeaver.sql`) e execute com **Alt+X**; no SSMS/sqlcmd use os originais.
   - LocalDB, se ainda não existir: `sqllocaldb create MSSQLLocalDB` e `sqllocaldb start MSSQLLocalDB`.
2. **Conexão:** o padrão em `src/CitizenTech.Api/appsettings.json` usa **autenticação do Windows** no LocalDB. Para outra instância/credencial, **não grave senha no arquivo**: use `dotnet user-secrets set "ConnectionStrings:CitizenTech" "<string>"` na pasta da API ou a variável de ambiente `ConnectionStrings__CitizenTech`.
3. **Build e testes automatizados**
   ```bash
   dotnet restore
   dotnet build
   dotnet test            # testes xUnit (opcional)
   ```
4. **API:** `dotnet run --project src/CitizenTech.Api` (Swagger em `http://localhost:5080/swagger`). Na inicialização o log informa se a conexão com o SQL Server foi verificada.
5. **App Windows Forms:** com a API ligada: `dotnet run --project src/CitizenTech.Desktop`. As abas seguem o fluxo: 1 API e equipe · 2 Estabelecimentos · 3 Avaliar e diagnosticar · 4 Foto da barreira · 5 Encaminhar com reserva · 6 Risco regional (ML). A URL da API fica em `src/CitizenTech.Desktop/appsettings.json`.
6. **Importar o modelo de ML:** Swagger → `POST /api/risco/importar` (≈1 min para o Brasil; `?uf=SP` é mais rápido).

## 🔌 Endpoints
| Método e rota | Função |
|---|---|
| `GET /api/equipe` | Identificação da equipe (lida de `equipe.json`) |
| `GET /api/estabelecimentos` · `GET /api/parceiros` | Consultas do domínio (filtros e paginação) |
| `POST /api/imagens` (multipart) | Envia imagem PNG/JPEG ≤ 2 MiB → **201** com ID |
| `GET /api/imagens/{id}` · `GET /api/imagens?estabelecimentoId=` | Metadados em JSON |
| `GET /api/imagens/{id}/conteudo` | Bytes com `Content-Type` `image/png` ou `image/jpeg` |
| `POST /api/reservas` · `GET /api/reservas` | **Encaminhar diagnóstico a parceiro com reserva de vaga** (UPDATE saldo + INSERT encaminhamento + INSERT reserva na mesma transação) e consulta das reservas |
| `POST /api/estabelecimentos/{id}/avaliacoes` · `POST /api/avaliacoes/{id}/diagnosticos` · `GET /api/diagnosticos/{id}/parceiros-sugeridos` | Fluxo de avaliação e triagem |
| `GET/POST /api/encaminhamentos` · `PATCH /api/encaminhamentos/{id}/status` | Encaminhamentos e ciclo de vida |
| `GET /api/risco/regioes` · `/ufs` · `/escolas/{inep}` · `POST /api/risco/importar` | Risco regional do modelo de ML |

Códigos: 200/201 sucesso · 400 entrada inválida · 404 inexistente · 409 saldo insuficiente ou encaminhamento repetido · 413 acima de 2 MiB · 500 `Transação revertida (ROLLBACK)` · 503 banco indisponível.

## 🎯 Como atende cada critério
| Critério | Onde ver |
|---|---|
| GS · POO, interfaces, DI | entidades com regras (`Domain/Entities`), interfaces de repositório e serviço, injeção em `Infrastructure/DependencyInjection.cs` |
| GS · Web API e Swagger | `src/CitizenTech.Api/Controllers` + Swagger em `/swagger` |
| GS · EF Core + LINQ | `Infrastructure/Persistencia` (mapeamentos) e `Repositorios` (projeções, `Include`, agrupamentos) |
| GS · async/await | todos os serviços e repositórios usam `async` com `CancellationToken` |
| GS · testes unitários / qualidade | `tests/CitizenTech.Tests` (xUnit, repositórios em memória); camadas `Domain ← Application ← Infrastructure ← Api` |
| D01 · modelo, SQL, imagem, transação, validações, T01–T10, evidências | `modelo/`, `sql/`, `ImagemService`, `ReservaService`, `testes/api.http`, `evidencias/`, `docs/relatorio.docx` |