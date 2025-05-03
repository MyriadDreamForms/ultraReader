# ultraReader - Ürün Gereksinim Belgesi (PRD)

## Proje Özeti
UltraReader, kullanıcıların webtoon'ları görüntüleyip okuyabilecekleri bir web uygulamasıdır. Sistem dosya sisteminden webtoon bilgilerini ve görsellerini okuyarak kullanıcıya sunar.

## Hedefler
- Webtoon'ları kullanıcılara kolay ve hızlı bir şekilde sunmak
- Admin ve moderatörlere içerik yönetimi için gerekli araçları sağlamak
- Güvenli ve performanslı bir okuma deneyimi oluşturmak

## Teknik Özellikler
- .NET Core 9 kullanılacak
- Entity Framework Core ile veritabanı işlemleri yapılacak
- Identity ile kullanıcı yönetimi sağlanacak
- MVC mimarisi ile uygulanacak

## Ana Bileşenler

### Kullanıcı Rollerine Göre İşlevler

#### Genel Kullanıcı
- Webtoon listesi görüntüleme
- Webtoon detaylarını görme
- Bölümleri okuma ve navigasyon

#### Admin
- Webtoon ekleme/düzenleme/silme
- Moderatör atama
- Sistem parametrelerini yönetme

#### Moderatör
- Yeni içerikleri onaylama
- Kullanıcı yorumlarını yönetme

### Temel Sayfalar
- Ana Sayfa (Webtoon listesi)
- Webtoon Detay Sayfası
- Bölüm Okuma Sayfası
- Admin Panel
- Moderatör Panel

## Veri Yapısı
- Webtoon'lar dosya sisteminde saklanacak
- Her webtoon kendi klasöründe olacak (`wwwroot/webtoons/[webtoon-adı]`)
- Her webtoon'un `info.json` dosyası temel bilgilerini içerecek
- Bölümler alt klasörlerde tutulacak

## Yapıldı
- Proje klasör yapısının belirlenmesi
- Veri akışı tasarımı
- Temel mimari kararları
- Temel klasör yapısını oluşturma
- Entity modellerini tanımlama
- Controller'ları oluşturma
- Servis katmanını geliştirme
- Dosya sisteminden veri okuma altyapısını kurma
- Bölüm okuyucu sayfasını tasarlama
- Admin ve moderatör panellerini geliştirme
- Kullanıcı yetkilendirme sistemini kurma
- Performans optimizasyonları
- Test senaryolarını yazma ve uygulama
- Konteynerizasyon (Docker kullanımı)
- Redis entegrasyonu (Distributed Cache için)
- Grafana entegrasyonu (Uygulama izleme için)
- PostgreSQL veritabanı desteği
- Identity sayfalarını Türkçeleştirme
- Docker Compose dosyası oluşturma
- Admin panelinde webtoon ve kullanıcı yönetimi sayfaları
- Moderatör panelinde içerik onay sayfası

## Yapılacaklar
- Docker container'larını çalıştırıp PostgreSQL veritabanı bağlantısını test etmek
- Admin panelinde webtoon ekleme/düzenleme formlarını tamamlamak
- Uygulama ayarlarını production için yapılandırmak
- Sayfa stillendirmelerini geliştirmek
- **Webtoon Favorileme:** Kullanıcıların favori webtoonlarını ekleyebilecekleri bir sistem kurulabilir.
- **Okuma Geçmişi:** Kullanıcıların kaldığı yerden devam edebilmeleri için son okunan bölümün kaydedilmesi.
- **Karanlık/Aydınlık Tema:** Kullanıcıların tercihlerine göre tema seçebilmesi görsel yorgunluğu azaltabilir.
- **HTTPS Yönlendirmesi:** Tüm HTTP isteklerinin HTTPS'e yönlendirilmesi.
- **CORS Politikası:** Daha sıkı CORS kuralları ile istenmeyen domainlerden isteklerin engellenmesi.
- **Rate Limiting:** Aşırı istek durumlarına karşı hız sınırlaması uygulanabilir.
- **Web Application Firewall (WAF):** Potansiyel saldırılara karşı koruma sağlayabilir.
- **Detaylı Loglama:** Serilog veya NLog entegrasyonu ile daha detaylı log toplama.
- **Kullanıcı Analitikleri:** Okuma davranışları ve tercihlerine dair analitikler.
- **A/B Testing:** Farklı özellikler için A/B testleri ile kullanıcı deneyimini ölçmek.
- **Kullanıcı Yorumları:** Webtoon ve bölümlere yorum yapabilme.
- **Webtoon Oylama:** Beğenme/beğenmeme veya yıldız sistemi ile kalite değerlendirmesi.
- **Sosyal Medya Entegrasyonu:** Beğenilen içeriklerin paylaşılabilmesi.
- **RESTful API:** Mobil uygulamalar ve diğer platformlar için API geliştirmek.
- **GraphQL:** Daha esnek veri alma imkanı sunar, özellikle mobil uygulamalar için uygundur.
- **Swagger:** API dokümantasyonu için Swagger entegrasyonu yapılması.

## Tavsiyeler ve İleriye Dönük Öneriler

### Gelişmiş Kullanıcı Deneyimi
- **Lazy Loading:** Görüntüler için lazy loading özelliği eklenebilir. Bölüm açıldıktan sonra görsellerin yüklenmesini erteleyip, sitenin daha hızlı açılmasını sağlayabilir, siteye gereksiz yük oluşturmasını engelleyebilirsiniz. Böylece görünüm alanında olmayan görseller ilk sayfa yükleme performansını geliştirir.
- **Kitaplık Görünümü:** Kullanıcıların favori webtoonlarını ve son okuduklarını görebileceği kişiselleştirilmiş bir kitaplık sayfası.
- **Okuma İstatistikleri:** Kullanıcıların ne kadar okuduğu, hangi tür webtoonları tercih ettiği gibi kişisel istatistikler sunan bir panel.

### Güvenlik Önlemleri
- **JWT Yenilemesi:** Token süresi dolan kullanıcılar için otomatik token yenileme mekanizması.
- **İki Faktörlü Doğrulama:** Hassas işlemler ve admin girişleri için ekstra güvenlik katmanı.
- **Güvenlik Denetimi:** Periyodik olarak güvenlik açıklarını tespit etmek için OWASP kurallarına göre otomatik denetim.

### Performans İyileştirmeleri
- **Dinamik Görsel Boyutlandırma:** Kullanıcının cihazına göre görsellerin optimum boyuta indirilmesi.
- **Önbellek Stratejisi:** Hangi verilerin ne kadar süreyle önbellekte tutulacağına dair kapsamlı bir strateji geliştirmek.
- **Redis Cluster:** Yüksek erişilebilirlik için Redis Cluster yapılandırması.

### Ölçeklendirme ve Altyapı
- **Kubernetes ile Otomatik Ölçeklendirme:** Yük artışlarına göre pod sayısını otomatik artırma konfigürasyonu.
- **CI/CD Pipeline:** GitHub Actions veya Azure DevOps ile otomatik dağıtım.
- **Veritabanı Replikasyonu:** Yüksek erişilebilirlik için veritabanı replikasyonu.
- **Mikroservis Mimarisi:** Ölçeklenebilirlik için monolitik yapıdan mikroservis mimarisine geçiş planlaması.

### Veri Yönetimi ve İzleme
- **Prometheus Grafikleri:** Sunucu performansı, istek sayısı, yanıt süreleri gibi ölçümlerin Grafana'da görselleştirilmesi.
- **Loglama Stratejisi:** Farklı log seviyelerinin tanımlanması ve merkezi bir log yönetim sistemi kurulması.
- **Veri Yedekleme Stratejisi:** Veritabanı ve dosya sisteminin otomatik yedeklenmesi için stratejiler.

### Topluluk ve İçerik Özellikleri
- **İçerik Bildirimleri:** Yeni bölüm eklendiğinde kullanıcıları bilgilendirme.
- **Tartışma Forumu:** Her webtoon veya bölüm için ayrı tartışma alanları oluşturma.
- **İçerik Önerileri:** Kullanıcının okuma alışkanlıklarına göre öneriler sunma algoritması.

### Web API Geliştirme
- **API Versiyonlama:** API'nin geriye dönük uyumluluğunu korumak için versiyon yönetimi.
- **Hız Limitleri:** API isteklerini IP veya token bazında sınırlandırma.
- **API Dokümantasyonu:** Geliştiricilerin kolayca adapte olabilmesi için detaylı API dokümantasyonu.
