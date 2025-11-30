# UltraReader

Kişisel kullanım için yerel bir webtoon/manga okuyucu web uygulaması. Blazor Server ve SQLite ile geliştirilmiştir.

## Özellikler

- 📖 **Dikey Scroll Okuyucu**: Webtoon tarzı kesintisiz okuma deneyimi
- 📚 **Kütüphane Yönetimi**: Serileri arama, filtreleme ve sıralama
- 📊 **İlerleme Takibi**: Son okunan bölüm ve sayfa hatırlatma
- ⭐ **Favoriler**: Favori serileri işaretleme ve filtreleme
- 📂 **Klasörden İçe Aktarma**: Görsel dosyalarını otomatik olarak içe aktarma
- 🌙 **Karanlık Tema**: Okuyucu için göz yormayan karanlık tema
- 📱 **Mobil Uyumlu**: Responsive tasarım ile tüm cihazlarda kullanım

## Gereksinimler

- .NET 10 SDK
- SQLite (otomatik olarak oluşturulur)

## Kurulum

1. Projeyi klonlayın:
```bash
git clone <repo-url>
cd ultraReader
```

2. Bağımlılıkları yükleyin:
```bash
cd src/UltraReader
dotnet restore
```

3. İçerik klasörünü yapılandırın (`appsettings.json`):
```json
{
  "WebtoonSettings": {
    "ContentRootPath": "C:/path/to/your/webtoons"
  }
}
```

4. Uygulamayı çalıştırın:
```bash
dotnet run
```

5. Tarayıcıda açın: `http://localhost:5000`

## İçerik Klasör Yapısı

Webtoon/manga dosyalarınızı şu yapıda organize edin:

```
webtoons/
├── seri-adi/
│   ├── cover.jpg
│   ├── bolum-1/
│   │   ├── 001.jpg
│   │   ├── 002.jpg
│   │   └── ...
│   ├── bolum-2/
│   │   └── ...
│   └── ...
└── diger-seri/
    └── ...
```

## Kullanım

### Seri Ekleme

1. **Yönetim** > **Yeni Seri** sayfasına gidin
2. Seri bilgilerini girin (başlık, slug, açıklama, vb.)
3. Kaydedin

### Bölüm Ekleme

1. Seri yönetimi sayfasında **Bölümler** butonuna tıklayın
2. **Yeni Bölüm** ekleyin
3. Bölüm numarası ve başlık girin

### Sayfa İçe Aktarma

1. Bölüm yönetiminde **Sayfalar** butonuna tıklayın
2. **Klasörden İçe Aktar** butonuna tıklayın
3. İçerik kökü içindeki klasör yolunu girin (örn: `seri-adi/bolum-1`)
4. Görsel dosyaları otomatik olarak doğal sırayla içe aktarılır

## Yapılandırma

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=App_Data/ultrareader.db"
  },
  "WebtoonSettings": {
    "ContentRootPath": "C:/webtoons",
    "DefaultPageSize": 20,
    "MaxImageWidth": 1200
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

## Teknoloji Yığını

- **Frontend**: Blazor Server, Razor Components
- **Backend**: ASP.NET Core 10
- **Veritabanı**: SQLite + Entity Framework Core
- **Stil**: Custom CSS (Dark theme optimized for reading)

## Lisans

Bu proje kişisel kullanım içindir.
