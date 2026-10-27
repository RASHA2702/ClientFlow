# Automated Testing Strategy & Verification Report

## 1. Test Suite Overview
ClientFlow features an automated test suite implemented with **NUnit / MSTest** and **Moq**, verifying business rules, calculation invariants, and service orchestration.

### Test Categories
1. **Domain Invariant Tests (`ClientFlow.Tests/Domain/`):**
   - Verification of client entity constraints, email validation, and status transitions.
   - Project budget, hourly rate boundaries, and milestone completion state changes.
   - Time tracking daily limits (max 24h/entry), billable calculations.
   - Multi-currency invoice calculation: subtotal summation, tax percentage rounding, line totals, and payment settlement protection (cannot cancel or edit paid invoices).
2. **Application Service Tests (`ClientFlow.Tests/Services/`):**
   - Mocked repository interactions using Moq.
   - Verification of duplicate client email detection.
   - Protection against deleting clients with active projects.
   - Verification of audit log entry generation.
3. **Infrastructure & Export Tests (`ClientFlow.Tests/Infrastructure/`):**
   - Verification of CSV generation for time logs and invoices.
   - Special character escaping (commas, quotes, newlines).

---

## 2. Actual Verification Results
Ran on local environment using `.NET 8`:
```text
Test run for c:\Projects\ClientFlow\tests\ClientFlow.Tests\bin\Debug\net8.0\ClientFlow.Tests.dll (.NETCoreApp,Version=v8.0)
A total of 1 test files matched the specified pattern.

Passed!  - Failed: 0, Passed: 20, Skipped: 0, Total: 20, Duration: 633 ms - ClientFlow.Tests.dll (net8.0)
```
- **Total Tests:** 20
- **Passed:** 20 (100%)
- **Failed:** 0
- **Execution Time:** 633 ms

---

## 3. How to Run the Tests
```powershell
dotnet test c:\Projects\ClientFlow\tests\ClientFlow.Tests\ClientFlow.Tests.csproj
```
