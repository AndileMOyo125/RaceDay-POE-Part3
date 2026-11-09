# RaceDay – Parts 1, 2 and 3

RaceDay is a South African community running, walking and cycling event-management platform. Organisers publish events, define categories, view entrants and record results. Participants discover events, enrol in a category, manage their profile and review their race history.

## Solution structure

```text
RaceDay-POE-main/
├── docs/                         Part 1 planning, ERD and API plan
├── Part2-API/
│   ├── RaceDay.sln
│   └── RaceDay.Api/               ASP.NET Core 8 REST API + EF Core + Azure Blob uploads
├── Part3-MVC/                     ASP.NET Core 8 MVC frontend (API consumer only)
├── .github/workflows/             CI build and test
├── docker-compose.yml             SQL Server + API + MVC orchestration
└── README.md
```

## Technology

- .NET 8 / ASP.NET Core MVC and Web API
- Entity Framework Core 8 and SQL Server
- Server-side session authentication (the MVC app securely keeps the API session cookie in its own server-side session)
- Azure Blob Storage for event banners and profile images
- Bootstrap 5 and custom responsive RaceDay styling
- Docker / Docker Compose
- GitHub Actions

## Run locally in VS Code

Prerequisites: .NET 8 SDK, SQL Server (Express or LocalDB), Visual Studio Code with the C# Dev Kit extension, and the EF Core CLI tool.

1. Open the repository root folder in VS Code.
2. In `Part2-API/RaceDay.Api/appsettings.json`, set `ConnectionStrings:RaceDayDb` to your SQL Server instance.
3. In terminal 1:
   ```bash
   cd Part2-API
   dotnet restore
   dotnet build
   dotnet test
   dotnet run --project RaceDay.Api --urls http://localhost:5080
   ```
   The API applies EF Core migrations at startup. Swagger is at `http://localhost:5080/swagger`.
4. In terminal 2:
   ```bash
   cd Part3-MVC
   dotnet restore
   dotnet build
   dotnet run --urls http://localhost:5081
   ```
5. Open `http://localhost:5081`. Register an Organiser and a Participant account to test both role-specific experiences.
6. The MVC app calls the Part 2 API for every data operation. It does not connect directly to SQL Server or duplicate API business rules.

### Azure Blob Storage setup

1. Create an Azure Storage Account and a Blob container for event banners (default name `raceday-event-banners`) and profile pictures (default `raceday-profile-images`).
2. Configure the storage account to allow blob public access if you want the returned blob URLs to display directly in the browser. Follow your institution's security guidance for public images.
3. Set the API environment variable `AzureBlobStorage__ConnectionString` to the storage account connection string. Do not commit this secret to GitHub. In PowerShell:
   ```powershell
   $env:AzureBlobStorage__ConnectionString = "<your-storage-connection-string>"
   ```
4. Restart the API. Image uploads are limited to JPG, PNG and WEBP, with a 5 MB maximum. Uploads pass through the Part 2 API; the MVC project never connects to Azure directly.

If Azure is not configured, the rest of the application can still run, but banner/profile-image upload will return a configuration error.

## Docker – one command

Install and start Docker Desktop. From the repository root run:

```bash
docker compose up --build
```

Then open the MVC app at **http://localhost:8080** and Swagger at **http://localhost:5080/swagger**. Docker Compose starts SQL Server, the API and the MVC frontend. It persists SQL Server data in the `raceday-sql-data` volume. The MVC container calls the API by its Compose service name (`http://api:8080`), so the containerised app uses the same API functionality as the local setup.

For Azure image uploads, set `AZURE_BLOB_CONNECTION_STRING` in your shell before running Compose. Change the example SQL Server password before using this outside local development:

```powershell
$env:SQL_SA_PASSWORD = "Use-A-Strong-Local-Password!"
$env:AZURE_BLOB_CONNECTION_STRING = "<your-storage-connection-string>"
docker compose up --build
```

Stop containers with `Ctrl+C`, or run `docker compose down`. Use `docker compose down -v` only if you also want to delete the local SQL Server data volume.

If you only want to run the MVC image while the API is already running on your computer, you can also use the direct Docker commands (Docker Desktop on Windows/macOS provides `host.docker.internal`):

```bash
docker build -t raceday-mvc ./Part3-MVC
docker run --rm -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5080/ raceday-mvc
```

For the full assessment demonstration, prefer `docker compose up --build` because it starts SQL Server and the API as well as the MVC container.

## Features

### Organiser
- Role-specific dashboard with event totals, upcoming-event counts, participant-entry totals, and the three most recently created events.
- Organiser event list with each event's entry count.
- Create, edit and delete owned events.
- Upload an event banner through the API to Azure Blob Storage.
- Add age/distance categories.
- View enrolments and recorded results for owned events.
- Capture finish time and finishing position for entrants who do not yet have a result.

### Participant
- Browse events and view details/categories.
- Enrol in an event under a category; API prevents duplicate entries.
- View personal enrolment history and published results.
- View/update profile details and upload a profile image through the API to Azure Blob Storage.

### Security and validation
- API owns authentication, role checks, event ownership checks, and data validation.
- MVC stores the API session cookie server-side and forwards it to the API; it does not expose the API cookie to browser JavaScript.
- API validates image MIME types and size. Do not commit Azure credentials or production secrets.

## Testing and manual walkthrough

From the repository root, build both applications and run the API test suite:

```bash
dotnet build Part2-API/RaceDay.sln --configuration Release
dotnet test Part2-API/RaceDay.sln --configuration Release
dotnet build Part3-MVC/RaceDay.Mvc.csproj --configuration Release
```

For a manual end-to-end check, start the API and MVC using the local setup above, then:

1. Register and sign in as an Organiser. Create an event and add a category.
2. Register and sign in as a Participant. Find the event, view its categories, and enrol.
3. Sign back in as the Organiser. Open the event's enrolments and record a finish time and position.
4. Sign in as the Participant and confirm the entry and result appear in **My entries** and **My results**.
5. Try a duplicate enrolment and an invalid login; confirm that each produces a useful error.
6. If Azure is configured, upload an event banner and a profile picture. Without Azure configuration, confirm that the upload displays the API's configuration message.

The API tests use an in-memory database. Azure uploads and a complete browser workflow require the corresponding Azure configuration and a running SQL Server/API.

## CI/CD

GitHub Actions restores and builds the API and MVC projects and runs the API xUnit tests on pushes and pull requests. See `.github/workflows/dotnet-ci.yml`.

## Notes

- Part 2 API remains the source of truth for business logic and SQL access.
- The API database uses EF Core migrations for event creation timestamps and nullable `BannerImageUrl` and `ProfileImageUrl` columns. The event timestamp migration backfills existing rows with the database migration time.
- Azure Blob Storage requires a real storage account connection string; no credentials are included in this repository.

## Suggested meaningful Git commit sequence

The assessment brief asks for at least 20 meaningful commits. Use commits as you complete and verify individual pieces of work; do not create empty or misleading commits. For example:

1. Add the Part 3 MVC project structure.
2. Configure MVC services, routing and server-side session.
3. Add the typed API HTTP client.
4. Build the RaceDay shared navigation and layout.
5. Add the responsive RaceDay colour and typography system.
6. Add account registration view.
7. Connect registration to the Part 2 API.
8. Add login and API session-cookie forwarding.
9. Add logout and role-aware navigation.
10. Build the participant event catalogue.
11. Add event search and type filters.
12. Add event details and category display.
13. Add participant event enrolment.
14. Add participant enrolment history.
15. Add participant results history.
16. Add profile editing.
17. Add profile picture upload through the API.
18. Add organiser dashboard and event totals.
19. Add organiser event create/edit/delete pages.
20. Add event category management.
21. Add organiser enrolment list and result capture.
22. Add Azure Blob event banner upload.
23. Add EF Core migrations for image URLs.
24. Add MVC and API Dockerfiles.
25. Add Docker Compose and SQL Server service.
26. Update GitHub Actions to build MVC and test API.
27. Complete README, run verification and record fixes.

Adapt this list to the actual order you work in and make each commit only after that feature is genuinely implemented.
