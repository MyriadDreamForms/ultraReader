# Implementation Plan: Webtoon/Manga Reader Web Application

**Branch**: `001-webtoon-manga-reader` | **Date**: 2024-11-30 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-webtoon-manga-reader/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Personal LAN webtoon/manga reading web application with mobile-optimized vertical scroll reader, series/chapter/page CRUD management, and file system based image storage. Built with ASP.NET Core Blazor Server, Entity Framework Core with SQLite for data persistence, serving images via static files middleware from a configurable root content folder.

## Technical Context

**Language/Version**: C# / .NET 10 (ASP.NET Core Blazor Server)  
**Primary Dependencies**: ASP.NET Core Blazor Server, Entity Framework Core 10, Microsoft.EntityFrameworkCore.Sqlite  
**Storage**: SQLite (single file: `App_Data/webtoon.db`) + File system for images (configurable root folder)  
**Testing**: xUnit, bUnit (Blazor component testing), EF Core InMemory/SQLite for integration tests  
**Target Platform**: Windows/Linux self-hosted, accessed via LAN from mobile browsers (Chrome, Safari)  
**Project Type**: Single Blazor Server web application  
**Performance Goals**: Page load <3s, chapter reader loads 50 images <5s over LAN WiFi, smooth vertical scroll on mid-range mobile  
**Constraints**: LAN-only access, single user, responsive mobile-first reader UI, graceful handling of missing image files  
**Scale/Scope**: 500+ series, 100+ chapters per series, thousands of pages, pagination/lazy-loading for large lists

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

> **Note**: Constitution file contains template placeholders. Using reasonable defaults based on .NET best practices.

| Principle | Status | Notes |
|-----------|--------|-------|
| Single Responsibility | ✅ Pass | Single Blazor Server project with clear separation (Data, Entities, Services, Pages, Components) |
| Test-First Approach | ✅ Pass | Plan includes xUnit + bUnit testing strategy |
| Simplicity (YAGNI) | ✅ Pass | No unnecessary abstractions; direct EF Core without repository pattern for single-user app |
| Observability | ✅ Pass | Built-in ASP.NET Core logging; structured error handling for missing files |
| Configuration Management | ✅ Pass | appsettings.json for all configurable values (port, root folder, DB path) |

**Pre-Design Gate**: ✅ PASSED - No violations requiring justification

## Project Structure

### Documentation (this feature)

```text
specs/001-webtoon-manga-reader/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── api-endpoints.md # Internal API/route documentation
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── UltraReader/                    # Main Blazor Server project
│   ├── Program.cs                  # Application entry point, DI configuration
│   ├── appsettings.json            # Configuration (DB path, content root, port)
│   ├── appsettings.Development.json
│   │
│   ├── Data/                       # Data access layer
│   │   ├── AppDbContext.cs         # EF Core DbContext
│   │   └── Migrations/             # EF Core migrations
│   │
│   ├── Entities/                   # EF Core entity classes
│   │   ├── Series.cs
│   │   ├── Chapter.cs
│   │   ├── Page.cs
│   │   ├── ReadingProgress.cs
│   │   ├── Tag.cs
│   │   └── SeriesStatus.cs         # Enum
│   │
│   ├── Services/                   # Business logic services
│   │   ├── SeriesService.cs
│   │   ├── ChapterService.cs
│   │   ├── PageService.cs
│   │   ├── ReaderService.cs        # Reader data + reading progress tracking
│   │   ├── ImageService.cs         # Image path resolution, validation
│   │   └── FolderImportService.cs  # Bulk page import from folder
│   │
│   ├── Pages/                      # Blazor pages (routable)
│   │   ├── Index.razor             # Dashboard / redirect to Library
│   │   ├── Library.razor           # Series grid with search/filter/sort
│   │   ├── SeriesDetail.razor      # Series info + chapter list
│   │   ├── Reader.razor            # Webtoon reader (vertical scroll)
│   │   ├── Admin/
│   │   │   ├── SeriesManagement.razor      # Series CRUD
│   │   │   ├── SeriesEdit.razor            # Series create/edit form
│   │   │   ├── ChapterManagement.razor     # Chapters for a series
│   │   │   ├── ChapterEdit.razor           # Chapter create/edit form
│   │   │   ├── PageManagement.razor        # Pages for a chapter
│   │   │   └── Settings.razor              # App settings (root folder)
│   │   └── _Imports.razor
│   │
│   ├── Components/                 # Reusable Blazor components
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor
│   │   │   ├── NavMenu.razor
│   │   │   └── ReaderLayout.razor  # Minimal layout for reader
│   │   ├── SeriesCard.razor        # Series thumbnail card
│   │   ├── ChapterListItem.razor   # Chapter row with read status
│   │   ├── Pagination.razor        # Reusable pagination component
│   │   ├── SearchFilter.razor      # Search/filter bar
│   │   ├── ConfirmDialog.razor     # Delete confirmation modal
│   │   └── ImagePlaceholder.razor  # Placeholder for missing images
│   │
│   ├── wwwroot/                    # Static assets
│   │   ├── css/
│   │   │   ├── app.css             # Main styles
│   │   │   └── reader.css          # Reader-specific dark theme
│   │   └── images/
│   │       └── placeholder.png     # Missing image placeholder
│   │
│   └── UltraReader.csproj

tests/
├── UltraReader.Tests/              # Test project
│   ├── Unit/
│   │   ├── Services/               # Service unit tests
│   │   └── Entities/               # Entity validation tests
│   ├── Integration/
│   │   └── Data/                   # DbContext integration tests
│   ├── Components/
│   │   └── *.razor.tests           # bUnit component tests
│   └── UltraReader.Tests.csproj

App_Data/                           # Runtime data (gitignored)
└── webtoon.db                      # SQLite database file
```

**Structure Decision**: Single Blazor Server project following ASP.NET Core conventions. Data/Entities/Services folders provide logical separation without over-engineering. No separate API project since Blazor Server handles both UI and data access. Admin pages under `/Admin` subfolder for potential future auth protection.

## Complexity Tracking

> **No violations to justify** - Design follows simplicity principles.

| Decision | Rationale |
|----------|-----------|
| No Repository Pattern | Single-user app; direct DbContext injection in services is sufficient |
| No Separate API Project | Blazor Server handles both UI and data; no need for REST API layer |
| No Authentication | LAN-only, single user; simple admin password can be added later if needed |
| Tags as JSON column | Simpler than many-to-many for free-text tags; sufficient for single-user filtering |

---

## Post-Design Constitution Check

*Re-evaluation after Phase 1 design completion.*

| Principle | Status | Notes |
|-----------|--------|-------|
| Single Responsibility | ✅ Pass | Clear separation: Entities (data), Services (logic), Pages (UI), Components (reusable UI) |
| Test-First Approach | ✅ Pass | Test structure defined: Unit (Services), Integration (DbContext), Component (bUnit) |
| Simplicity (YAGNI) | ✅ Pass | No over-abstraction; direct service-to-DbContext; JSON tags instead of junction table |
| Observability | ✅ Pass | ILogger injection in services; missing file logging; structured error handling |
| Configuration Management | ✅ Pass | Strongly-typed WebtoonSettings; appsettings.json; runtime settings page |

**Post-Design Gate**: ✅ PASSED - Design maintains simplicity while meeting all functional requirements.

---

## Phase 1 Outputs

| Artifact | Status | Path |
|----------|--------|------|
| research.md | ✅ Complete | [research.md](research.md) |
| data-model.md | ✅ Complete | [data-model.md](data-model.md) |
| contracts/api-endpoints.md | ✅ Complete | [contracts/api-endpoints.md](contracts/api-endpoints.md) |
| quickstart.md | ✅ Complete | [quickstart.md](quickstart.md) |
| Agent context updated | ✅ Complete | `.github/agents/copilot-instructions.md` |

---

## Ready for Phase 2

This plan is complete and ready for `/speckit.tasks` to generate implementation tasks.

**Next Command**: `/speckit.tasks` to create `tasks.md` with prioritized implementation steps.
