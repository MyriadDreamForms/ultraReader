using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using UltraReader.Components;
using UltraReader.Configuration;
using UltraReader.Data;
using UltraReader.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<WebtoonSettings>(
    builder.Configuration.GetSection(WebtoonSettings.SectionName));

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IReaderService, ReaderService>();
builder.Services.AddScoped<ISeriesService, SeriesService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IFolderImportService, FolderImportService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

// Serve static assets from wwwroot
app.MapStaticAssets();

// Serve webtoon content from configurable content root
var webtoonSettings = builder.Configuration
    .GetSection(WebtoonSettings.SectionName)
    .Get<WebtoonSettings>();

if (webtoonSettings != null && !string.IsNullOrWhiteSpace(webtoonSettings.ContentRootPath))
{
    // Göreceli yolu mutlak yola çevir
    var contentPath = Path.IsPathRooted(webtoonSettings.ContentRootPath)
        ? webtoonSettings.ContentRootPath
        : Path.Combine(builder.Environment.ContentRootPath, webtoonSettings.ContentRootPath);

    if (Directory.Exists(contentPath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(contentPath),
            RequestPath = "/content"
        });
    }
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
