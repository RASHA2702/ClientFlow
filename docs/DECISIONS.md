# Architectural Decision Records (ADRs)

## ADR-001: Clean Architecture over Monolithic Layering
- **Status:** Accepted
- **Context:** Freelance international clients require software that can evolve over multiple years without turning into untestable spaghetti code.
- **Decision:** Separate the solution into Domain, Application, Infrastructure, and API layers.
- **Consequences:** Provides clear dependency direction inward toward the Domain. Business rules can be 100% unit-tested with NUnit without booting up HTTP servers or physical databases.

---

## ADR-002: Dual Storage Provider with Zero-Friction Local Seeding
- **Status:** Accepted
- **Context:** Reviewers and technical hiring managers often evaluate GitHub projects in tight time constraints and may be on Linux/Mac environments where SQL Server LocalDB is unavailable.
- **Decision:** Provide a thread-safe `InMemoryDataStore` enabled by default with rich enterprise seed data, while maintaining SQL Server compatibility (`System.Data.SqlClient` / EF Core).
- **Consequences:** Reviewers can clone the repo and execute `dotnet run` or `dotnet test` within 5 seconds without installing external database servers, while SQL Server connection strings are configurable via `appsettings.json`.

---

## ADR-003: RFC 7807 ProblemDetails for Unified API Error Reporting
- **Status:** Accepted
- **Context:** REST APIs with ad-hoc or inconsistent error formats cause high integration friction for frontend clients and mobile apps.
- **Decision:** Implement `GlobalExceptionHandlerMiddleware` returning standard RFC 7807/9110 `application/problem+json` payloads containing status codes, title, error details, and an `X-Correlation-ID` header.
- **Consequences:** Frontend applications can reliably parse validation errors and display contextual feedback to end users.

---

## ADR-004: Native CSS Design System over Heavy UI Component Kits
- **Status:** Accepted
- **Context:** Off-the-shelf UI component libraries frequently introduce heavy bundle bloat, dependency deprecations, and generic aesthetic clones.
- **Decision:** Handcraft a responsive, modern SaaS CSS design system using CSS variables, flexbox, and CSS grid tailored specifically for operations workflows.
- **Consequences:** Production bundle size remains ultra-compact (~270 kB total initial chunk) with fast first-contentful paint.
