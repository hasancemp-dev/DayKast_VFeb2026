using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DayKast_VFeb2026.Models;

namespace DayKast_VFeb2026.Controllers
{
    public class HomeController : Controller
    {
        DKEntities db = new DKEntities();

        // Her sayfa için kategorileri ViewBag'e koy (sidebar'da kullanılıyor)
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.Categories = db.Categories.ToList();
            base.OnActionExecuting(filterContext);
        }

        // ============================================================
        // ŞİFRE HASHLEME (SHA256)
        // Şifre düz metin olarak ASLA veritabanında saklanmaz.
        // Kayıt sırasında hash'lenir, giriş sırasında hash karşılaştırılır.
        // ============================================================
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Şifreyi byte dizisine çevir
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Byte dizisini hexadecimal string'e çevir
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // ============================================================
        // ANA SAYFA - Ürünleri ve kategorileri listeler
        // ============================================================
        public ActionResult Index(int? categoryId, int? brandId, string sortBy)
        {
            // Kategorileri ViewBag ile gönder (navbar ve kategori kartları için)
            ViewBag.Categories = db.Categories.Include("Products").ToList();
            ViewBag.Brands = db.Brands.OrderBy(b => b.BrandName).ToList();
            ViewBag.ProductCount = db.Products.Count();
            ViewBag.CategoryCount = db.Categories.Count();
            ViewBag.BrandCount = db.Brands.Count();

            // Filtre değerlerini ViewBag'e gönder (dropdown'larda seçili göstermek için)
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedBrandId = brandId;
            ViewBag.SelectedSort = sortBy;

            // Ürün listesi - aktif ürünler (include ile ilişkili veriler)
            IQueryable<Products> query = db.Products
                .Include("Categories")
                .Include("Brands")
                .Include("ProductImages")
                .Where(p => p.IsActive == true || p.IsActive == null);

            // Kategori filtresi (eğer seçildiyse)
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
                var selectedCategory = db.Categories.Find(categoryId.Value);
                if (selectedCategory != null)
                {
                    ViewBag.SelectedCategory = selectedCategory.CategoryName;
                }
            }

            // Marka filtresi
            if (brandId.HasValue && brandId.Value > 0)
            {
                query = query.Where(p => p.BrandID == brandId.Value);
            }

            // Sıralama
            switch (sortBy)
            {
                case "price-asc":
                    query = query.OrderBy(p => p.SalePrice);
                    break;
                case "price-desc":
                    query = query.OrderByDescending(p => p.SalePrice);
                    break;
                case "name-asc":
                    query = query.OrderBy(p => p.ProductName);
                    break;
                case "name-desc":
                    query = query.OrderByDescending(p => p.ProductName);
                    break;
                default: // "newest" veya boş
                    query = query.OrderByDescending(p => p.ProductID);
                    break;
            }

            var products = query.ToList();
            return View(products);
        }

        // ============================================================
        // ÜRÜN DETAY SAYFASI
        // ============================================================
        public ActionResult ProductDetail(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var product = db.Products
                .Include("Categories")
                .Include("Brands")
                .Include("Suppliers")
                .Include("ProductImages")
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Aynı kategorideki diğer ürünler (benzer ürünler)
            ViewBag.RelatedProducts = db.Products
                .Include("ProductImages")
                .Include("Brands")
                .Where(p => p.CategoryID == product.CategoryID && p.ProductID != product.ProductID && (p.IsActive == true || p.IsActive == null))
                .Take(6)
                .ToList();

            // Yorumları yükle (raw SQL — Database-First EF uyumlu)
            ViewBag.Comments = db.Database.SqlQuery<CommentViewModel>(
                "SELECT c.CommentID, (m.FirstName + ' ' + m.LastName) AS MemberName, c.CommentText, c.Rating, c.CommentDate FROM ProductComments c JOIN Members m ON c.MemberID = m.MemberID WHERE c.ProductID = @pid ORDER BY c.CommentDate DESC",
                new SqlParameter("@pid", id)).ToList();

            // Favori durumunu kontrol et
            if (Session["UserID"] != null)
            {
                int memberId = Convert.ToInt32(Session["UserID"]);
                var favCount = db.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM Favorites WHERE MemberID = @mid AND ProductID = @pid",
                    new SqlParameter("@mid", memberId),
                    new SqlParameter("@pid", id)).FirstOrDefault();
                ViewBag.IsFavorite = favCount > 0;
            }
            else
            {
                ViewBag.IsFavorite = false;
            }

            return View(product);
        }

        // ============================================================
        // KULLANICI GİRİŞ SAYFASI (GET)
        // Admin login'den farklı bir tasarım.
        // ============================================================
        public ActionResult Login()
        {
            // Zaten giriş yapmışsa ana sayfaya yönlendir
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // ============================================================
        // KULLANICI GİRİŞ İŞLEMİ (POST)
        // MemberRole == 1 ise admin/kullanıcı seçim modalı gösterir
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "E-posta ve şifre alanları zorunludur.";
                return View();
            }

            // Girilen şifreyi hashle ve veritabanındaki hash ile karşılaştır
            string hashedPassword = HashPassword(password);
            var member = db.Members.FirstOrDefault(m => m.Email == email && m.Password == hashedPassword);

            if (member == null)
            {
                ViewBag.ErrorMessage = "E-posta veya şifre hatalı.";
                return View();
            }

            // Session bilgilerini oluştur
            Session["UserID"] = member.MemberID;
            Session["UserEmail"] = member.Email;
            Session["UserName"] = $"{member.FirstName} {member.LastName}";
            Session["UserRole"] = member.MemberRole;

            // MemberRole == 1 ise admin/kullanıcı seçim modalını göster
            if (member.MemberRole == 1)
            {
                ViewBag.ShowRoleModal = true;
                return View();
            }

            // Normal kullanıcı ise direkt ana sayfaya yönlendir
            return RedirectToAction("Index");
        }

        // ============================================================
        // ADMİN PANELİNE YÖNLENDİRME
        // Modal'dan "Admin Paneli ile devam et" seçildiğinde çağrılır
        // ============================================================
        public ActionResult RedirectToAdmin()
        {
            if (Session["UserID"] == null || Session["UserRole"] == null)
            {
                return RedirectToAction("Login");
            }

            // Kullanıcının admin olduğunu doğrula
            byte role = Convert.ToByte(Session["UserRole"]);
            if (role != 1)
            {
                return RedirectToAction("Index");
            }

            // Admin session'ını oluştur (AdminController ile uyumlu)
            Session["AdminID"] = Session["UserID"];
            Session["AdminEmail"] = Session["UserEmail"];
            Session["AdminName"] = Session["UserName"];

            return RedirectToAction("YonetimPaneli", "Admin");
        }

        // ============================================================
        // KULLANICI OLARAK DEVAM
        // Modal'dan "Kullanıcı olarak devam et" seçildiğinde çağrılır
        // ============================================================
        public ActionResult ContinueAsUser()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            return RedirectToAction("Index");
        }

        // ============================================================
        // KAYIT OL SAYFASI (GET)
        // ============================================================
        public ActionResult Register()
        {
            // Zaten giriş yapmışsa ana sayfaya yönlendir
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // ============================================================
        // KAYIT OL İŞLEMİ (POST)
        // Yeni üye oluşturur (MemberRole = 0 olarak)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string firstName, string middleName, string lastName, string email,
                                      string password, string confirmPassword,
                                      string countryCode, string phone, DateTime? birthday,
                                      string country, string city, string district, string address)
        {
            // Baş ve sondaki boşlukları temizle
            firstName = firstName?.Trim();
            middleName = middleName?.Trim();
            lastName = lastName?.Trim();
            email = email?.Trim();
            password = password?.Trim();
            confirmPassword = confirmPassword?.Trim();
            countryCode = countryCode?.Trim();
            phone = phone?.Trim();
            country = country?.Trim();
            city = city?.Trim();
            district = district?.Trim();
            address = address?.Trim();

            // Doğrulama kontrolleri
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Ad, soyad, e-posta ve şifre alanları zorunludur.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "Şifreler eşleşmiyor.";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.ErrorMessage = "Şifre en az 6 karakter olmalıdır.";
                return View();
            }

            // E-posta zaten kayıtlı mı kontrol et
            var existingMember = db.Members.FirstOrDefault(m => m.Email == email);
            if (existingMember != null)
            {
                ViewBag.ErrorMessage = "Bu e-posta adresi zaten kayıtlı.";
                return View();
            }

            // Yeni üye oluştur
            var newMember = new Members
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                Email = email,
                Password = HashPassword(password),  // Şifre hash'lenerek kaydedilir
                CountryCode = string.IsNullOrEmpty(countryCode) ? "+90" : countryCode,
                Phone = phone,
                Birthday = birthday ?? DateTime.Now,
                Country = string.IsNullOrEmpty(country) ? "Türkiye" : country,
                City = city,
                District = district,
                Address = address,
                MemberRole = (byte)0  // Normal kullanıcı (sadece admin değiştirebilir)
            };

            db.Members.Add(newMember);
            db.SaveChanges();

            ViewBag.SuccessMessage = "Hesabınız başarıyla oluşturuldu! Giriş yapabilirsiniz.";
            return View();
        }

        // ============================================================
        // ÜRÜN ARAMA (AJAX - JSON)
        // Navbar'daki arama kutusu için
        // ============================================================
        public ActionResult SearchProducts(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Json(new object[] { }, JsonRequestBehavior.AllowGet);
            }

            var results = db.Products
                .Where(p => p.ProductName.Contains(q) && (p.IsActive == true || p.IsActive == null))
                .OrderBy(p => p.ProductName)
                .Take(10)
                .Select(p => new
                {
                    id = p.ProductID,
                    name = p.ProductName,
                    price = p.SalePrice
                })
                .ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        // ============================================================
        // ÖDEME SAYFASI (GET)
        // ============================================================
        public ActionResult Checkout()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // ============================================================
        // SİPARİŞ OLUŞTURMA (POST)
        // Sepet verilerini localStorage'dan alıp DB'ye kaydeder
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(string cartData, string totalAmount)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Oturum açmanız gerekmektedir." });
            }

            try
            {
                int memberId = Convert.ToInt32(Session["UserID"]);
                
                // JavaScript'ten gelen totalAmount string'ini InvariantCulture ile parse et
                decimal parsedTotal = 0;
                if (!string.IsNullOrEmpty(totalAmount))
                {
                    // Hem nokta hem virgül formatını dene
                    totalAmount = totalAmount.Replace(",", ".");
                    decimal.TryParse(totalAmount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedTotal);
                }

                var cartItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItemModel>>(cartData);

                if (cartItems == null || !cartItems.Any())
                {
                    return Json(new { success = false, message = "Sepetiniz boş." });
                }

                // Sipariş oluştur
                var order = new Orders
                {
                    MemberID = memberId,
                    OrderDate = DateTime.Now,
                    TotalAmount = parsedTotal,
                    OrderStatus = "Beklemede",
                    TrackingNumber = null
                };
                db.Orders.Add(order);
                db.SaveChanges();

                // Sipariş detayları
                int totalItemCount = 0;
                foreach (var item in cartItems)
                {
                    var detail = new OrderDetails
                    {
                        OrderID = order.OrderID,
                        ProductID = item.id,
                        Quantity = item.qty,
                        UnitPrice = item.price
                    };
                    db.OrderDetails.Add(detail);
                    totalItemCount += item.qty;

                    // Stoktan düş
                    var product = db.Products.Find(item.id);
                    if (product != null)
                    {
                        product.StockQuantity = product.StockQuantity - item.qty;
                        if (product.StockQuantity < 0) product.StockQuantity = 0;
                    }
                }
                db.SaveChanges();

                return Json(new { success = true, orderId = order.OrderID, itemCount = totalItemCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sipariş oluşturulurken hata: " + ex.Message });
            }
        }

        // ============================================================
        // SİPARİŞ BAŞARI SAYFASI (GET)
        // ============================================================
        public ActionResult OrderSuccess(int orderId, int itemCount)
        {
            ViewBag.OrderId = orderId;
            ViewBag.ItemCount = itemCount;
            return View();
        }

        // ============================================================
        // ÇIKIŞ YAP
        // Kullanıcı session'ını temizler
        // ============================================================
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index");
        }

        // ============================================================
        // HESABIM SAYFASI (GET)
        // Kullanıcının profil bilgilerini gösterir
        // ============================================================
        public ActionResult MyAccount()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            var member = db.Members.Find(userId);
            if (member == null)
            {
                return RedirectToAction("Login");
            }

            return View(member);
        }

        // ============================================================
        // PROFİL GÜNCELLEME (POST)
        // Kullanıcının bilgilerini günceller
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(string Email, string Phone, 
            string Country, string City, string District, string Address, string OldPassword, string NewPassword)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Oturum açmanız gerekmektedir." });
            }

            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                var member = db.Members.Find(userId);
                if (member == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                // Bilgileri güncelle (Ad ve Soyad readonly - değiştirilmez)
                member.Email = Email;
                member.Phone = Phone;
                member.Country = Country;
                member.City = City;
                member.District = District;
                member.Address = Address;

                // Şifre değiştirilmek isteniyorsa - eski şifreyi doğrula
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    if (string.IsNullOrWhiteSpace(OldPassword))
                    {
                        return Json(new { success = false, message = "Eski şifrenizi girmelisiniz." });
                    }

                    string oldPasswordHash = HashPassword(OldPassword);
                    if (member.Password != oldPasswordHash)
                    {
                        return Json(new { success = false, message = "Eski şifreniz yanlış." });
                    }

                    member.Password = HashPassword(NewPassword);
                }

                db.SaveChanges();

                // Session'ı güncelle
                Session["UserName"] = $"{member.FirstName} {member.LastName}";
                Session["UserEmail"] = member.Email;

                return Json(new { success = true, message = "Bilgileriniz başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        // ============================================================
        // SİPARİŞLERİM SAYFASI (GET)
        // Kullanıcının tüm siparişlerini listeler
        // ============================================================
        public ActionResult MyOrders()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            var orders = db.Orders
                .Include("OrderDetails")
                .Include("OrderDetails.Products")
                .Where(o => o.MemberID == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // ============================================================
        // SİPARİŞ İPTAL (POST)
        // Beklemedeki siparişleri iptal eder
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int orderId)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Oturum açmanız gerekmektedir." });
            }

            try
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                var order = db.Orders.FirstOrDefault(o => o.OrderID == orderId && o.MemberID == userId);
                
                if (order == null)
                {
                    return Json(new { success = false, message = "Sipariş bulunamadı." });
                }

                if (order.OrderStatus != "Beklemede")
                {
                    return Json(new { success = false, message = "Sadece beklemedeki siparişler iptal edilebilir." });
                }

                order.OrderStatus = "İptal Edildi";

                // Stokları geri ekle
                var details = db.OrderDetails.Where(d => d.OrderID == orderId).Include("Products").ToList();
                foreach (var detail in details)
                {
                    if (detail.Products != null)
                    {
                        detail.Products.StockQuantity += detail.Quantity;
                    }
                }

                db.SaveChanges();
                return Json(new { success = true, message = "Sipariş başarıyla iptal edildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        // ============================================================
        // İLETİŞİM SAYFASI
        // ============================================================
        public ActionResult Contact()
        {
            return View();
        }

        // ============================================================
        // HAKKIMIZDA SAYFASI
        // ============================================================
        public ActionResult About()
        {
            return View();
        }

        // ============================================================
        // KARİYER SAYFASI
        // ============================================================
        public ActionResult Career()
        {
            return View();
        }

        // ============================================================
        // FAVORİ EKLE/KALDIR (AJAX)
        // ============================================================
        [HttpPost]
        public JsonResult ToggleFavorite(int productId)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "login_required" });
                }

                int memberId = Convert.ToInt32(Session["UserID"]);
                var existCount = db.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM Favorites WHERE MemberID = @mid AND ProductID = @pid",
                    new SqlParameter("@mid", memberId),
                    new SqlParameter("@pid", productId)).FirstOrDefault();

                if (existCount > 0)
                {
                    db.Database.ExecuteSqlCommand(
                        "DELETE FROM Favorites WHERE MemberID = @mid AND ProductID = @pid",
                        new SqlParameter("@mid", memberId),
                        new SqlParameter("@pid", productId));
                    var count = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM Favorites WHERE MemberID = @mid",
                        new SqlParameter("@mid", memberId)).FirstOrDefault();
                    return Json(new { success = true, isFavorite = false, count = count });
                }
                else
                {
                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO Favorites (MemberID, ProductID, AddedDate) VALUES (@mid, @pid, GETDATE())",
                        new SqlParameter("@mid", memberId),
                        new SqlParameter("@pid", productId));
                    var count = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM Favorites WHERE MemberID = @mid",
                        new SqlParameter("@mid", memberId)).FirstOrDefault();
                    return Json(new { success = true, isFavorite = true, count = count });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // FAVORİ SAYISI (AJAX - header badge)
        // ============================================================
        public JsonResult GetFavoriteCount()
        {
            if (Session["UserID"] == null)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }
            int memberId = Convert.ToInt32(Session["UserID"]);
            var count = db.Database.SqlQuery<int>(
                "SELECT COUNT(*) FROM Favorites WHERE MemberID = @mid",
                new SqlParameter("@mid", memberId)).FirstOrDefault();
            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
        }

        // ============================================================
        // FAVORİ ID'LERİ (AJAX - kart kalpleri için)
        // ============================================================
        public JsonResult GetFavoriteIds()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new { ids = new int[0] }, JsonRequestBehavior.AllowGet);
                }
                int memberId = Convert.ToInt32(Session["UserID"]);
                var ids = db.Database.SqlQuery<int>(
                    "SELECT ProductID FROM Favorites WHERE MemberID = @mid",
                    new SqlParameter("@mid", memberId)).ToList();
                return Json(new { ids = ids }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ids = new int[0], error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // ============================================================
        // FAVORİLERİM SAYFASI
        // ============================================================
        public ActionResult MyFavorites()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            int memberId = Convert.ToInt32(Session["UserID"]);
            var products = db.Database.SqlQuery<int>(
                "SELECT ProductID FROM Favorites WHERE MemberID = @mid ORDER BY AddedDate DESC",
                new SqlParameter("@mid", memberId)).ToList();

            var favProducts = new List<Products>();
            foreach (var pid in products)
            {
                var product = db.Products
                    .Include("Brands")
                    .Include("Categories")
                    .Include("ProductImages")
                    .FirstOrDefault(p => p.ProductID == pid);
                if (product != null)
                {
                    favProducts.Add(product);
                }
            }

            return View(favProducts);
        }

        // ============================================================
        // YORUM EKLE (POST - AJAX)
        // ============================================================
        [HttpPost]
        public JsonResult AddComment(int productId, string commentText, int rating)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "login_required" });
                }

                if (string.IsNullOrWhiteSpace(commentText) || rating < 1 || rating > 5)
                {
                    return Json(new { success = false, message = "Lütfen yorum ve puan girin." });
                }

                int memberId = Convert.ToInt32(Session["UserID"]);
                string memberName = Session["UserName"] != null ? Session["UserName"].ToString() : "Anonim";
                var now = DateTime.Now;

                db.Database.ExecuteSqlCommand(
                    "INSERT INTO ProductComments (ProductID, MemberID, CommentText, Rating, CommentDate) VALUES (@pid, @mid, @txt, @rat, @dt)",
                    new SqlParameter("@pid", productId),
                    new SqlParameter("@mid", memberId),
                    new SqlParameter("@txt", commentText.Trim()),
                    new SqlParameter("@rat", rating),
                    new SqlParameter("@dt", now));

                return Json(new
                {
                    success = true,
                    comment = new
                    {
                        memberName = memberName,
                        text = commentText.Trim(),
                        rating = rating,
                        date = now.ToString("dd.MM.yyyy HH:mm")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================================================
        // DISPOSE - Veritabanı bağlantısını temizle
        // ============================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Helper model for deserializing cart data from client
    public class CartItemModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public int qty { get; set; }
    }
}
