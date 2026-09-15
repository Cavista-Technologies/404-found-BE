# CT Recruita — Backend
Backend API for CT Recruita, Cavista's recruitment/hiring platform. Handles job roles, candidate applications, the hiring pipeline, and dashboard analytics.
Built with **.NET 9**, **ASP.NET Core Web API**, **Entity Framework Core (Pomelo MySQL)**, and **MediatR**, following a CQRS-oriented layered architecture.
## Architecture
The solution is split into five class libraries plus the Web host:
| Project | Responsibility |
|---|---|
| `Cavista.CTRecruita.Web` | API host — controllers, `Program.cs` startup/DI, EF Core migrations, email templates |
| `Cavista.CTRecruita.Commands` | Write operations (create/update/delete), dispatched via MediatR |
| `Cavista.CTRecruita.Queries` | Read operations, dispatched via MediatR |
| `Cavista.CTRecruita.Data` | EF Core `DbContext`s, entities, and the DB seeder |
| `Cavista.CTRecruita.Services` | Cross-cutting application services |
| `Cavista.CTRecruita.Utilities` | Shared config models, email sender, helpers, extensions, the mediator setup |
Controllers accept a request, dispatch a Command or Query via MediatR, and return the result — controllers themselves stay thin.
### Key infrastructure
- **Database:** MySQL via Pomelo EF Core provider. Two contexts are registered:
 - `ApplicationContext` — read/write, used for commands
 - `ApplicationReadOnlyContext` — `NoTracking`, used for queries
- **Auth:** ASP.NET Core Identity (`AppUser` / `AppRole`) + JWT bearer tokens
- **Background jobs:** Hangfire (MySQL storage), dashboard mounted at `/jobs`
- **Logging:** Serilog, writing to console and a MySQL sink
- **Email:** SMTP-based `EmailService`, with templates in `Web/EmailTemplate/`
- **File uploads:** stored under `wwwroot/uploads/applications`, served via a configurable public path
- **API docs:** Swagger/Swashbuckle, available at the app root when running
## API surface
All routes are prefixed as shown below.
| Route prefix | Controller | Covers |
|---|---|---|
| `/auth` | `AuthController` | Login |
| `/department` | `DepartmentController` | Department creation |
| `/lookups` | `LookupController` | Reference/lookup data (e.g. departments) |
| `/job-roles` | `JobRolesController` | Create/update/archive roles, pipeline, applicants, timeline, stage changes, open roles |
| `/application-form` | `ApplicationFormsController` | Build & publish application forms, public form retrieval, candidate submission |
| `/dashboard` | `AnalyticsController` | Snapshot, time-to-fill, funnel, conversion-by-channel analytics |
Full request/response contracts are documented via Swagger once the app is running.
## Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- MySQL Server (local instance, or update connection strings to point elsewhere)
- A SMTP account for outbound email (only required if you're testing email flows)
## Getting started
1. **Clone and restore**
  ```bash
  git clone https://github.com/Cavista-Technologies/404-found-BE.git
  cd 404-found-BE
  dotnet restore
  ```
2. **Configure local settings**
  Copy `Cavista.CTRecruita.Web/appsettings.Development.json` and fill in your own local values — at minimum:
  - `ConnectionStrings:DefaultConnection` / `ReadOnlyConnection` / `HangfireConn` (MySQL)
  - `JWT:SigningKey`
  - `Smtp:*` (if testing email)
  - `HangfireAuth:Username` / `Password` (protects the `/jobs` dashboard)
> The committed `appsettings.Development.json` currently has real-looking secrets checked in (SMTP credentials, JWT signing key). Treat these as compromised and rotate them — they shouldn't be in source control. Consider moving local secrets to [.NET user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) instead.
3. **Run the API**
  ```bash
  dotnet run --project Cavista.CTRecruita.Web
  ```
  On startup, the app automatically applies pending EF Core migrations and runs the DB seeder — no separate migration step needed for local dev.
4. **Explore the API**
  Swagger UI is available at the app's root URL once running (e.g. `https://localhost:7227/swagger`). All endpoints except `auth/login` and the public application-form routes require a JWT bearer token — grab one via `POST /auth/login`.
## Database migrations
Migrations live in `Cavista.CTRecruita.Web/Migrations`. To add a new one after changing entities in `Cavista.CTRecruita.Data`:
```bash
dotnet ef migrations add <MigrationName> \
 --project Cavista.CTRecruita.Data \
 --startup-project Cavista.CTRecruita.Web
```
Migrations apply automatically on app startup (see `Program.cs`), so no manual `dotnet ef database update` is needed for local dev — just restart the app.
## Deployment
Deployment is automated via GitHub Actions (`.github/workflows/deploy.yml`) on every push to `main`:
1. Restores, builds, and publishes the Web project in Release mode (.NET 8 runner — note the toolchain here targets 8.0.x while the project itself targets `net9.0`; worth double-checking this still builds correctly or update the workflow's SDK pin)
2. Injects production secrets from GitHub Actions secrets into `appsettings.Production.json`
3. Deploys to SmarterASP via Web Deploy (`msdeploy`)
Required repo secrets: `PROD_DEFAULT_CONN`, `PROD_READONLY_CONN`, `PROD_HANGFIRE_CONN`, `PROD_SERILOG_CONN`, `PROD_JWT_SIGNING_KEY`, `PROD_HANGFIRE_USER`, `PROD_HANGFIRE_PASS`, `PROD_ENCRYPT_KEY`, `ASP_SITE_NAME`, `ASP_MSDEPLOY_URL`, `ASP_USER`, `ASP_PASSWORD`.
## Project structure reference
```
Cavista.CTRecruita.Web/
├── Controllers/        # Auth, JobRoles, Application, DashBoardAnalytics, Lookups, Department
├── RequestModels/       # DTOs for incoming requests
├── Filters/             # Custom action/auth filters (incl. Hangfire dashboard auth)
├── Extensions/           # Service registration extensions
├── EmailTemplate/       # Plaintext email templates
├── Migrations/          # EF Core migrations
└── Program.cs           # App startup, DI, middleware pipeline
Cavista.CTRecruita.Commands/   # Write-side handlers (Applications, Auth, Department, Roles)
Cavista.CTRecruita.Queries/    # Read-side handlers (ApplicationQueries, DashboardAnalytics, Departments, JobRoles)
Cavista.CTRecruita.Data/       # DbContexts, Entities, Seeder
Cavista.CTRecruita.Services/   # Application services
Cavista.CTRecruita.Utilities/  # Config models, Emailer, Mediator setup, Helpers, Extensions
```
## Contributing
- Keep controllers thin — business logic belongs in Command/Query handlers.
- New read logic goes through `ApplicationReadOnlyContext`; writes go through `ApplicationContext`.
- Add a migration whenever you change an entity in `Cavista.CTRecruita.Data`.
- Don't commit real secrets — use user-secrets or environment-specific config that's gitignored.
