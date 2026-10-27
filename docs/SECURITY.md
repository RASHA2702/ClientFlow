# Security & Threat Mitigation Specification

## 1. Input Validation & Defense-in-Depth
- All incoming requests undergo strict validation at both the API controller boundary and the Domain entity constructor.
- Text inputs are trimmed, sanitized, and bounds-checked.
- Email formats are validated against RFC standards.
- Numeric constraints prevent negative budgets, negative hourly rates, or impossible time entries (>24 hours/day).

## 2. SQL Injection Prevention
- All database operations utilize parameterized commands (`SqlCommand.Parameters.AddWithValue` or EF Core parameterized LINQ).
- Dynamic SQL string concatenation is strictly prohibited across the codebase.

## 3. Cross-Origin Resource Sharing (CORS)
- CORS policy `ClientFlowPolicy` is configured to whitelist specific allowed client origins (`http://localhost:4200`, `http://127.0.0.1:4200`).
- Sensitive correlation headers (`X-Correlation-ID`) are explicitly exposed.

## 4. Sensitive Configuration & Secret Hygiene
- Zero secrets, API keys, or production passwords exist in source control.
- `appsettings.json` contains only local development templates and placeholders.
- Cloud credentials must be injected via environment variables or Azure Key Vault in production.

## 5. Audit Logging
- Every destructive or financial mutation (client deletion, task transition, invoice dispatch, settlement) produces an immutable `AuditLog` entry recording:
  - Entity name and identifier
  - Action performed
  - Acting user identifier
  - UTC timestamp
  - Modification summary
