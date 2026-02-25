# 🏎️ DayKast - E-Ticaret Projesi

![Version](https://img.shields.io/badge/Sürüm-Şubat_2026-blue.svg)
![Platform](https://img.shields.io/badge/Platform-ASP.NET_MVC_5-512BD4.svg)
![.NET Framework](https://img.shields.io/badge/.NET_Framework-4.7.2-512BD4.svg)
![Database](https://img.shields.io/badge/Veritabanı-SQL_Server-CC2927.svg)
![ORM](https://img.shields.io/badge/ORM-Entity_Framework_6-388E3C.svg)

DayKast, model araba ve koleksiyon ürünlerinin satışının yapıldığı tam kapsamlı bir e-ticaret web uygulamasıdır. Kullanıcı tarafı ve yönetici paneli olmak üzere iki ana bölümden oluşur. 

Model-Based (Database-First) yaklaşımı ile geliştirilmiş sağlam bir veritabanı mimarisine sahiptir.

---

## 🚀 Teknolojiler

Projenin geliştirilmesinde aşağıdaki teknolojiler ve araçlar kullanılmıştır:

* **Backend:** ASP.NET MVC 5, C#
* **Veritabanı & ORM:** SQL Server, Entity Framework 6.1.3 (Database-First)
* **Frontend:** HTML5, CSS3, Bootstrap 5, jQuery 3.7.1
* **İkonlar & Fontlar:** Font Awesome
* **Yardımcı Kütüphaneler:** Newtonsoft.Json 13.0.4 (JSON işlemleri), PagedList.Mvc (Sayfalama)
* **Güvenlik:** SHA256 (Şifre hashleme)
* **Geliştirme Ortamı:** Visual Studio 2019

---

## 📂 Proje Yapısı

Proje dosyaları standart MVC mimarisine uygun olarak aşağıdaki gibi organize edilmiştir:

```text
DayKast_VFeb2026/
│
├── Controllers/                  # MVC Controller'lar (Home, Admin, Products vb.)
├── Models/                       # Veri modelleri ve EDMX modeli
├── Views/                        # Razor View dosyaları (Kullanıcı ve Yönetici)
├── Content/                      # CSS dosyaları ve arayüz görselleri
├── Scripts/                      # JavaScript dosyaları (jQuery, Bootstrap vb.)
├── Uploads/                      # Kullanıcı tarafından yüklenen dosyalar (Ürün görselleri vb.)
└── App_Start/                    # Uygulama yapılandırması (Route, Bundle, Filter)
```

---

## 🗄️ Veritabanı Tabloları

| Tablo Adı | Açıklama |
| :--- | :--- |
| **Products** | Ürün bilgileri (ad, fiyat, stok, açıklama) |
| **Categories** | Ürün kategorileri |
| **Brands** | Markalar |
| **Suppliers** | Tedarikçiler |
| **Members** | Kayıtlı kullanıcılar |
| **Orders** | Siparişler |
| **OrderDetails** | Sipariş detay satırları |
| **OrdersArchive** | Tamamlanmış sipariş arşivi |
| **ProductImages** | Ürün görselleri (çoklu resim desteği) |
| **Favorites** | Kullanıcı favori ürünleri |
| **ProductComments** | Ürün yorumları ve puanlamaları (1-5 yıldız) |

---

## 🌟 Özellikler

### 👤 Kullanıcı Özellikleri
* **Ana Sayfa:** Grid görünümlü ürün listesi; kategori, marka ve fiyat filtreleme; arama ve sıralama fonksiyonları.
* **Ürün Detay:** Çoklu ürün görseli galerisi, benzer ürünler önerisi, favorilere ekleme (kalp butonu) ve yorum/puanlama sistemi (AJAX tabanlı).
* **Sepet & Sipariş:** AJAX ile sayfa yenilenmeden sepete ürün ekleme, sepet yönetimi ve onaylanmış ödeme sayfaları.
* **Kullanıcı Hesabı:** SHA256 ile güvenli kayıt, oturum açma, hesap bilgileri güncelleme ve sipariş geçmişi görüntüleme.
* **Etkileşim:** Pulse animasyonlu favori butonu ve mikro-animasyonlar.

### 🛠️ Yönetici Paneli Özellikleri
* **Dashboard:** Toplam ürün, sipariş, üye ve kategori istatistiklerini barındıran özet ekran.
* **Katalog Yönetimi:** Ürün, kategori ve marka için tam CRUD işlemleri. Çoklu resim yükleme ve kapak resmi belirleme.
* **Kullanıcı & Sipariş Yönetimi:** Üye ve tedarikçi bilgileri düzenleme, sipariş durumu güncelleme.
* **Raporlama:** Belirli kategorilere ve üyelere bazlı detaylı sistem raporları.

---

## ⚙️ Kurulum & Çalıştırma

Projeyi yerel ortamınızda çalıştırmak için aşağıdaki adımları izleyebilirsiniz:

1.  **Projeyi Açın:** `DayKast_VFeb2026.sln` dosyasını Visual Studio 2019 ile açın.
2.  **Paketleri Yükleyin:** Visual Studio menüsünden `Tools > NuGet Package Manager > Restore NuGet Packages` yolunu izleyerek eksik paketleri indirin.
3.  **Veritabanını Hazırlayın:** * SQL Server üzerinde `DayKast` adında boş bir veritabanı oluşturun.
    * Models klasöründeki EDMX modeli üzerinden veya mevcut SQL scriptlerinizle tabloları oluşturun.
4.  **Bağlantı Ayarları:** `Web.config` dosyasındaki `DKEntities` connection string alanını kendi yerel SQL Server bilgilerinize (`Data Source`, `User ID`, `Password` vb.) göre güncelleyin.
5.  **Projeyi Başlatın:** `F5` veya `Ctrl+F5` tuşlarına basarak projeyi IIS Express üzerinde ayağa kaldırın.
    * **Kullanıcı Arayüzü:** `https://localhost:xxxx/Home/Index`
    * **Yönetici Paneli:** `https://localhost:xxxx/Admin/Login`

---

## 🔒 Güvenlik & Tasarım
* Şifreler **SHA256** algoritması ile hashlenerek veritabanında korunur.
* Admin ve normal kullanıcılar için izole edilmiş Session tabanlı oturum yönetimi uygulanmıştır.
* Arayüzde **Glassmorphism** efektleri, gradient butonlar ve modern kart tasarımları tercih edilmiştir. Tüm yapı **Responsive** olarak geliştirilmiştir.

---

**Geliştirici:** Hasan Cem Pınar  
**Tarih:** Şubat 2026
