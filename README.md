# Global Logistics Management System

ASP.NET Core MVC prototype for client, contract and freight service-request workflows. The application targets .NET 10, uses EF Core with SQL Server, and is container-ready for Azure Container Apps.

## Included

- Client directory and contract register with status and overlapping-date filters.
- Signed agreement uploads restricted to PDF content, stored under unique names outside `wwwroot`.
- Service requests restricted to active contracts inside their date range.
- Live USD-to-ZAR estimate and authoritative server-side conversion when saving a request; the rate is stored with the request.
- xUnit business-rule tests, EF Core migrations, and a Docker build in GitHub Actions.

## Run locally

Install the .NET 10 SDK and SQL Server LocalDB, then run:

```powershell
dotnet tool restore
dotnet ef database update --project GLMS.Web
dotnet run --project GLMS.Web
```

The development connection string in `GLMS.Web/appsettings.json` targets Windows LocalDB. To use another SQL Server locally, set `ConnectionStrings__DefaultConnection` in the environment or use .NET user secrets. Do not commit credentials.

Run tests with `dotnet test GLMS.sln`.

## Azure SQL and Container Apps

Create an Azure SQL logical server and database, and configure a managed identity for the Container App. Grant that identity access in the target database; an Entra administrator can run:

```sql
CREATE USER [<container-app-identity-name>] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [<container-app-identity-name>];
ALTER ROLE db_datawriter ADD MEMBER [<container-app-identity-name>];
```

For first-time schema creation, run the generated migration script using a deployment identity with DDL permissions. Do not grant schema-change permissions to the runtime identity.

Set the Container App environment variable `ConnectionStrings__DefaultConnection` to a managed-identity connection string, replacing the server and database names:

```text
Server=tcp:<server-name>.database.windows.net,1433;Initial Catalog=<database-name>;Encrypt=True;TrustServerCertificate=False;Authentication=Active Directory Managed Identity
```

Container Apps have ephemeral local storage. Create and mount an Azure Files share for agreement PDFs, then set `AgreementStorage__Path` to its mount path (for example `/mnt/glms-agreements`). The app exposes `/health` for a liveness probe and listens on port `8080`.

Build the image from the repository root with `docker build -t glms-web .`, push it to Azure Container Registry, then deploy that image to Container Apps. The image does not contain database credentials or uploaded agreements.

Before exposing this prototype publicly, add Entra ID sign-in and role-based authorization for administrators. This assignment prototype currently has no authentication layer.