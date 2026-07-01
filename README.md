# Employee Birthday & Anniversary Portal

## Solution Structure

```
BirthdayPortal/
├── BirthdayPortal.Data/          # EF Core models, DbContext, repositories
│   ├── Models/                   # Entity models
│   ├── Repositories/             # Repository pattern implementation
│   │   └── Interfaces/
│   └── Migrations/               # EF migrations (auto-generated)
├── BirthdayPortal.Services/      # Business logic layer
│   ├── Interfaces/               # Service interfaces
│   └── DTOs/                     # Data transfer objects
├── BirthdayPortal.API/           # ASP.NET Web API project
│   ├── Controllers/              # API controllers
│   └── Filters/                  # Action filters (auth)
└── BirthdayPortal.MVC/           # ASP.NET MVC frontend project
    ├── Controllers/              # MVC controllers
    ├── Views/                    # Razor views
    │   ├── Account/
    │   ├── Dashboard/
    │   ├── Employee/
    │   └── Shared/
    ├── Models/                   # View models
    └── wwwroot/                  # Static assets
        ├── css/
        └── js/
```

## Setup Instructions

1. Update connection string in `appsettings.json` for both API and MVC projects
2. Run `dotnet ef database update` from the API project to apply migrations
3. Run both projects (API on port 5001, MVC on port 5000)

## Technologies

- ASP.NET Core 8 MVC + Web API
- Entity Framework Core 8 (Code First)
- SQL Server
- Bootstrap 5
- jQuery / AJAX
- BCrypt password hashing
- JWT-based session authentication
