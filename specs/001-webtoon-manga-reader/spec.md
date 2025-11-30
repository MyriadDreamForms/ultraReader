# Feature Specification: Webtoon/Manga Reader Web Application

**Feature Branch**: `001-webtoon-manga-reader`  
**Created**: 2024-11-30  
**Status**: Draft  
**Input**: User description: "Personal LAN webtoon/manga reading web application with vertical scroll reader optimized for mobile, series/chapter/page management, and file system based storage"

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Reading a Chapter with Vertical Scroll (Priority: P1)

As the library owner, I want to read a webtoon chapter on my phone using vertical scroll so that I can enjoy the content in the most natural way for webtoons.

**Why this priority**: This is the core purpose of the application - without a functional reader, the app has no value. The reading experience is the primary user interaction.

**Independent Test**: Can be tested by opening a chapter with pre-loaded images and scrolling through them on a mobile browser. Delivers the core value of reading webtoons.

**Acceptance Scenarios**:

1. **Given** a chapter with 20 page images exists, **When** I open the chapter in the reader, **Then** all images are displayed vertically one after another
2. **Given** I am reading on a mobile phone, **When** I view the reader page, **Then** images are centered, fit the screen width, and have a dark/neutral background
3. **Given** I am at the end of a chapter, **When** I tap "Next Chapter", **Then** I am taken to the next chapter's reader page
4. **Given** I am reading a chapter, **When** I tap "Chapter List", **Then** I return to the series detail page with the chapter list
5. **Given** an image file is missing or corrupted, **When** I open the chapter, **Then** a placeholder with an error message is shown instead of the broken image

---

### User Story 2 - Browsing the Series Library (Priority: P1)

As the library owner, I want to browse my series collection with covers and basic info so that I can quickly find what I want to read.

**Why this priority**: Users need to discover and select content before reading. This is the entry point to all reading activities.

**Independent Test**: Can be tested by loading the library view with a few series entries and verifying covers, titles, and status badges appear correctly.

**Acceptance Scenarios**:

1. **Given** I have 50 series in my library, **When** I open the library page, **Then** I see a grid/list of series with cover images, titles, and reading status
2. **Given** I am on the library page, **When** I search for "Tower", **Then** only series with "Tower" in the title are displayed
3. **Given** series have tags assigned, **When** I filter by "action" tag, **Then** only series tagged with "action" are shown
4. **Given** I want to find series I'm currently reading, **When** I filter by "Reading" status, **Then** only series with "Reading" status appear
5. **Given** I have many series, **When** I sort by "Last Read", **Then** recently read series appear first

---

### User Story 3 - Viewing Series Details and Chapter List (Priority: P1)

As the library owner, I want to view a series' details and its chapters so that I can select which chapter to read.

**Why this priority**: This bridges library browsing and reading - users need to navigate to specific chapters.

**Independent Test**: Can be tested by clicking a series and verifying the detail page shows series info and a navigable chapter list.

**Acceptance Scenarios**:

1. **Given** I click on a series in the library, **When** the series detail page loads, **Then** I see the cover, title, description, tags, status, and chapter list
2. **Given** a series has 100 chapters, **When** I view the chapter list, **Then** chapters are listed with number, optional title, and read/unread status
3. **Given** I have read chapter 45, **When** I view the chapter list, **Then** chapters 1-45 show as "read" and 46+ show as "unread"
4. **Given** I want to start reading, **When** I click on chapter 50, **Then** the reader opens with chapter 50 content
5. **Given** I want to toggle read status, **When** I mark chapter 45 as "unread", **Then** the chapter status updates immediately

---

### User Story 4 - Tracking Reading Progress (Priority: P2)

As the library owner, I want the app to remember my reading progress so that I can easily continue where I left off.

**Why this priority**: Enhances user experience significantly but the app is usable without it. Saves time finding the last read position.

**Independent Test**: Can be tested by reading a chapter, closing the app, reopening, and verifying the last read chapter is remembered.

**Acceptance Scenarios**:

1. **Given** I finish reading chapter 10 of a series, **When** I return to the series detail page, **Then** the last read chapter is marked and visible
2. **Given** I have been reading "Tower of God", **When** I open the app's home/dashboard, **Then** I see "Tower of God" in "Continue Reading" section with a button to continue
3. **Given** I open a chapter and scroll to the end, **When** the chapter ends, **Then** my reading progress is updated automatically
4. **Given** I have unread chapters after my last read, **When** I click "Continue", **Then** I am taken to the first unread chapter

---

### User Story 5 - Managing Series (Priority: P2)

As the library owner, I want to add, edit, and delete series through the web interface so that I can maintain my library without touching files or database directly.

**Why this priority**: Essential for library management but reading existing content is possible without it initially.

**Independent Test**: Can be tested by creating a new series, editing its metadata, and deleting it - all through the UI.

**Acceptance Scenarios**:

1. **Given** I want to add a new series, **When** I fill in title, slug, cover path, tags, and status, **Then** the series is created and appears in the library
2. **Given** I want to change a series title, **When** I edit the series and save, **Then** the new title is reflected everywhere
3. **Given** I want to remove a series, **When** I delete it and confirm, **Then** the series is removed from the library (with clear indication of what happens to chapters/pages)
4. **Given** I enter a duplicate slug, **When** I try to save, **Then** I receive a validation error message
5. **Given** I assign tags to a series, **When** I save and view the series, **Then** the tags are visible and filterable

---

### User Story 6 - Managing Chapters (Priority: P2)

As the library owner, I want to add, edit, and delete chapters for a series so that I can organize my content.

**Why this priority**: Required to build a functional library but series with pre-existing chapters can still be read.

**Independent Test**: Can be tested by adding a chapter to a series, editing its number/title, and deleting it.

**Acceptance Scenarios**:

1. **Given** I am on a series page, **When** I add a new chapter with number and optional title, **Then** the chapter appears in the list sorted by number
2. **Given** a chapter exists, **When** I edit its title, **Then** the change is saved and reflected in the list
3. **Given** I want to renumber chapters, **When** I change chapter 5 to chapter 6, **Then** the ordering updates correctly
4. **Given** I delete a chapter, **When** I confirm deletion, **Then** the chapter and its pages are removed from the system
5. **Given** I add a duplicate chapter number, **When** I save, **Then** I receive a validation error

---

### User Story 7 - Managing Pages (Priority: P2)

As the library owner, I want to add, reorder, and delete pages within chapters so that I can control the reading sequence.

**Why this priority**: Necessary for content management but existing properly-ordered pages can still be read.

**Independent Test**: Can be tested by adding page images to a chapter, reordering them, and verifying the new order in the reader.

**Acceptance Scenarios**:

1. **Given** I am managing a chapter, **When** I add a page with image path and order number, **Then** the page is saved and visible
2. **Given** I want to reorder pages, **When** I change page order or move up/down, **Then** the reading sequence updates
3. **Given** I have a folder with 30 images, **When** I use "Import from folder" feature, **Then** all images are added as pages sorted by filename
4. **Given** I delete a page, **When** I confirm, **Then** the page is removed from the chapter
5. **Given** I add a page with an invalid image path, **When** I save, **Then** the system accepts it but shows a warning

---

### User Story 8 - Configuring the Root Content Folder (Priority: P2)

As the library owner, I want to configure where my webtoon/manga files are stored so that the app knows where to find images.

**Why this priority**: Required configuration before the app can serve any content.

**Independent Test**: Can be tested by setting a root folder path and verifying the app can resolve image paths relative to it.

**Acceptance Scenarios**:

1. **Given** I am setting up the app, **When** I configure the root folder path, **Then** the setting is saved
2. **Given** the root folder is set, **When** I add a page with relative path "SeriesA/ch01/001.jpg", **Then** the app serves the image from the correct location
3. **Given** the root folder path is invalid, **When** I try to save, **Then** I receive an error message

---

### User Story 9 - Managing Favorites (Priority: P3)

As the library owner, I want to mark series as favorites so that I can quickly filter to my preferred content.

**Why this priority**: Nice-to-have feature that improves browsing but doesn't block core functionality.

**Independent Test**: Can be tested by marking a series as favorite and filtering the library to show only favorites.

**Acceptance Scenarios**:

1. **Given** I am on a series page, **When** I click the favorite button, **Then** the series is marked as favorite
2. **Given** I have favorite series, **When** I filter library by "Favorites only", **Then** only favorited series appear
3. **Given** a series is favorited, **When** I unfavorite it, **Then** it no longer appears in favorites filter

---

### User Story 10 - Dashboard with Continue Reading and Updates (Priority: P3)

As the library owner, I want a home dashboard showing my reading progress and recent updates so that I can quickly resume reading or find new content.

**Why this priority**: Enhances convenience but users can navigate directly to the library instead.

**Independent Test**: Can be tested by verifying the dashboard shows recently read series and newly added chapters.

**Acceptance Scenarios**:

1. **Given** I have read 3 series recently, **When** I open the dashboard, **Then** I see those series in "Continue Reading" section
2. **Given** I added new chapters yesterday, **When** I view the dashboard, **Then** I see those series in "Recently Updated" section
3. **Given** I click "Continue" on a series, **When** the reader opens, **Then** it shows the next unread chapter

---

### Edge Cases

- What happens when an image file referenced in the database no longer exists on disk?
  - The reader displays a placeholder image with an error message indicating the file is missing
- What happens when accessing the app from outside the local network?
  - The app should only be accessible from the configured LAN; no special handling required
- What happens when a series has no chapters?
  - The series detail page shows an empty state message with a prompt to add chapters
- What happens when a chapter has no pages?
  - The reader shows an empty state message indicating no pages are available
- What happens when deleting a series that has chapters with pages?
  - Confirmation dialog clearly states that all chapters and pages will be removed
- What happens if two pages have the same order number?
  - The system accepts it with a validation warning; pages with duplicate order numbers are sorted by PageId (creation order) as tiebreaker
- What happens when the root folder setting is changed after content exists?
  - Existing relative paths may break; user is warned before saving
- What happens when importing a folder with non-image files?
  - Only recognized image file extensions are imported; others are skipped silently

---

## Requirements *(mandatory)*

### Functional Requirements

**Library Management**

- **FR-001**: System MUST display a list of all series with cover image, title, and reading status
- **FR-002**: System MUST support filtering series by name search, tag, and reading status
- **FR-003**: System MUST support sorting series by last read date, last updated date, and alphabetically
- **FR-004**: System MUST allow creating new series with title, slug (unique, URL-safe), optional cover image path, optional description, tags, and status
- **FR-005**: System MUST allow editing all series metadata
- **FR-006**: System MUST allow deleting a series with confirmation, removing all associated chapters and pages

**Chapter Management**

- **FR-007**: System MUST display all chapters of a series sorted by chapter number
- **FR-008**: System MUST show read/unread status for each chapter
- **FR-009**: System MUST allow creating new chapters with number and optional title
- **FR-010**: System MUST allow editing chapter metadata including renumbering
- **FR-011**: System MUST allow deleting a chapter with confirmation, removing all associated pages
- **FR-012**: System MUST validate that chapter numbers are unique within a series

**Page Management**

- **FR-013**: System MUST allow adding pages to a chapter with page order and image file path
- **FR-014**: System MUST allow reordering pages within a chapter
- **FR-015**: System MUST allow deleting individual pages
- **FR-016**: System MUST support bulk importing pages from a folder, sorted by filename
- **FR-017**: System MUST store image paths relative to a configurable root folder

**Reader**

- **FR-018**: System MUST display all pages of a chapter in vertical scroll layout
- **FR-019**: System MUST optimize the reader view for mobile devices with images centered and width-fitted
- **FR-020**: System MUST provide a dark/neutral background in the reader
- **FR-021**: System MUST provide navigation to previous/next chapter from the reader
- **FR-022**: System MUST provide navigation back to the chapter list from the reader
- **FR-023**: System MUST display a user-friendly placeholder when an image file is missing

**Reading Progress**

- **FR-024**: System MUST track which chapters have been read per series
- **FR-025**: System MUST remember the last read chapter for each series
- **FR-026**: System MUST update reading progress when a chapter is opened or completed
- **FR-027**: System MUST allow manually marking chapters as read or unread

**Favorites & Tags**

- **FR-028**: System MUST allow marking/unmarking series as favorites
- **FR-029**: System MUST support filtering library to show only favorites
- **FR-030**: System MUST support free-text tags on series
- **FR-031**: System MUST support filtering series by one or more tags

**Configuration**

- **FR-032**: System MUST allow configuring the root content folder path
- **FR-033**: System MUST validate that the configured root folder exists

**Dashboard (Nice-to-have)**

- **FR-034**: System SHOULD display a "Continue Reading" section with recently read series
- **FR-035**: System SHOULD display a "Recently Updated" section with series that have new chapters

**Performance & Scale**

- **FR-036**: System MUST support pagination or lazy loading for library views with hundreds of series
- **FR-037**: System MUST support pagination or lazy loading for chapter lists with hundreds of chapters
- **FR-038**: System MUST load reader pages efficiently using native browser lazy loading (`loading="lazy"`) to handle chapters with 100+ images without blocking initial render

### Key Entities

- **Series**: Represents a manga/webtoon title with metadata (title, original title, slug, description, status, cover image path, favorites flag, tags). Related to many Chapters.

- **Chapter**: Represents a chapter/episode within a series with number and optional title. Belongs to one Series, related to many Pages.

- **Page**: Represents a single image page with order number and relative image path. Belongs to one Chapter.

- **ReadingProgress**: Tracks the user's reading state per series including last read chapter and timestamp.

- **Tag**: Genre or category labels stored as a JSON array column on Series entity (e.g., `["action", "fantasy", "romance"]`). No separate Tag table.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: User can set up the root content folder and start reading within 5 minutes of first launch
- **SC-002**: User can create a new series with 10 chapters (each with 20 pages) in under 15 minutes using the web interface
- **SC-003**: User can find and open any chapter within 3 taps/clicks from the library view
- **SC-004**: Reader page loads and displays a 50-image chapter within 5 seconds on a typical mobile phone over LAN WiFi
- **SC-005**: User can scroll through an entire webtoon chapter smoothly without noticeable lag on a mid-range smartphone
- **SC-006**: All CRUD operations (create, read, update, delete) for series, chapters, and pages are completable through the web interface without manual file or database editing
- **SC-007**: When an image file is missing, the user sees a clear error message instead of a broken page or application crash
- **SC-008**: The application supports a library of 500+ series with 100+ chapters each while maintaining responsive page loads under 3 seconds
- **SC-009**: User can continue reading from where they left off with a single tap from the dashboard
- **SC-010**: The reader interface is usable with one-handed phone operation (vertical scrolling only, no horizontal swipes required for basic reading)

---

## Assumptions

The following assumptions were made based on the feature description:

1. **Single user only**: No authentication complexity is needed; if security is desired, a simple admin password check is sufficient
2. **LAN-only access**: No SSL/HTTPS configuration or internet exposure considerations
3. **Image formats**: Standard web image formats (JPG, PNG, WebP, GIF) will be supported
4. **Folder structure**: Users will maintain a consistent folder structure (Series/Chapter/images) as described
5. **Browser compatibility**: Modern mobile browsers (Chrome, Safari) are the primary target
6. **Reading direction**: Vertical scroll is the default; right-to-left manga reading mode is not in scope
7. **Data persistence**: A local database (SQLite or similar) is acceptable for single-user use
8. **Image serving**: The application will serve images directly from the file system, not copy them to another location
9. **Responsive design**: Management screens prioritize desktop usability while reader prioritizes mobile

---

## Out of Scope

The following items are explicitly excluded from this feature:

- Multi-user support and role-based access control
- Cloud synchronization or backup
- External API integrations (scraping manga sites, metadata services)
- Advanced statistics or reading analytics
- Recommendation algorithms
- Right-to-left reading mode (traditional manga style)
- Horizontal swipe/page-turn reader mode
- Offline mobile app (PWA with offline support)
- Image compression or optimization on upload
- Automated series/chapter detection from folder structure
