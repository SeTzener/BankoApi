# Banko AI Context

## Purpose

This file gives an AI agent a quick overview of where the Banko project code lives, both locally and on GitHub. It is meant to help the agent understand which repositories belong to the Banko ecosystem so it can consider all relevant code when answering questions, making suggestions, or implementing changes.

---

## Database Operations

### Production SQL Server

* Hosted on the GCP VM (`banko-20250101`, external IP from `GOOGLE_CLOUD_IP` in `.env`).
* Runs as an Azure SQL Edge Docker container named `sqlserver`, mapped to host port `1433`.
* Database files persist on the host at `/home/gaetanovf/sql-data` (bind-mounted to `/var/opt/mssql`).
* Container restart policy is `unless-stopped`; app container is `banko-api` (image `setzener/banko-api:<commit-sha>`).
* App database connection config lives in the VM `~/.env` (`DB_USER`, `DB_PASS`, `GOOGLE_CLOUD_IP`). Never write secrets into this file or any repo file.

### Scheduled Jobs (VM crontab)

* `07:00` daily — `~/sql_backup.sh`: `BACKUP DATABASE BankoDb`, zips, emails to the backup mailbox.
* Every `5 min` — `~/sql_watchdog.sh`: checks SQL is reachable; on failure restarts the container, and emails an alert if it is still down on the next run.

### Troubleshooting

* SQL up? `docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$DB_PASS" -C -Q "SELECT name FROM sys.databases"`
* `BankoUser` login exists? `SELECT name FROM sys.sql_logins` (recreate with `CREATE LOGIN BankoUser WITH PASSWORD='...'` if missing, then map to `BankoDb` and `BankoDb_Dev` with `db_owner`).
* Schema changes are managed via EF Core migrations in `BankoApi/Migrations`.
* Restore from a nightly backup: locate the `.bak` in `/home/gaetanovf/sql_backups` (or `~/sql-data/backups`), then `RESTORE DATABASE BankoDb FROM DISK = N'...' WITH REPLACE`.

---

## Local Paths

* Banko Mobile:
  `../Banko`

* Banko API:
  `../BankoApi`

---

## GitHub Repositories

* Banko Mobile:
  https://github.com/setzener/Banko

* Banko API:
  https://github.com/setzener/BankoApi

---

## Repository Rules

### Repository Awareness

* Always consider all Banko-related repositories together.
* Do not assume the current repository contains the full context.
* Review relevant code and architecture across both the Mobile and API repositories when possible.
* Consider cross-repository impacts when proposing or implementing changes.
* Ensure solutions remain consistent with the overall Banko architecture.

### Git Operations

* Never commit changes without explicit approval.
* Never push, merge, rebase, squash, tag, or modify Git history without explicit approval.
* If proposing commits, present the proposed commit structure first and wait for confirmation.

### Commit Quality

* Create clear, human-readable commit histories.
* Use focused commits that represent logical units of work.
* Avoid large commits containing unrelated changes.
* Write commit messages that explain the intent of the change.
* Structure commits so reviewers can understand the evolution of the work by reading the commit history.

### Code Style

#### No Loose Strings

* Avoid hardcoded string literals ("magic strings").
* Use enums, sealed classes, constants, value classes, or other dedicated types whenever appropriate.
* Represent domain concepts with types rather than raw strings.
* Prefer type-safe comparisons over string comparisons.

#### UI Strings (Banko Mobile Only)

* Any text that can be displayed to the user must be stored in `Res.string`.
* UI-facing strings must never be hardcoded in composables, view models, or business logic.

### Feedback and Decision Making

* Do not default to agreement.
* Challenge assumptions when appropriate.
* If a proposed solution has weaknesses, risks, tradeoffs, or better alternatives, explain them.
* Prioritize correctness, maintainability, scalability, and long-term value over validation.
* When asked for an opinion, provide an honest assessment supported by reasoning.

---

## Known Invariants

### Transaction soft-delete must survive refetches

* `TransactionsRepository.UpdateTransactionData` must never modify `isDeleted`.
* GoCardless sometimes returns the same transaction twice (once with `transactionId`, once without, with a different `internalTransactionId`). Users resolve these duplicates by soft-deleting them in the app.
* A refetch that matches an existing transaction must preserve the user's deletion. Resetting `isDeleted = false` on the update path resurrects deleted duplicates on the next sync.
* Guarded by the regression test `StoreTransactions_SoftDeletedTransaction_PreservesDeletionOnRefetch` and the `isDeleted`-preservation assertions in the transactionId and outside-window match tests.

---

## Pull Request Format

When creating a Pull Request description, always use the following structure:

## What

* Change 1
* Change 2
* Change 3

## Why

* Reason for Change 1
* Reason for Change 2
* Reason for Change 3

### PR Rules

* The description must contain exactly two sections: `## What` and `## Why`.
* Both sections must use bullet lists.
* `## What` describes the actual changes introduced by the PR.
* `## Why` explains the motivation, business value, bug fix, technical reason, or user benefit behind each corresponding item in `## What`.
* Every item in `## What` must have a matching item in `## Why` in the same order.
* Focus on intent and impact rather than implementation details.
* Keep each bullet concise and clear.
* Do not include sections such as Summary, Testing, Screenshots, Risks, or Notes unless explicitly requested.
* Write in professional, developer-friendly English.
