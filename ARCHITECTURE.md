# Stok Takip PRO — Mimari ve Geliştirici Rehberi

Bu dosya, projeye yeni başlayan bir geliştiricinin veya AI agent'ın kodu baştan okumadan
doğrudan göreve başlayabilmesi için yazılmıştır. Önce **Hızlı Başlangıç** ve **Değişiklik
Tarifleri** bölümlerini okuyun.

---

## 1. Hızlı Başlangıç

| Konu | Değer |
|---|---|
| Uygulama | Stok Takip PRO — tekstil odaklı yerel ürün/stok yönetimi (masaüstü) |
| Platform | Windows 10 / 11, WinForms |
| Hedef | `net9.0-windows`, C#, `Nullable=enable`, `ImplicitUsings=enable` |
| Veritabanı | SQLite (`Microsoft.Data.Sqlite` 9.0.0), ayrı kurulum gerekmez |
| DB dosyası | `%LocalAppData%\StokTakipPRO\stok.db` (klasör ve şema ilk açılışta otomatik oluşur/güncellenir) |
| Giriş (test) | Kullanıcı: `admin` — Şifre: `admin123` |
| Dil / Kültür | Tüm arayüz Türkçe, kültür `tr-TR` (`Program.Turkish`) |

```powershell
dotnet build
dotnet run --project StokTakipPro.csproj
dotnet publish -c Release -r win-x64 --self-contained
```

VS Code: `Ctrl+Shift+B` → **build** / **run** (`.vscode/tasks.json`), F5 ile hata ayıklama.

> **IDE notu:** Dosyalar editör dışında eklenirse dil sunucusu yanlış "tip bulunamadı" (CS0246)
> gösterebilir; `dotnet build` temizse `Developer: Reload Window` / `.NET: Restart Language Server` yeterlidir.

---

## 2. Proje Kısıtları

Kurucu kararlardır; değiştirmeden önce kullanıcıya sorun.

- **En fazla 12 C# dosyası** (şu an tam 12). Yeni dosya eklemek kullanıcı onayı gerektirir.
- **Designer dosyası yok.** Arayüz tamamen kodla kurulur.
- **Tüm SQL parametreli**; SQL yalnızca `Data/` altında yazılır.
- **Decimal değerler SQLite'a `InvariantCulture` ile** metin olarak yazılır/okunur.
- **Magic string/number yok:** sabitler sınıfın başında `const`.
- Gereksiz yorum yok; yalnızca karmaşık yerlere kısa Türkçe açıklama.
- Tüm kullanıcı metinleri Türkçe.
- **Gereksiz soyutlama yok:** generic repository, reflection tabanlı dinamik form, yeni interface yazılmaz.
- Unit test projesi eklenmemiştir.

---

## 3. Dosya Haritası

```
StokTakipPro/
├── StokTakipPro.csproj
├── Program.cs                  Giriş noktası
├── Models/
│   ├── Product.cs              Ürün modeli (tekstil alanları) + ProductFilter (arama/kategori filtresi)
│   └── ProductOptions.cs       Sabit listeler (kategori, birim, durum, sezon, KDV)
├── Data/
│   ├── Database.cs             Bağlantı + sürümlü migration listesi
│   └── ProductRepository.cs    IProductRepository + ProductRepository + DuplicateProductCodeException
├── Services/
│   └── AuthService.cs          IAuthService + AuthService
├── UI/
│   ├── Theme.cs                Renk, font, kontrol stil yardımcıları
│   ├── Dialogs.cs              Paylaşılan MessageBox yardımcıları
│   ├── LoginForm.cs            Giriş penceresi
│   ├── MainForm.cs             Üst bar + saat + TabControl (sadece iskelet)
│   ├── ProductEntryPanel.cs    UserControl: Ürün Ekle / Düzenle formu
│   └── ProductListPanel.cs     UserControl: arama, filtre, grid, özet
└── .vscode/                    tasks.json, launch.json
```

### Bağımlılık yönü

```
Program ──► LoginForm ──► IAuthService
   └──► MainForm ──► ProductEntryPanel ─┐
                └──► ProductListPanel ──┼──► IProductRepository ──► Database ──► SQLite
                                        └──► Theme / Dialogs / ProductOptions
```

Kural: **UI → Repository → Database**. `Data` katmanı `UI`'yi bilmez. Paneller yalnızca
`IProductRepository`'ye bağlıdır ve birbirini bilmez; iletişimi `MainForm` event'lerle kurar.

---

## 4. Dosya Dosya Sorumluluklar

### Program.cs
`ApplyCulture()` → `ApplicationConfiguration.Initialize()` → `TryCreateDatabase()` (hata → MessageBox + çıkış)
→ `LoginForm.ShowDialog()` (OK değilse çıkış) → `Application.Run(new MainForm(new ProductRepository(database)))`.
`Program.Turkish` (`CultureInfo tr-TR`) **tüm biçimlendirme/parse işlemlerinde** kullanılır. DI container yok.

### Models/Product.cs
Düz veri modeli. Zorunlu: `ProductCode`, `Name`. Metin alanları `string` (boş = `""`).
İsteğe bağlı sayısal alanlar **nullable**: `GramsPerSquareMeter`, `WidthCm`, `Quantity`, `RollCount` (int?),
`TotalWeightKg`, `MinStock`, `PurchasePrice`, `SalePrice`, `VatRate`.
Hesaplananlar (DB'de yok): `TotalValue = Quantity × PurchasePrice` (null = 0),
`IsCriticalStock = MinStock > 0 && Quantity < MinStock`.

### Models/ProductOptions.cs
Tüm sabit listeler burada: `Categories` (20 hazır giyim kategorisi), `Sizes` (beden), `FabricTypes` (kumaş türü), `Units`, `Statuses`,
`Seasons`, `VatRates` (+ `VatLabels`, `FormatVat`, `ParseVat`). ComboBox'lar buradan beslenir; **yeni kategori/birim/beden eklemek = listeye 1 satır.**
Listelerin ilk elemanı yeni ürün için varsayılandır (`Units[0]` = Metre, `Statuses[0]` = Stokta).

### Data/Database.cs
- Constructor: klasörü oluşturur, bağlantı dizesini hazırlar.
- `OpenConnection()`: her çağrıda yeni açık bağlantı; çağıran `using` ile kapatır.
- `EnsureCreated()`: `PRAGMA user_version` okur; `Migrations` listesinden **yalnızca henüz uygulanmamış**
  (`Version > user_version`) olanları sırayla çalıştırır. Her migration, sürüm güncellemesiyle **aynı transaction'da**
  uygulanır (yarım kalırsa geri alınır).
- `Migrations`: `new(sürümNo, sql)` satırlarından oluşan sıralı liste. Bir migration birden fazla SQL ifadesi içerebilir.

**Sürümler**

| Sürüm | İçerik |
|---|---|
| 1 | `Products` tablosu (`CREATE TABLE IF NOT EXISTS`) — eski kurulumlar v1 kabul edilir |
| 2 | Tekstil sütunları (`ALTER TABLE ADD COLUMN`; `FabricType`, `Composition` (Kumaş Bileşimi), `Size` dahil), eski satırlar için veri taşıma, `ProductCode` benzersiz index |

v3 yoktur: Beden / Kumaş Türü / Kumaş Bileşimi sütunları v2'de zaten oluşturulduğundan şema değişmedi; bu alanlar yalnızca arayüzde
ComboBox'a (`ProductOptions.Sizes`, `FabricTypes`) çevrildi. Sıradaki migration **v3** olacaktır.

v2 detayları: eski kayıtlara `ProductCode = 'URN-' + 5 haneli Id`, `Unit = 'Adet'`, `Status = 'Stokta'` atanır;
eski `Quantity → StockQuantity`, `WeightKg → TotalWeightKg`, `UnitPrice → PurchasePrice` kopyalanır.
**Eski `Quantity`, `WeightKg`, `UnitPrice` sütunları tabloda kalır ama kod tarafından kullanılmaz** (veri kaybı olmasın,
SQLite'ta sütun silme/NOT NULL kaldırma maliyetli olduğu için).

**Benzersizlik:** `IX_Products_ProductCode` (`COLLATE NOCASE`) — "abc" ile "ABC" aynı kod sayılır.

### Data/ProductRepository.cs
`IProductRepository`: `GetAll()`, `Search(term, category = null)`, `Add`, `Update`, `Delete(id)`.

- **Sütun listesi tek yerde:** `FieldColumns` dizisi SELECT/INSERT/UPDATE SQL'lerini üretir; parametre adı `@SütunAdı`.
- `AddFieldParameters` (yazma) ve `ReadProduct` (okuma, sütun **adıyla**) bu listeyle birebir uyumlu olmalıdır.
  Sütun sıraları (`RowReader.Ordinals`) sorgu başına **bir kez** hesaplanır, satır başına `GetOrdinal` çağrılmaz.
- Null sayısal alan → `DBNull`; decimal → `InvariantCulture` metni (`ToDb`), okurken `ReadDecimal`/`ReadInt`.
- `Model.Quantity` ↔ DB `StockQuantity` (eski `Quantity` sütunuyla çakışmasın diye).
- `Search` = `ProductFilter.Apply(GetAll(), term, category)`. Filtre `Models/Product.cs` içindeki `ProductFilter`'dadır:
  bellekte, kültüre duyarlı (`CompareInfo.IgnoreCase`; SQLite `LIKE` `İ/ı` ayrımını yapamaz).
  Alanlar: ürün kodu, ad, kategori, beden, kumaş türü, kumaş bileşimi, renk, tedarikçi, parti no. `category` doluysa tam eşleşme filtresi.
- `Add`/`Update`: `UNIQUE` ihlali (`SqliteErrorCode == 19` + mesajda "UNIQUE") →
  **`DuplicateProductCodeException`** (Türkçe mesajlı). `Update` 0 satır etkilerse `InvalidOperationException`.
- `Update` `CreatedAt`'i değiştirmez.

### Services/AuthService.cs
`IAuthService.Authenticate(...)`; kullanıcı bilgileri koda gömülü sabitler (`admin`/`admin123`), karşılaştırma
`CryptographicOperations.FixedTimeEquals`. Gerçek kullanıcı yönetimi yoktur.

### UI/Theme.cs
`Theme` (static): renkler (`Header`, `Background`, `Surface`, `Accent`, `Danger`, `CriticalRow` …), fontlar (Segoe UI),
`StyleButton`, `StyleTextBox`, `StyleComboBox`, `StyleGroupBox`, `StyleGrid`, `StyleTabControl` (owner-draw, flat), `CreateCard`.
Görsel detaylar: `FindCategoryColor` / `FindStatusColor` (kategori ve durum → renk), `FindColorByName` (serbest renk metnindeki Türkçe renk adı → renk),
`DrawBadge` (yuvarlak etiket), `StyleCategoryComboBox` (kategori listesinde renkli nokta). **Yeni kategori eklenirse `CategoryColors`'a da 1 satır eklenir**
(eklenmezse gri etiket çıkar); yeni renk adı = `NamedColors`'a 1 satır.

### UI/Dialogs.cs
`Dialogs` (static): `Info`, `Warning`, `Confirm`, `DatabaseError`, `TryDatabase(Action)`, `AppTitle`.
Tüm MessageBox çağrıları buradan yapılır.

### UI/LoginForm.cs
`FixedDialog`, `AcceptButton` (Enter ile giriş), maskeli şifre. Hatalı girişte Türkçe uyarı; başarıda `DialogResult.OK`.

### UI/MainForm.cs (sade iskelet)
Üst bar (başlık + `Timer` ile 1 sn'de güncellenen saat), `TabControl` ve panelleri bağlayan kod.
- `AddTab(başlık, kontrol)`: **yeni sekme = UserControl yaz + constructor'a 1 satır** (`AddTab("Başlık", new XPanel(repository))`).
- Panel arası iletişim (yalnızca MainForm bilir):
  - `listPanel.EditRequested` → `entryPanel.LoadForEdit(product)` + Ekle sekmesine geç
  - `entryPanel.EditCompleted` → Liste sekmesine dön

### UI/ProductEntryPanel.cs
Ekle/Düzenle formu. Üç `GroupBox`: **Genel Bilgi**, **Teknik Bilgi**, **Stok ve Fiyat** (4 sütunlu `TableLayoutPanel`
ızgaraları, dikey kaydırmalı; Kaydet/Temizle çubuğu altta sabit).
- `LoadForEdit(Product)`: forma doldurur, düzenleme moduna geçer (başlık "Ürünü Düzenle", Temizle → "Vazgeç").
- `Save()` → `TryReadProduct()` (doğrulama) → `TryPersist()` (Add/Update; `DuplicateProductCodeException` → uyarı + odak kod kutusunda).
- `EditCompleted` event'i yalnızca **başarılı düzenleme kaydında** tetiklenir.
- Düzenleme yarım bırakılıp sekmeden çıkılırsa (`OnVisibleChanged`, `Visible == false`) form temizlenir ve ekleme moduna döner.
- ComboBox'ların ilk öğesi boş (`""`); listede olmayan eski/özel değer düzenlemede listeye eklenir (veri kaybolmaz).

### UI/ProductListPanel.cs
Arama kutusu + kategori filtresi (`Tümü` dahil) + Düzenle/Sil + grid + alt özet.
- `Reload()`: `GetAll()` → `_allProducts` önbelleği → `ApplyFilter()`. **Panel her görünür olduğunda (`OnVisibleChanged`)
  otomatik çağrılır** (sekmeye her geçişte DB'den yenileme).
- Arama kutusu 250 ms debounce (`_searchTimer`) ile, kategori filtresi anında `ApplyFilter()` çağırır:
  `ProductFilter.Apply(_allProducts, ...)` — **tuş başına DB okuması yoktur.**
- Grid `BufferedGrid` (DoubleBuffered) kullanır; titreme/yavaşlama azalır.
- Kritik stok satırları açık kırmızı (`CellFormatting`, `Product.IsCriticalStock`).
- Özet: toplam çeşit, kritik stoktaki ürün sayısı (>0 ise kırmızı), toplam stok değeri (Miktar × Alış Fiyatı).
- Sil: onay (varsayılan Hayır) → `Delete` → `Reload`. Düzenle: `EditRequested` event'i (çift tık da aynı).

---

## 5. Doğrulama Kuralları (`ProductEntryPanel.TryReadProduct`)

| Alan | Kural |
|---|---|
| Ürün Kodu | Zorunlu, kırpılır, ≤ 50 karakter, benzersiz (büyük/küçük harf duyarsız) |
| Ürün Adı | Zorunlu, ≤ 100 karakter |
| Diğer metin alanları | İsteğe bağlı, ≤ 100 karakter; Notlar ≤ 500 |
| Sayısal alanlar | Boş → `null` (hesaplamada 0); doluysa tr-TR sayı (`12,50`), negatif olamaz, en çok 1.000.000.000 |
| Top/Rulo Sayısı | Boş → `null`; tam sayı, negatif olamaz |

Ondalık parse'ta **binlik ayracı kabul edilmez** (aksi halde `1.5` sessizce `15` olurdu). Hata → `Reject(...)`:
uyarı + odak ilgili kutuya.

---

## 6. Değişiklik Tarifleri

### 6.1 Ürüne yeni alan eklemek (örn. `Barcode`) — hızlı yol
1. **`Models/Product.cs`** → özellik ekle (metin: `string`, sayı: `decimal?`).
2. **`Data/Database.cs`** → `Migrations` listesine **yeni sürüm** ekle:
   `new(3, "ALTER TABLE Products ADD COLUMN Barcode TEXT NOT NULL DEFAULT '';")`
   (sayısal alan: `TEXT NULL`). **Var olan migration'ları asla düzenlemeyin.**
3. **`Data/ProductRepository.cs`** → 3 satır: `FieldColumns`'a `"Barcode"`, `AddFieldParameters`'a `Bind(...)`, `ReadProduct`'a okuma.
4. **UI** (alan formda/gridde görünecekse): `ProductEntryPanel` → kutuyu ekle, `TextBoxes` dizisine koy,
   `Build…Fields` içinde `Place(...)`, `TryReadProduct`, `LoadForEdit`; `ProductListPanel.ConfigureGrid` → `AddColumn(...)`.
5. Aramaya girecekse `ProductFilter.Matches` (`Models/Product.cs`) içine ekle.

> Yalnızca backend alanı (formda yok) gerekiyorsa 1–3 yeterlidir.

### 6.2 Yeni sabit liste değeri / yeni liste
`ProductOptions.cs` içindeki ilgili listeye 1 satır. Yeni liste → yeni `static readonly IReadOnlyList<string>` + `FillCombo`.

### 6.3 Yeni sekme
1. `UI/` altında `XPanel : UserControl` yaz (constructor'da `IProductRepository` alır; veri yenileme gerekiyorsa
   `OnVisibleChanged` içinde `Visible` ise yükle — `ProductListPanel` örneği).
2. `MainForm` constructor'ına `AddTab("Başlık", new XPanel(repository));`
3. Dosya sınırını (12) aşıyorsa kullanıcıya sor.

### 6.4 Yeni repository sorgusu
`IProductRepository`'ye metot ekle → `ProductRepository`'de uygula (parametreli SQL, `using`) → UI'dan
`Dialogs.TryDatabase(...)` ile çağır. Yeni iş kuralı hatası için `DuplicateProductCodeException` gibi özel exception kullan.

### 6.5 Yeni renk/stil
`Theme.cs` içine ekle; formlarda ham `Color.FromArgb` yazma.

### 6.6 Gerçek kimlik doğrulama
`IAuthService` sabit kalır; `AuthService`'i kullanıcı tablosu (yeni migration) + `Rfc2898DeriveBytes.Pbkdf2` ile değiştir.

---

## 7. Tasarım Kararları

1. **Sürümlü migration:** Kullanıcı verisi asla silinmez; yeni alan = yeni migration satırı. `ALTER TABLE ADD COLUMN` tercih edildi
   (SQLite'ta sütun silme/tip değiştirme tablo yeniden oluşturmayı gerektirir).
2. **`FieldColumns` tabanlı SQL:** 25 alanlı tabloda SELECT/INSERT/UPDATE senkron kalsın diye tek kaynak; reflection yok.
3. **Bellekte arama:** Türkçe harf duyarlılığı. Veri büyürse debounce + önbellek eklenebilir.
4. **Decimal → TEXT:** Kültür bağımsızlık; SQL'de `SUM` yapılamaz, toplamlar bellekte hesaplanır.
5. **Null = 0 davranışı:** Alan `null` saklanır (kullanıcı girmedi bilgisi korunur); hesaplamalarda `GetValueOrDefault()`.
6. **Interface'ler:** Yalnızca mevcut `IProductRepository`, `IAuthService`; yenisi eklenmez.
7. **Paneller birbirini bilmez:** Event'ler `MainForm`'da bağlanır; sekme eklemek diğer panelleri etkilemez.
8. **`Dialogs` ayrı dosyada (`UI/Dialogs.cs`):** Tüm MessageBox çağrıları oradan; panellerde doğrudan `MessageBox` yazılmaz.
9. **DPI:** `ApplicationHighDpiMode=SystemAware` + `AutoScaleMode.Dpi`. `PerMonitorV2` test gerektirir.

---

## 8. PR Öncesi Kontrol Listesi

- [ ] C# dosya sınırı (12) aşılmadı (veya kullanıcı onayladı)
- [ ] Var olan migration değiştirilmedi; yeni değişiklik yeni sürüm olarak eklendi
- [ ] SQL yalnızca `Data/` altında ve parametreli; `FieldColumns` / `AddFieldParameters` / `ReadProduct` senkron
- [ ] Decimal → `InvariantCulture`; ekranda `Program.Turkish`
- [ ] Magic string/number yerine `const`; listeler `ProductOptions`'ta
- [ ] DB çağrıları `Dialogs.TryDatabase` veya özel try/catch ile sarıldı
- [ ] Kullanıcı metinleri Türkçe (ı/İ, ş, ğ, ü, ö, ç)
- [ ] Renk/font `Theme`'den; yeni kontrol `Theme.Style…` ile stillendi
- [ ] `Dock`/`Anchor`/`TableLayoutPanel` kullanıldı, `MinimumSize` bozulmadı
- [ ] Gereksiz yorum/soyutlama eklenmedi
- [ ] `dotnet build` uyarısız/hatasız

---

## 9. Bilinen Sınırlar ve Öneriler

- Giriş bilgileri koda gömülü (test amaçlı).
- Liste bellekte tutulur ve tamamı yüklenir (binlerce kayıt için yeterli); on binlerce kayıtta sayfalama düşünülmeli.
- Eski `Quantity`/`WeightKg`/`UnitPrice` sütunları ölü sütun olarak durur.
- Kategori filtresi yalnızca `ProductOptions.Categories` değerlerini içerir (listede olmayan eski kategoriler yalnızca "Tümü" ile görünür).
- Eşzamanlı çok kullanıcı / yedekleme / dışa aktarma yok.
- Otomatik test yok.

Öncelik sırasıyla öneriler: CSV/Excel dışa aktarma, DB yedekleme, gerçek kullanıcı tablosu, `PerMonitorV2`.

---

## 10. Sık Karşılaşılan Tuzaklar

- **`Program.Turkish` dışında kültür kullanmayın.**
- **Migration'lar değiştirilmez**, yalnızca eklenir; sürüm numaraları ardışık ve benzersiz olmalı.
- **`ReadProduct` sütun adıyla okur**; `FieldColumns`'taki ad ile birebir aynı yazın.
- **`ProductEntryPanel`'de yeni TextBox'ı `TextBoxes` dizisine, ComboBox'ı `ComboBoxes` dizisine ekleyin**
  (stil, MaxLength ve temizleme oradan yapılır).
- **Sekme davranışı:** Liste yenilemesi `ProductListPanel.OnVisibleChanged` ile çalışır; `TabControl` seçimini
  programatik değiştirmek (`_tabs.SelectedTab = ...`) yenilemeyi otomatik tetikler.
- **Silinen ürünü düzenleme:** `Update` 0 satır etkilerse hata mesajı gösterilir (bilinçli).
- **DB sıfırlama:** `%LocalAppData%\StokTakipPRO\stok.db` silinirse tüm migration'lar baştan çalışır.

---

## 11. Yeni Göreve Başlarken

1. Bu dosyayı oku; değişikliğin katmanını belirle (Model / Data / UI / Theme).
2. İlgili tarifi (Bölüm 6) izle; yalnızca gerekli dosyalara dokun.
3. Kısıtlara (Bölüm 2) ve kontrol listesine (Bölüm 8) uy.
4. `dotnet build` ile doğrula; kullanıcı aksini belirtmedikçe test/ek dosya ekleme.
5. Mimariyi etkileyen karar aldıysan bu dosyanın ilgili bölümünü güncelle.
