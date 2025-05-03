# Servis Mimarisi

Bu projede uygulamanın servis katmanı için aşağıdaki yaklaşım kullanılmaktadır:

## Arayüz Yapısı
Tüm servis arayüzleri `Services` klasöründe `I` öneki ile saklanmaktadır:
- IWebtoonService.cs
- IAnalyticsService.cs
- ICommentService.cs
- IPreferencesService.cs
- IUserPreferencesService.cs

## Uygulama Yapısı
Servis arayüzlerinin uygulamaları da aynı klasörde saklanmaktadır:
- WebtoonService.cs
- AnalyticsService.cs 
- CommentService.cs
- PreferencesService.cs
- UserPreferencesService.cs

## Bağımlılık Enjeksiyonu
Servisler Program.cs dosyasında Dependency Injection container'a kaydedilir:

```csharp
// Servis kayıtları
builder.Services.AddScoped<IWebtoonService, WebtoonService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IPreferencesService, PreferencesService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
```

## Kullanım
Controller sınıflarında constructor enjeksiyonu ile servisler kullanılır:

```csharp
public class WebtoonController : Controller
{
    private readonly IWebtoonService _webtoonService;
    
    public WebtoonController(IWebtoonService webtoonService)
    {
        _webtoonService = webtoonService;
    }
    
    // Controller metotları...
}
```

## Servis Katmanı Kuralları
1. Tüm public metotlar async olmalı ve Task dönmeli
2. Servisler veri erişimi, iş mantığı ve önbellek işlemleri için kullanılmalı
3. Controller sınıfları doğrudan veri erişimi yapmamalı, her zaman servisler üzerinden çalışmalı
4. Servisler hata yönetimi ve loglama uygulamalı 