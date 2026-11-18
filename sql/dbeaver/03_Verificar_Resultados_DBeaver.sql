/* VERSÃO PARA DBEAVER: sem GO. Execute com Alt+X (script inteiro). */
/* =====================================================================
   CitizenTech (Acessibilidade) - 03_Verificar_Resultados.sql
   Consultas de verificação (somente leitura). Use ANTES e DEPOIS de cada chamada da API.
   Troque o ID da imagem (seção B) e o ID do parceiro (seção C) pelos IDs reais dos seus testes.
   ===================================================================== */
USE CitizenTechDb;
/* ---------- A. Estrutura física (E03): tabelas, colunas e tipos ---------- */
SELECT t.name AS Tabela, c.name AS Coluna, ty.name AS Tipo, c.max_length, c.is_nullable
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE t.name IN ('Estabelecimentos','Avaliacoes','Diagnosticos','Parceiros','Encaminhamentos','ImagensEstabelecimento','ReservasAtendimento')
ORDER BY t.name, c.column_id;

SELECT fk.name AS ChaveEstrangeira, OBJECT_NAME(fk.parent_object_id) AS TabelaFilha, OBJECT_NAME(fk.referenced_object_id) AS TabelaPai
FROM sys.foreign_keys fk ORDER BY TabelaFilha;
/* ---------- B. Imagem: bytes no banco (E04, T01/T02) ---------- */
SELECT Id, EstabelecimentoId, Titulo, NomeArquivo, TipoMime,
       TamanhoBytes, DATALENGTH(Dados) AS BytesGravados,             -- DATALENGTH deve ser positivo e igual ao tamanho original
       CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', Dados), 2) AS Sha256,
       DataUtc, EquipeRmLider, Executor
FROM dbo.ImagensEstabelecimento WHERE Id = 1 /* <== ID da imagem */;

SELECT COUNT(*) AS TotalImagens FROM dbo.ImagensEstabelecimento;      -- não deve mudar após as rejeições T03-T05
/* ---------- C. Transação: estado do saldo e das reservas (E06/E07, T07/T08) ---------- */
SELECT Id AS ParceiroId, Nome, VagasAtendimento AS SaldoAtual FROM dbo.Parceiros WHERE Id = 1 /* <== ID do parceiro */;
SELECT COUNT(*) AS ReservasDoParceiro, ISNULL(SUM(Quantidade), 0) AS VagasReservadas
FROM dbo.ReservasAtendimento WHERE ParceiroId = 1 /* <== ID do parceiro */;
SELECT Id, EncaminhamentoId, DiagnosticoId, ParceiroId, EstabelecimentoId, Quantidade, DataUtc, EquipeRmLider, Executor
FROM dbo.ReservasAtendimento WHERE ParceiroId = 1 /* <== ID do parceiro */ ORDER BY Id;

/* Consistência: saldo atual + vagas reservadas = saldo inicial da carga (parceiro 1 = 20, parceiro 5 = 15). Deve voltar ao saldo da carga após COMMIT e após ROLLBACK (T07 usa o parceiro 1; T08 usa o parceiro 5). */
SELECT p.VagasAtendimento + ISNULL((SELECT SUM(r.Quantidade) FROM dbo.ReservasAtendimento r WHERE r.ParceiroId = p.Id), 0) AS SaldoInicialReconstituido
FROM dbo.Parceiros p WHERE p.Id = 1 /* <== ID do parceiro */;
/* ---------- E. Fluxo completo: estabelecimento -> avaliação -> diagnóstico -> encaminhamento -> reserva (E06/E07) ---------- */
SELECT e.Nome AS Estabelecimento, d.Id AS DiagnosticoId, d.NivelRisco, d.Resumo,
       en.Id AS EncaminhamentoId, en.Status, p.Nome AS Parceiro, r.Id AS ReservaId, r.Quantidade, r.Executor
FROM dbo.Diagnosticos d
JOIN dbo.Avaliacoes a ON a.Id = d.AvaliacaoId
JOIN dbo.Estabelecimentos e ON e.Id = a.EstabelecimentoId
LEFT JOIN dbo.Encaminhamentos en ON en.DiagnosticoId = d.Id
LEFT JOIN dbo.Parceiros p ON p.Id = en.ParceiroId
LEFT JOIN dbo.ReservasAtendimento r ON r.EncaminhamentoId = en.Id
ORDER BY d.Id, en.Id;

SELECT (SELECT COUNT(*) FROM dbo.Encaminhamentos) AS TotalEncaminhamentos,
       (SELECT COUNT(*) FROM dbo.ReservasAtendimento) AS TotalReservas;   -- iguais: cada COMMIT cria 1 encaminhamento + 1 reserva; ROLLBACK não cria nenhum
/* ---------- D. Integridade: nada de saldo negativo nem reserva órfã ---------- */
SELECT COUNT(*) AS ParceirosComSaldoNegativo FROM dbo.Parceiros WHERE VagasAtendimento < 0;       -- 0
SELECT COUNT(*) AS ReservasSemEncaminhamento FROM dbo.ReservasAtendimento r
LEFT JOIN dbo.Encaminhamentos en ON en.Id = r.EncaminhamentoId WHERE en.Id IS NULL;                -- 0
