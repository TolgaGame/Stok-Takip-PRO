# Stok-Takip-PRO
.NET Practice Project


# Stok Takip PRO

**Tekstil ve hazır giyim ürünleri için geliştirilmiş, C# ve Windows Forms tabanlı masaüstü stok takip uygulaması.**

Stok Takip PRO; ürün bilgilerini düzenlemek, stok durumunu takip etmek ve ürünleri kategorilerine göre filtrelemek için tasarlanmış bir .NET uygulamasıdır. Yerel SQLite veritabanı kullanır ve verileri bilgisayarda saklar.

## Özellikler

- **Ürün yönetimi:** Ürün ekleme, düzenleme ve silme.
- **Tekstil odaklı ürün bilgileri:** Ürün kodu, kategori, beden, kumaş türü, kumaş bileşimi, renk ve tedarikçi gibi alanlar.
- **Stok takibi:** Stok miktarı, minimum stok seviyesi, birim ve stok durumu bilgileri.
- **Fiyat bilgileri:** Alış fiyatı, satış fiyatı ve KDV oranı.
- **Arama ve filtreleme:** Ürünleri metin aramasıyla bulma ve kategoriye göre filtreleme.
- **Kritik stok göstergesi:** Minimum stok seviyesinin altındaki ürünleri görsel olarak belirleme.
- **Stok özeti:** Ürün çeşidi sayısı, kritik stoktaki ürün sayısı ve hesaplanan toplam stok değeri.
- **Yerel veritabanı:** SQLite ile verileri bilgisayarda saklama.
- **Veritabanı migration'ları:** Şema değişikliklerini sürümlü olarak uygulama.
- **Türkçe arayüz:** Türkçe metinler ve `tr-TR` kültürüne uygun sayı biçimlendirmesi.

## Teknolojiler

| Teknoloji | Kullanım amacı |
|---|---|
| C# | Uygulama geliştirme dili |
| .NET 9 | Hedef çalışma platformu |
| Windows Forms | Masaüstü kullanıcı arayüzü |
| SQLite | Yerel veritabanı |
| Microsoft.Data.Sqlite 9.0.0 | SQLite veri erişimi |
| Git ve GitHub | Kaynak kod yönetimi |

## Gereksinimler

- Windows 10 veya Windows 11
- .NET 9 SDK
- Git (kaynak kodu klonlamak için)

.NET SDK kurulumunu [resmî .NET indirme sayfasından](https://dotnet.microsoft.com/download/dotnet/9.0) yapabilirsiniz.

## Kurulum ve çalıştırma

### 1. Depoyu klonlayın

```bash
git clone https://github.com/TolgaGame/Stok-Takip-PRO.git
cd Stok-Takip-PRO
```

### 2. Projeyi derleyin

```bash
dotnet restore
dotnet build
```

### 3. Uygulamayı çalıştırın

```bash
dotnet run --project StokTakipPro.csproj
```

Uygulama ilk açılışta gerekli veritabanı klasörünü ve şemasını oluşturur.

### 4. Yayın sürümü oluşturun

Windows x64 için bağımsız çalıştırılabilir bir yayın oluşturmak üzere:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

Oluşturulan yayın dosyaları, .NET yayınlama çıktısı dizininde bulunur.

## Veritabanı

Uygulama SQLite kullanır. Ayrı bir veritabanı sunucusu kurmanız gerekmez.

Varsayılan veritabanı konumu:

```text
%LocalAppData%\StokTakipPRO\stok.db
```

Veritabanı dosyası uygulamanın yerel kullanıcı verileri dizininde tutulur.

Veritabanı yapısındaki değişiklikler sürümlü migration mekanizmasıyla uygulanır. Mevcut migration'ları değiştirmek yerine yeni şema değişikliklerinin yeni bir migration olarak eklenmesi gerekir.

**Önemli:** Veritabanı dosyasını silmek kayıtlı ürün verilerinin kaybolmasına neden olabilir. Dosyayı silmeden veya şemayı değiştirmeden önce yedek alın.

## Proje yapısı

```text
Stok-Takip-PRO/
├── Data/
│   ├── Database.cs
│   └── ProductRepository.cs
├── Models/
│   ├── Product.cs
│   └── ProductOptions.cs
├── Services/
│   └── AuthService.cs
├── UI/
│   ├── Theme.cs
│   ├── Dialogs.cs
│   ├── LoginForm.cs
│   ├── MainForm.cs
│   ├── ProductEntryPanel.cs
│   └── ProductListPanel.cs
├── Program.cs
├── StokTakipPro.csproj
├── ARCHITECTURE.md
└── README.md
```

### Mimari yaklaşım

Proje, kullanıcı arayüzü, veri erişimi ve model sorumluluklarını ayrı tutan bir yapıyla düzenlenmiştir.

- **Models:** Ürün verilerini, filtreleme mantığını ve sabit seçenek listelerini tanımlar.
- **Data:** SQLite bağlantısını, veritabanı migration'larını ve ürün verilerine erişimi yönetir.
- **Services:** Kimlik doğrulama işlemlerini içerir.
- **UI:** Giriş ekranını, ana pencereyi, ürün formlarını ve listeleme arayüzünü yönetir.
- **Program.cs:** Uygulamanın başlangıç akışını ve temel yapılandırmasını yürütür.

Ayrıntılı mimari açıklamalar ve geliştirme kuralları için [`ARCHITECTURE.md`](ARCHITECTURE.md) dosyasına bakabilirsiniz.

## Kullanım

1. Uygulamayı başlatın ve giriş ekranını açın.
2. Ürün ekleme ekranından gerekli ürün bilgilerini girin.
3. Ürün kodu ve ürün adı gibi zorunlu alanları doldurun.
4. Ürünü kaydedin.
5. Ürün listesinde arama ve kategori filtrelerini kullanın.
6. İlgili ürünü düzenleyin veya silme işlemini onaylayın.
7. Liste ekranındaki stok özetini ve kritik stok göstergelerini inceleyin.

Sayısal alanlarda Türkçe sayı biçimi kullanılır. Örneğin, ondalıklı değerler `12,50` biçiminde girilebilir.

## Güvenlik ve bilinen sınırlamalar

- Mevcut kimlik doğrulama yapısı geliştirme amaçlı sabit kullanıcı bilgileri kullanır. Üretim ortamında kullanılmadan önce gerçek kullanıcı yönetimi ve güvenli parola saklama mekanizması uygulanmalıdır.
- Uygulama yerel veritabanı kullanır; merkezi ve çok kullanıcılı bir sunucu mimarisi sunmaz.
- Otomatik test altyapısı henüz projeye eklenmemiştir.
- Otomatik yedekleme, geri yükleme ve CSV/Excel dışa aktarma özellikleri mevcut mimari dokümanında geliştirme önerileri olarak belirtilmiştir.
- Büyük ürün listelerinde sayfalama ve performans iyileştirmeleri gerekebilir.

Bu proje, gerçek ticari kullanım öncesinde güvenlik, veri yedekleme ve işlevsel testler açısından ayrıca değerlendirilmelidir.

## Geliştirme

Projenin geliştirilmesine katkıda bulunmak istiyorsanız:

1. Depoyu fork'layın.
2. Değişiklikleriniz için ayrı bir branch oluşturun.
3. Kod değişikliklerinizi yapın.
4. `dotnet build` ile derleme kontrolü gerçekleştirin.
5. Değişikliklerinizi açıklayan bir pull request gönderin.

Mimari değişiklikler yaparken mevcut migration kurallarına ve katmanlar arasındaki bağımlılık yönüne dikkat edin.

## Lisans

Bu proje [MIT Lisansı](LICENSE) kapsamında yayımlanmaktadır.

## Geliştirici

**TolgaGame**

GitHub deposu: https://github.com/TolgaGame/Stok-Takip-PRO
