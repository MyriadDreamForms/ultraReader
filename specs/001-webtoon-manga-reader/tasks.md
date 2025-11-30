# Tasks: Webtoon/Manga Reader Web Application

**Input**: Design documents from `/specs/001-webtoon-manga-reader/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: Not explicitly requested - test tasks are omitted. Add tests later if needed.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create .NET 10 Blazor Server project in src/UltraReader/
- [X] T002 [P] Configure appsettings.json with ConnectionStrings, WebtoonSettings, Kestrel endpoints in src/UltraReader/appsettings.json
- [X] T003 [P] Configure appsettings.Development.json in src/UltraReader/appsettings.Development.json
- [X] T004 [P] Add NuGet packages: Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.Design in src/UltraReader/UltraReader.csproj
- [X] T005 [P] Create WebtoonSettings options class in src/UltraReader/Configuration/WebtoonSettings.cs
- [X] T006 [P] Create App_Data directory structure and add to .gitignore
- [X] T007 [P] Create placeholder.png image in src/UltraReader/wwwroot/images/placeholder.png

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T008 Create SeriesStatus enum in src/UltraReader/Entities/SeriesStatus.cs
- [X] T009 [P] Create Series entity class in src/UltraReader/Entities/Series.cs
- [X] T010 [P] Create Chapter entity class in src/UltraReader/Entities/Chapter.cs
- [X] T011 [P] Create Page entity class in src/UltraReader/Entities/Page.cs
- [X] T012 [P] Create ReadingProgress entity class in src/UltraReader/Entities/ReadingProgress.cs
- [X] T013 Create AppDbContext with entity configurations in src/UltraReader/Data/AppDbContext.cs
- [X] T014 Create initial EF Core migration in src/UltraReader/Data/Migrations/ (using EnsureCreated instead)
- [X] T015 Configure Program.cs with DbContext, WebtoonSettings, and static file middleware in src/UltraReader/Program.cs
- [X] T016 [P] Create SortOption enum in src/UltraReader/Services/SortOption.cs
- [X] T017 [P] Create PaginatedResult<T> class in src/UltraReader/Services/PaginatedResult.cs
- [X] T018 [P] Create custom exception classes (NotFoundException, ValidationException) in src/UltraReader/Exceptions/
- [X] T019 [P] Create base CSS styles in src/UltraReader/wwwroot/css/app.css
- [X] T020 [P] Create MainLayout.razor in src/UltraReader/Components/Layout/MainLayout.razor
- [X] T021 [P] Create NavMenu.razor in src/UltraReader/Components/Layout/NavMenu.razor
- [X] T022 Create _Imports.razor with common using statements in src/UltraReader/Pages/_Imports.razor

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Reading a Chapter with Vertical Scroll (Priority: P1) 🎯 MVP

**Goal**: Display all pages of a chapter vertically with mobile-optimized scroll, dark theme, and chapter navigation

**Independent Test**: Open `/reader/{slug}/{number}` and verify all images display vertically with dark background and prev/next buttons

### Implementation for User Story 1

- [X] T023 [P] [US1] Create reader.css with dark theme and mobile-first styles in src/UltraReader/wwwroot/css/reader.css
- [X] T024 [P] [US1] Create ReaderLayout.razor minimal layout in src/UltraReader/Components/Layout/ReaderLayout.razor
- [X] T025 [P] [US1] Create ImagePlaceholder.razor component with onerror fallback in src/UltraReader/Components/ImagePlaceholder.razor
- [X] T026 [P] [US1] Create IImageService interface in src/UltraReader/Services/IImageService.cs
- [X] T027 [US1] Implement ImageService with path validation and URL generation in src/UltraReader/Services/ImageService.cs
- [X] T028 [P] [US1] Create ReaderDataDto, ReaderPageDto records in src/UltraReader/Services/DTOs/ReaderDtos.cs
- [X] T029 [P] [US1] Create IReaderService interface in src/UltraReader/Services/IReaderService.cs
- [X] T030 [US1] Implement ReaderService with GetReaderDataAsync and UpdateReadingProgressAsync in src/UltraReader/Services/ReaderService.cs
- [X] T031 [US1] Create Reader.razor page with vertical scroll layout and chapter navigation in src/UltraReader/Pages/Reader.razor
- [X] T032 [US1] Register IImageService and IReaderService in Program.cs

**Checkpoint**: Reader page works - can view chapters with images, navigate prev/next

---

## Phase 4: User Story 2 - Browsing the Series Library (Priority: P1) 🎯 MVP

**Goal**: Display series grid with covers, search, filter by tag/status, sort options

**Independent Test**: Open `/library` and verify series cards display, search works, filters apply

### Implementation for User Story 2

- [X] T033 [P] [US2] Create SeriesCardDto record in src/UltraReader/Services/DTOs/SeriesDtos.cs
- [X] T034 [P] [US2] Create LibraryFilter record in src/UltraReader/Services/DTOs/SeriesDtos.cs
- [X] T035 [P] [US2] Create ISeriesService interface in src/UltraReader/Services/ISeriesService.cs
- [X] T036 [US2] Implement SeriesService.GetLibraryAsync with filters and pagination in src/UltraReader/Services/SeriesService.cs
- [X] T037 [US2] Implement SeriesService.GetAllTagsAsync in src/UltraReader/Services/SeriesService.cs
- [X] T038 [P] [US2] Create FilterChangedEventArgs record in src/UltraReader/Components/FilterChangedEventArgs.cs
- [X] T039 [P] [US2] Create SearchFilter.razor component in src/UltraReader/Components/SearchFilter.razor
- [X] T040 [P] [US2] Create Pagination.razor component in src/UltraReader/Components/Pagination.razor
- [X] T041 [P] [US2] Create SeriesCard.razor component in src/UltraReader/Components/SeriesCard.razor
- [X] T042 [US2] Create Library.razor page with grid, search, filters, sorting in src/UltraReader/Pages/Library.razor
- [X] T043 [US2] Register ISeriesService in Program.cs

**Checkpoint**: Library page works - can browse, search, filter series

---

## Phase 5: User Story 3 - Viewing Series Details and Chapter List (Priority: P1) 🎯 MVP

**Goal**: Display series info, chapter list with read/unread status, navigation to reader

**Independent Test**: Click series in library, verify detail page shows chapters with read status

### Implementation for User Story 3

- [X] T044 [P] [US3] Create SeriesDetailDto record in src/UltraReader/Services/DTOs/SeriesDtos.cs
- [X] T045 [P] [US3] Create ChapterListItemDto record in src/UltraReader/Services/DTOs/ChapterDtos.cs
- [X] T046 [US3] Implement SeriesService.GetSeriesDetailAsync with chapters and read status in src/UltraReader/Services/SeriesService.cs
- [X] T047 [P] [US3] Create ChapterListItem.razor component with read/unread indicator in src/UltraReader/Components/ChapterListItem.razor
- [X] T048 [US3] Create SeriesDetail.razor page with info and chapter list in src/UltraReader/Pages/SeriesDetail.razor

**Checkpoint**: Series detail works - can see chapters, navigate to reader, see read status

---

## Phase 6: User Story 4 - Tracking Reading Progress (Priority: P2)

**Goal**: Remember last read chapter, update progress on chapter open/complete, manual toggle

**Independent Test**: Read a chapter, return to series page, verify last read is marked

### Implementation for User Story 4

- [X] T049 [P] [US4] Create IChapterService interface with MarkAsReadAsync/MarkAsUnreadAsync in src/UltraReader/Services/IChapterService.cs
- [X] T050 [US4] Implement ChapterService with read status toggle in src/UltraReader/Services/ChapterService.cs
- [X] T051 [US4] Update ReaderService to update ReadingProgress on chapter open in src/UltraReader/Services/ReaderService.cs
- [X] T052 [US4] Add toggle read button to ChapterListItem.razor in src/UltraReader/Components/ChapterListItem.razor
- [X] T053 [US4] Register IChapterService in Program.cs

**Checkpoint**: Reading progress tracking works - can see and toggle read status

---

## Phase 7: User Story 5 - Managing Series (Priority: P2)

**Goal**: CRUD operations for series through web UI

**Independent Test**: Create new series, edit it, delete it - all through UI

### Implementation for User Story 5

- [X] T054 [P] [US5] Create CreateSeriesCommand, UpdateSeriesCommand records in src/UltraReader/Services/DTOs/SeriesDtos.cs
- [X] T055 [US5] Implement SeriesService CRUD: CreateSeriesAsync, UpdateSeriesAsync, DeleteSeriesAsync, ToggleFavoriteAsync in src/UltraReader/Services/SeriesService.cs
- [X] T056 [P] [US5] Create ConfirmDialog.razor component in src/UltraReader/Components/ConfirmDialog.razor
- [X] T057 [US5] Create Admin/SeriesManagement.razor with list and actions in src/UltraReader/Pages/Admin/SeriesManagement.razor
- [X] T058 [US5] Create Admin/SeriesEdit.razor for create/edit form in src/UltraReader/Pages/Admin/SeriesEdit.razor

**Checkpoint**: Series CRUD works - can manage series through admin UI

---

## Phase 8: User Story 6 - Managing Chapters (Priority: P2)

**Goal**: CRUD operations for chapters within a series

**Independent Test**: Add chapter to series, edit number/title, delete chapter

### Implementation for User Story 6

- [X] T059 [P] [US6] Create CreateChapterCommand, UpdateChapterCommand, ChapterEditDto records in src/UltraReader/Services/DTOs/ChapterDtos.cs
- [X] T060 [US6] Implement ChapterService CRUD: GetChaptersAsync, GetChapterForEditAsync, CreateChapterAsync, UpdateChapterAsync, DeleteChapterAsync in src/UltraReader/Services/ChapterService.cs
- [X] T061 [US6] Create Admin/ChapterManagement.razor with list and actions in src/UltraReader/Pages/Admin/ChapterManagement.razor
- [X] T062 [US6] Create Admin/ChapterEdit.razor for create/edit form in src/UltraReader/Pages/Admin/ChapterEdit.razor

**Checkpoint**: Chapter CRUD works - can manage chapters through admin UI

---

## Phase 9: User Story 7 - Managing Pages (Priority: P2)

**Goal**: Add, reorder, delete pages; bulk import from folder

**Independent Test**: Add pages to chapter, reorder them, use folder import

### Implementation for User Story 7

- [X] T063 [P] [US7] Create PageDto, AddPageCommand records in src/UltraReader/Services/DTOs/PageDtos.cs
- [X] T064 [P] [US7] Create IPageService interface in src/UltraReader/Services/IPageService.cs
- [X] T065 [P] [US7] Create IFolderImportService interface in src/UltraReader/Services/IFolderImportService.cs
- [X] T066 [US7] Implement FolderImportService with natural sort in src/UltraReader/Services/FolderImportService.cs
- [X] T067 [US7] Implement PageService: GetPagesAsync, AddPageAsync, UpdatePageOrderAsync, DeletePageAsync, ImportFromFolderAsync in src/UltraReader/Services/PageService.cs
- [X] T068 [US7] Create Admin/PageManagement.razor with list, reorder, import in src/UltraReader/Pages/Admin/PageManagement.razor
- [X] T069 [US7] Register IPageService and IFolderImportService in Program.cs

**Checkpoint**: Page management works - can add, reorder, import pages

---

## Phase 10: User Story 8 - Configuring the Root Content Folder (Priority: P2)

**Goal**: Configure content root path through settings UI, validate existence

**Independent Test**: Open settings, change path, verify validation

### Implementation for User Story 8

- [ ] T070 [P] [US8] Create AppSettingsDto record in src/UltraReader/Services/DTOs/SettingsDtos.cs
- [ ] T071 [P] [US8] Create ISettingsService interface in src/UltraReader/Services/ISettingsService.cs
- [ ] T072 [US8] Implement SettingsService with path validation in src/UltraReader/Services/SettingsService.cs
- [ ] T073 [US8] Create Admin/Settings.razor page in src/UltraReader/Pages/Admin/Settings.razor
- [ ] T074 [US8] Register ISettingsService in Program.cs

**Checkpoint**: Settings page works - can configure and validate content root

---

## Phase 11: User Story 9 - Managing Favorites (Priority: P3)

**Goal**: Mark/unmark series as favorites, filter by favorites

**Independent Test**: Toggle favorite on series, filter library to show only favorites

### Implementation for User Story 9

- [X] T075 [US9] Add favorite toggle button to SeriesCard.razor in src/UltraReader/Components/SeriesCard.razor
- [X] T076 [US9] Add favorite toggle to SeriesDetail.razor in src/UltraReader/Pages/SeriesDetail.razor
- [X] T077 [US9] Add favorites filter option to SearchFilter.razor in src/UltraReader/Components/SearchFilter.razor

**Checkpoint**: Favorites work - can toggle and filter favorites

---

## Phase 12: User Story 10 - Dashboard with Continue Reading and Updates (Priority: P3)

**Goal**: Home dashboard with continue reading section and recently updated

**Independent Test**: Verify dashboard shows recently read series and newly added chapters

### Implementation for User Story 10

- [X] T078 [P] [US10] Create ContinueReadingDto, RecentlyUpdatedDto records in src/UltraReader/Services/DTOs/DashboardDtos.cs
- [X] T079 [P] [US10] Create IDashboardService interface in src/UltraReader/Services/IDashboardService.cs
- [X] T080 [US10] Implement DashboardService with GetContinueReadingAsync and GetRecentlyUpdatedAsync in src/UltraReader/Services/DashboardService.cs
- [X] T081 [US10] Create Index.razor dashboard page in src/UltraReader/Pages/Index.razor
- [X] T082 [US10] Register IDashboardService in Program.cs

**Checkpoint**: Dashboard works - shows continue reading and recent updates

---

## Phase 13: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T083 [P] Add responsive CSS improvements to app.css in src/UltraReader/wwwroot/css/app.css
- [X] T084 [P] Add logging to all services for error tracking
- [X] T085 [P] Add empty state messages for no series, no chapters, no pages
- [X] T086 Validate quickstart.md instructions work end-to-end
- [X] T087 [P] Create README.md with setup instructions at repository root

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-12)**: All depend on Foundational phase completion
  - US1, US2, US3 (P1) form MVP - do first
  - US4-US8 (P2) enhance functionality
  - US9, US10 (P3) nice-to-have features
- **Polish (Phase 13)**: Depends on all desired user stories being complete

### User Story Dependencies

| Story | Priority | Can Start After | Notes |
|-------|----------|-----------------|-------|
| US1 (Reader) | P1 | Phase 2 | Core MVP - independent |
| US2 (Library) | P1 | Phase 2 | Core MVP - independent |
| US3 (Series Detail) | P1 | Phase 2 | Core MVP - uses SeriesService from US2 |
| US4 (Progress) | P2 | US1, US3 | Extends reader and series detail |
| US5 (Series CRUD) | P2 | US2 | Extends SeriesService |
| US6 (Chapter CRUD) | P2 | US5 | Needs series to exist |
| US7 (Page CRUD) | P2 | US6 | Needs chapters to exist |
| US8 (Settings) | P2 | Phase 2 | Independent |
| US9 (Favorites) | P3 | US2 | Extends library filtering |
| US10 (Dashboard) | P3 | US4 | Needs reading progress |

### Parallel Opportunities

Within each phase, tasks marked [P] can run in parallel.

**Phase 2 Parallel Groups**:
```
Group A: T009, T010, T011, T012 (Entities - all parallel)
Group B: T016, T017, T018 (Services infrastructure - all parallel)
Group C: T019, T020, T021 (UI infrastructure - all parallel)
```

**User Story Parallel Example (US2)**:
```
Parallel: T033, T034, T038, T039, T040, T041 (DTOs and components)
Sequential: T035 → T036 → T037 → T042 (Service implementation → Page)
```

---

## Implementation Strategy

### MVP First (User Stories 1, 2, 3)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL)
3. Complete Phase 3: US1 (Reader) → Verify reading works
4. Complete Phase 4: US2 (Library) → Verify browsing works
5. Complete Phase 5: US3 (Series Detail) → Verify navigation works
6. **STOP and VALIDATE**: Full read flow works end-to-end
7. Deploy/demo MVP

### Incremental Delivery

1. **MVP**: Setup + Foundation + US1 + US2 + US3 → Basic reading works
2. **+Progress**: Add US4 → Reading progress tracking
3. **+Admin**: Add US5, US6, US7 → Full content management
4. **+Settings**: Add US8 → Configuration UI
5. **+Polish**: Add US9, US10 → Favorites and dashboard
6. **Final**: Phase 13 polish

---

## Summary

| Phase | Task Count | Purpose |
|-------|------------|---------|
| Phase 1: Setup | 7 | Project initialization |
| Phase 2: Foundational | 15 | Core infrastructure |
| Phase 3: US1 Reader | 10 | Core reading experience |
| Phase 4: US2 Library | 11 | Series browsing |
| Phase 5: US3 Series Detail | 5 | Chapter navigation |
| Phase 6: US4 Progress | 5 | Reading progress |
| Phase 7: US5 Series CRUD | 5 | Series management |
| Phase 8: US6 Chapter CRUD | 4 | Chapter management |
| Phase 9: US7 Page CRUD | 7 | Page management |
| Phase 10: US8 Settings | 5 | Configuration |
| Phase 11: US9 Favorites | 3 | Favorite filtering |
| Phase 12: US10 Dashboard | 5 | Home page |
| Phase 13: Polish | 5 | Final improvements |
| **Total** | **87** | |

**Parallel opportunities**: 42 tasks marked [P]  
**MVP scope**: Phases 1-5 (48 tasks) for functional reading app
