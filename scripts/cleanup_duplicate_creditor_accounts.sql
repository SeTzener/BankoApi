-- One-time cleanup: merge duplicate CreditorAccount rows before deploying the
-- TransactionsRepository dedup fix.
--
-- The buggy code path created a NEW CreditorAccount row for every sync, keyed only by
-- its Guid default value, so the same Iban could exist many times. This script:
--   1. Repoints every Transaction.CreditorAccountId to the oldest CreditorAccount per Iban.
--   2. Deletes the now-unreferenced duplicate rows.
--
-- Run inside a transaction so the operation is atomic. To audit first, run the SELECT
-- queries below and review the counts before executing the UPDATE/DELETE.

BEGIN TRANSACTION;

-- Preview: duplicates per Iban (more than one row sharing an Iban).
-- SELECT Iban, COUNT(*) AS Duplicates FROM CreditorAccounts GROUP BY Iban HAVING COUNT(*) > 1;

-- Repoint transactions to the keeper row (MIN(Id)) per Iban.
UPDATE t
SET t.CreditorAccountId = keeper.Id
FROM Transactions t
JOIN CreditorAccounts c ON c.Id = t.CreditorAccountId
JOIN (
    SELECT Iban, MIN(Id) AS Id
    FROM CreditorAccounts
    GROUP BY Iban
) keeper ON keeper.Iban = c.Iban
WHERE t.CreditorAccountId <> keeper.Id;

-- Delete duplicate rows that no transaction references.
DELETE c
FROM CreditorAccounts c
LEFT JOIN Transactions t ON t.CreditorAccountId = c.Id
WHERE t.Id IS NULL;

COMMIT TRANSACTION;
