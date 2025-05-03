using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ultraReader.Data;
using ultraReader.Services;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Production ortamı ayarları
if (builder.Environment.IsProduction())
{
    builder.WebHost.UseUrls("http://*:5000");
    
    // HTTPS yönlendirme
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = 443;
    });
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Memory Cache servisini ekleme
builder.Services.AddMemoryCache();

// Redis önbelleği konfigürasyonu
if (builder.Configuration.GetValue<bool>("Caching:EnableRedisCache"))
{
    var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
    if (!string.IsNullOrEmpty(redisConnection))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "ultraReader_";
        });
    }
    else
    {
        // Eğer Redis bağlantısı yoksa memory önbelleğe geri dön
        builder.Services.AddDistributedMemoryCache();
    }
}
else
{
    // Redis devre dışı ise memory önbelleği kullan
    builder.Services.AddDistributedMemoryCache();
}

// Veritabanı yapılandırması
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// PostgreSQL için Entity Framework ayarları
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    
    // Production ortamında detailed errors ve sensitive data logging kapatılır
    if (!builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors(false)
               .EnableSensitiveDataLogging(false);
    }
});

// Identity yapılandırması
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = builder.Configuration.GetValue<bool>("Security:RequireEmailConfirmation");
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Lockout ayarları
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("Security:LockoutMinutes"));
    options.Lockout.MaxFailedAccessAttempts = builder.Configuration.GetValue<int>("Security:MaxFailedLoginAttempts");
    options.Lockout.AllowedForNewUsers = builder.Configuration.GetValue<bool>("Security:AutoLockoutEnabled");
})
    .AddRoles<IdentityRole>() // Rol desteğini etkinleştirme
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Servisleri ekle
builder.Services.AddScoped<IWebtoonService, WebtoonService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IPreferencesService, PreferencesService>();

// Uygulama izleme için Health Check ekle
builder.Services.AddHealthChecks()
    .AddRedis(
        builder.Configuration.GetConnectionString("RedisConnection") ?? "localhost:6379",
        name: "redis", 
        tags: new[] { "db", "cache" }
    )
    .AddNpgSql(connectionString, name: "postgres", tags: new[] { "db", "data" });

// Response compression ekleme
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Response caching ekleme
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1MB
    options.UseCaseSensitivePaths = false;
});

// Session ayarları
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Görüntü optimizasyonu için WebOptimizer
// WebOptimizer paketi eksik, ihtiyaç duyulduğunda eklenebilir
//builder.Services.AddWebOptimizer(pipeline =>
//{
//    pipeline.AddJpegOptimizer();
//    pipeline.AddPngOptimizer();
//    pipeline.AddCssMinifier();
//    pipeline.AddJsMinifier();
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Statik dosyalar için caching (CSS, JS, resimler)
        const int durationInSeconds = 60 * 60 * 24 * 7; // 1 hafta
        ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
    }
});

// Response compression aktifleştirme
app.UseResponseCompression();

// Response caching aktifleştirme
app.UseResponseCaching();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Health Checks için endpoint ekleniyor
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=WebtoonList}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity sayfaları için gerekli

// Varsayılan kullanıcıları ve rolleri seed'leme
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Task.Run(async () => {
            await DbInitializer.SeedUsersAndRolesAsync(services);
        }).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı başlatma sırasında bir hata oluştu.");
    }
}

app.Run();
