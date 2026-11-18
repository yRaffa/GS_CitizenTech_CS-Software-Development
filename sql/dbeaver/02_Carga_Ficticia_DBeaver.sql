/* VERSÃO PARA DBEAVER: sem GO. Execute com Alt+X (script inteiro). */
/* =====================================================================
   CitizenTech (Acessibilidade) - 02_Carga_Ficticia.sql
   Carga INICIAL com dados FICTÍCIOS de domínio (nomes marcados "(exemplo)").
   Os dados acadêmicos reais da equipe ficam no equipe.json, não neste script.
   Execute depois do 01. Reexecutável: limpa e recarrega as tabelas usadas.
   ===================================================================== */
USE CitizenTechDb;
DELETE FROM dbo.ReservasAtendimento;
DELETE FROM dbo.ImagensEstabelecimento;
DELETE FROM dbo.Encaminhamentos;
DELETE FROM dbo.Parceiros;
DELETE FROM dbo.Estabelecimentos;   -- (cascata remove avaliações, diagnósticos e barreiras)
DBCC CHECKIDENT ('dbo.ReservasAtendimento', RESEED, 0);
DBCC CHECKIDENT ('dbo.ImagensEstabelecimento', RESEED, 0);
DBCC CHECKIDENT ('dbo.Encaminhamentos', RESEED, 0);
DBCC CHECKIDENT ('dbo.Parceiros', RESEED, 0);
DBCC CHECKIDENT ('dbo.Estabelecimentos', RESEED, 0);
/* ---- Estabelecimentos fictícios (IDs 1, 2 e 3) ---- */
INSERT INTO dbo.Estabelecimentos (Nome, Tipo, CodigoInep, Email, DataCadastro, Logradouro, Numero, Bairro, Cidade, Uf) VALUES
 (N'Escola Municipal Exemplo (fictícia)',     1, NULL, N'contato@exemplo.com.br', SYSUTCDATETIME(), N'Rua das Acácias',        N'100', N'Centro',       N'São Paulo', N'SP'),
 (N'Mercadinho Exemplo (fictício)',           3, NULL, N'contato@exemplo.com.br', SYSUTCDATETIME(), N'Avenida Principal',      N'250', N'Vila Nova',    N'São Paulo', N'SP'),
 (N'Unidade de Saúde Exemplo (fictícia)',     2, NULL, N'contato@exemplo.com.br', SYSUTCDATETIME(), N'Rua do Comércio',        N'45',  N'Jardim Azul',  N'Campinas',  N'SP');
/* ---- Parceiros fictícios. O parceiro 1 começa com 20 vagas (saldo da transação: 20 -> 17 no COMMIT) ---- */
INSERT INTO dbo.Parceiros (Nome, Descricao, Especialidades, UfsAtendidas, Email, Telefone, Ativo, VagasAtendimento) VALUES
 (N'Rampa Certa Obras (exemplo)',          N'Obras de acessibilidade e adequação de rampas.',              3,  N';SP;RJ;MG;ES;', N'contato@exemplo.com.br', N'(11) 0000-0001', 1, 20),
 (N'Norte Acessível Engenharia (exemplo)', N'Obras e consultoria técnica na região Norte.',                3,  N';AM;PA;AC;RO;RR;AP;TO;', N'contato@exemplo.com.br', N'(92) 0000-0002', 1, 10),
 (N'Nordeste Inclusivo (exemplo)',         N'Obras e sinalização no Nordeste.',                             17, N';BA;PE;CE;MA;PB;RN;AL;SE;PI;', N'contato@exemplo.com.br', N'(71) 0000-0003', 1, 10),
 (N'Sul Acessível (exemplo)',              N'Obras de acessibilidade na região Sul.',                       1,  N';PR;SC;RS;', N'contato@exemplo.com.br', N'(41) 0000-0004', 1, 10),
 (N'Inclusão Sinalizada (exemplo)',        N'Sinalização tátil, visual e de vagas.',                        16, N'*', N'contato@exemplo.com.br', N'(11) 0000-0005', 1, 15),
 (N'Libras em Ação (exemplo)',             N'Capacitação de atendimento inclusivo e tecnologia assistiva.', 12, N'*', N'contato@exemplo.com.br', N'(61) 0000-0006', 1, 15),
 (N'Consultoria NBR Brasil (exemplo)',     N'Laudos e consultoria técnica de acessibilidade.',              2,  N'*', N'contato@exemplo.com.br', N'(21) 0000-0007', 1, 15);
SELECT 'Estabelecimentos' AS Tabela, COUNT(*) AS Linhas FROM dbo.Estabelecimentos
UNION ALL SELECT 'Parceiros', COUNT(*) FROM dbo.Parceiros;
