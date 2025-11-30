# Quickstart: Webtoon/Manga Reader Web Application

**Feature**: 001-webtoon-manga-reader  
**Date**: 2024-11-30  

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git
- A folder with webtoon/manga images organized as:
  ```
  ContentRoot/
  ├── series-slug/
  │   ├── chapter-001/
  │   │   ├── 001.jpg
  │   │   ├── 002.jpg
  │   │   └── ...
  │   └── chapter-002/
  │       └── ...
  └── another-series/
      └── ...
  ```

---

## Quick Setup

### 1. Clone and Navigate

```bash
git clone <repository-url>
cd ultraReader
```

### 2. Configure Content Root

Edit `src/UltraReader/appsettings.json`:

```json
{
  "WebtoonSettings": {
    "ContentRootPath": "C:/Your/Webtoon/Folder"
  }
}
```

### 3. Create Database

```bash
cd src/UltraReader
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

### 5. Access the Application

- **Local**: http://localhost:5000
- **From phone** (same WiFi): http://YOUR_PC_IP:5000

---

## First-Time Setup

1. **Open Admin > Settings** (`/admin/settings`)
2. **Verify** the content root path is correct
3. **Add a Series**:
   - Go to Admin > Series (`/admin/series`)
   - Click "Add Series"
   - Fill in title and slug (e.g., `tower-of-god`)
   - Set cover image path (e.g., `tower-of-god/cover.jpg`)
4. **Add Chapters**:
   - Click on the series → Manage Chapters
   - Add chapter with number (e.g., 1)
5. **Add Pages**:
   - Click on chapter → Manage Pages
   - Use "Import from Folder" with relative path (e.g., `tower-of-god/chapter-001`)
6. **Start Reading**:
   - Go to Library (`/library`)
   - Click series → Select chapter → Enjoy!

---

## Configuration Reference

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=App_Data/webtoon.db"
  },
  "WebtoonSettings": {
    "ContentRootPath": "C:/Webtoons",
    "DefaultPageSize": 20,
    "MaxImportFiles": 500
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

| Setting | Description | Default |
|---------|-------------|---------|
| `ConnectionStrings:DefaultConnection` | SQLite database file path | `App_Data/webtoon.db` |
| `WebtoonSettings:ContentRootPath` | Root folder for webtoon images | *(required)* |
| `WebtoonSettings:DefaultPageSize` | Items per page in lists | 20 |
| `WebtoonSettings:MaxImportFiles` | Max files to import at once | 500 |
| `Kestrel:Endpoints:Http:Url` | Server listen URL | `http://0.0.0.0:5000` |

---

## Development Commands

### Run in Development Mode

```bash
cd src/UltraReader
dotnet watch run
```

### Create New Migration

```bash
cd src/UltraReader
dotnet ef migrations add <MigrationName>
```

### Apply Migrations

```bash
dotnet ef database update
```

### Run Tests

```bash
cd tests/UltraReader.Tests
dotnet test
```

### Build for Production

```bash
cd src/UltraReader
dotnet publish -c Release -o ../../publish
```

---

## Accessing from Phone

1. Find your PC's local IP address:
   ```bash
   # Windows
   ipconfig
   # Look for IPv4 Address (e.g., 192.168.1.100)
   ```

2. Ensure Windows Firewall allows port 5000 (or your configured port)

3. On your phone's browser, navigate to:
   ```
   http://192.168.1.100:5000
   ```

---

## Folder Structure Convention

For the best experience, organize your content as:

```
ContentRoot/
├── series-slug/                    # Use URL-friendly names
│   ├── cover.jpg                   # Cover image (optional)
│   ├── chapter-001/                # Chapter folders
│   │   ├── 001.jpg                 # Pages numbered for sorting
│   │   ├── 002.jpg
│   │   └── 003.jpg
│   ├── chapter-002/
│   │   └── ...
│   └── chapter-100/
│       └── ...
└── another-series-slug/
    └── ...
```

**Tips**:
- Use zero-padded numbers (001, 002...) for proper sorting
- Supported image formats: `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`
- Slug should match folder name for easy path entry

---

## Troubleshooting

### Images Not Loading

1. Check content root path in settings
2. Verify the relative path in Page is correct
3. Check browser console for 404 errors
4. Ensure image files exist in the specified location

### Database Errors

1. Ensure `App_Data` folder exists
2. Run `dotnet ef database update` to apply migrations
3. Check file permissions on database file

### Cannot Access from Phone

1. Check PC and phone are on same WiFi network
2. Verify Windows Firewall rule for the port
3. Try using PC's IP address instead of hostname
4. Check if app is bound to `0.0.0.0` not `localhost`

### Slow Loading with Many Images

1. Ensure images are reasonably sized (web-optimized)
2. Check LAN connection speed
3. Consider reducing image quality/size externally

---

## Next Steps

After basic setup:

1. **Add more series** from Admin
2. **Import existing folders** using bulk import
3. **Organize with tags** for easy filtering
4. **Mark favorites** for quick access
5. **Use dashboard** for continue reading

---

## Useful URLs

| URL | Description |
|-----|-------------|
| `/` | Dashboard (continue reading) |
| `/library` | All series |
| `/series/{slug}` | Series detail with chapters |
| `/reader/{slug}/{number}` | Read a chapter |
| `/admin/series` | Manage series |
| `/admin/settings` | App settings |
