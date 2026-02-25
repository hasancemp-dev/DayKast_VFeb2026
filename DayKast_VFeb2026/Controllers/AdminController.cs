using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DayKast_VFeb2026.Models;
using System.Security.Cryptography;
using System.Text;

namespace DayKast_VFeb2026.Controllers
{
    public class AdminController : Controller
    {
        DKEntities db = new DKEntities();
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
            // Admin kontrolü için private metod
            private bool CheckAdminAuth()
        {
            // AdminID ile giriş yapmışsa VEYA user tarafından admin rolüyle giriş yapmışsa
            if (Session["AdminID"] != null) return true;
            if (Session["UserRole"] != null && Session["UserRole"].ToString() == "1")
            {
                // User tarafından gelen admin - Admin session'larını da oluştur
                Session["AdminID"] = Session["UserID"];
                Session["AdminName"] = Session["UserName"];
                return true;
            }
            return false;
        }

        public ActionResult Login()
        {
            // Zaten giriş yapmışsa yönetim paneline yönlendir
            if (CheckAdminAuth())
            {
                return RedirectToAction("YonetimPaneli");
            }
            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "E-posta ve şifre alanları zorunludur.";
                return View();
            }

            var hashedPassword = HashPassword(password);
            var member = db.Members.FirstOrDefault(m => m.Email == email && m.Password == hashedPassword);

            if (member == null)
            {
                ViewBag.ErrorMessage = "E-posta veya şifre hatalı.";
                return View();
            }

            // MemberRole kontrolü: 1 = Admin, 0 veya null = Normal üye
            if (member.MemberRole != 1)
            {
                ViewBag.ErrorMessage = "Bu alana erişim yetkiniz bulunmamaktadır.";
                return View();
            }

            // Admin session oluştur
            Session["AdminID"] = member.MemberID;
            Session["AdminEmail"] = member.Email;
            Session["AdminName"] = $"{member.FirstName} {member.LastName}";

            // User tarafı session'larını da oluştur (geçiş yapınca tekrar giriş gerektirmesin)
            Session["UserID"] = member.MemberID;
            Session["UserName"] = $"{member.FirstName} {member.LastName}";
            Session["UserEmail"] = member.Email;
            Session["UserRole"] = member.MemberRole;
           
            ViewBag.SuccessMessage = "Giriş başarılı! Yönlendiriliyorsunuz...";
            return RedirectToAction("YonetimPaneli");
        }

        // GET: Admin/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login");
        }

        // GET: Admin/YonetimPaneli
        public ActionResult YonetimPaneli()
        {
            // Admin kontrolü
            if (!CheckAdminAuth())
            {
                return RedirectToAction("Login");
            }

            // Dashboard istatistikleri
            ViewBag.TotalProducts = db.Products.Count();      //Toplam Ürün Sayısı
            ViewBag.TotalOrders = db.Orders.Count();          //Toplam Sipariş Sayısı
            ViewBag.TotalMembers = db.Members.Count();        //Toplam Üye Sayısı
            ViewBag.TotalCategories = db.Categories.Count();  //Toplam Kategori Sayısı
            ViewBag.TotalBrands = db.Brands.Count();          //Toplam Marka Sayısı
            ViewBag.TotalSuppliers = db.Suppliers.Count();    //Toplam Tedarikçi Sayısı

            // Admin bilgisi
            ViewBag.AdminName = Session["AdminName"];

            // Son 5 sipariş
            var recentOrders = db.Orders
                .Include("Members")
                .OrderByDescending(o => o.OrderID)
                .Take(5)
                .ToList();

            ViewBag.RecentOrders = recentOrders;

            return View();
        }

        // GET: Admin/Reports
        public ActionResult Reports()
        {
            if (!CheckAdminAuth())
            {
                return RedirectToAction("Login");
            }

            ViewBag.Title = "Rapor İşlemleri";
            return View();
        }

        // GET: Admin/GenerateReport
        public ActionResult GenerateReport(string reportType, string subType, string format)
        {
            if (!CheckAdminAuth())
                return RedirectToAction("Login");

            string html = "";
            string fileName = "";

            switch (reportType)
            {
                case "products":
                    var result = GenerateProductReport(subType);
                    html = result.Item1;
                    fileName = result.Item2;
                    break;
                case "orders":
                    var result2 = GenerateOrderReport(subType);
                    html = result2.Item1;
                    fileName = result2.Item2;
                    break;
                case "members":
                    var result3 = GenerateMemberReport(subType);
                    html = result3.Item1;
                    fileName = result3.Item2;
                    break;
                case "sales":
                    var result4 = GenerateSalesReport(subType);
                    html = result4.Item1;
                    fileName = result4.Item2;
                    break;
                case "stock":
                    var result5 = GenerateStockReport(subType);
                    html = result5.Item1;
                    fileName = result5.Item2;
                    break;
                case "suppliers":
                    var result6 = GenerateSupplierReport(subType);
                    html = result6.Item1;
                    fileName = result6.Item2;
                    break;
                default:
                    return HttpNotFound();
            }

            if (format == "excel")
            {
                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName + ".xls");
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.Write(html);
                Response.End();
                return null;
            }
            else
            {
                // PDF olarak HTML sayfası döndür (tarayıcıdan yazdırılabilir)
                ViewBag.ReportHtml = html;
                ViewBag.ReportTitle = fileName;
                return View("ReportPrint");
            }
        }

        #region Rapor Oluşturma Metodları

        private string ReportStyle()
        {
            return @"
                <style>
                    body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; color: #333; }
                    h2 { color: #667eea; border-bottom: 2px solid #667eea; padding-bottom: 8px; }
                    h3 { color: #555; margin-top: 20px; }
                    table { width: 100%; border-collapse: collapse; margin-top: 12px; font-size: 13px; }
                    th { background: linear-gradient(135deg, #667eea, #764ba2); color: #fff; padding: 10px 12px; text-align: left; }
                    td { padding: 8px 12px; border-bottom: 1px solid #eee; }
                    tr:nth-child(even) { background: #f9f9fc; }
                    tr:hover { background: #f0edff; }
                    .report-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
                    .report-date { color: #888; font-size: 12px; }
                    .total-row { font-weight: bold; background: #f5f3ff !important; }
                    .badge { display: inline-block; padding: 3px 10px; border-radius: 12px; font-size: 11px; font-weight: 600; }
                    .badge-success { background: #d4edda; color: #155724; }
                    .badge-warning { background: #fff3cd; color: #856404; }
                    .badge-danger { background: #f8d7da; color: #721c24; }
                </style>";
        }

        // ========== ÜRÜN RAPORU ==========
        private Tuple<string, string> GenerateProductReport(string subType)
        {
            int topCount = subType == "top50" ? 50 : 20;
            string fileName = "Urun_Raporu_Top" + topCount + "_SehirBazli";

            var topProducts = db.OrderDetails
                .GroupBy(od => od.ProductID)
                .Select(g => new { ProductID = g.Key, TotalSold = g.Sum(x => x.Quantity) })
                .OrderByDescending(g => g.TotalSold)
                .Take(topCount)
                .ToList();

            var productIds = topProducts.Select(p => p.ProductID).ToList();

            var cityData = db.OrderDetails
                .Where(od => productIds.Contains(od.ProductID))
                .Select(od => new
                {
                    od.ProductID,
                    od.Products.ProductName,
                    od.Quantity,
                    od.UnitPrice,
                    City = od.Orders.Members.City
                })
                .ToList()
                .GroupBy(x => new { x.ProductID, x.ProductName })
                .Select(g => new
                {
                    g.Key.ProductID,
                    g.Key.ProductName,
                    TotalSold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice),
                    Cities = g.GroupBy(c => c.City ?? "Belirtilmemiş")
                              .Select(cg => new { City = cg.Key, Count = cg.Sum(x => x.Quantity) })
                              .OrderByDescending(cg => cg.Count)
                              .ToList()
                })
                .OrderByDescending(x => x.TotalSold)
                .ToList();

            var sb = new StringBuilder();
            sb.Append(ReportStyle());
            sb.Append("<h2>🏆 En Çok Satan Top " + topCount + " Ürün - Şehir Bazlı Rapor</h2>");
            sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");
            sb.Append("<table><tr><th>#</th><th>Ürün Adı</th><th>Toplam Satış</th><th>Toplam Gelir</th><th>En Çok Satan Şehirler</th></tr>");

            int rank = 1;
            foreach (var p in cityData)
            {
                var topCities = string.Join(", ", p.Cities.Take(5).Select(c => c.City + " (" + c.Count + ")"));
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2} adet</td><td>₺{3:N2}</td><td>{4}</td></tr>",
                    rank++, p.ProductName, p.TotalSold, p.TotalRevenue, topCities);
            }

            sb.Append("</table>");
            return Tuple.Create(sb.ToString(), fileName);
        }

        // ========== SİPARİŞ RAPORU ==========
        private Tuple<string, string> GenerateOrderReport(string subType)
        {
            var sb = new StringBuilder();
            sb.Append(ReportStyle());
            string fileName = "Siparis_Raporu";

            switch (subType)
            {
                case "member":
                    fileName += "_UyeBazli";
                    sb.Append("<h2>👤 Üye Bazlı Sipariş Raporu</h2>");
                    sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");

                    var memberOrders = db.Orders
                        .GroupBy(o => new { o.MemberID, o.Members.FirstName, o.Members.LastName, o.Members.City })
                        .Select(g => new
                        {
                            g.Key.MemberID,
                            Name = g.Key.FirstName + " " + g.Key.LastName,
                            g.Key.City,
                            OrderCount = g.Count(),
                            TotalSpent = g.Sum(x => x.TotalAmount)
                        })
                        .OrderByDescending(x => x.TotalSpent)
                        .ToList();

                    sb.Append("<table><tr><th>#</th><th>Üye</th><th>Şehir</th><th>Sipariş Sayısı</th><th>Toplam Harcama</th></tr>");
                    int i = 1;
                    foreach (var m in memberOrders)
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>₺{4:N2}</td></tr>",
                            i++, m.Name, m.City ?? "-", m.OrderCount, m.TotalSpent);
                    }
                    sb.Append("</table>");

                    sb.AppendFormat("<p class='total-row' style='margin-top:16px; padding:10px; background:#f5f3ff; border-radius:8px;'>Toplam: {0} üye, {1} sipariş, ₺{2:N2} gelir</p>",
                        memberOrders.Count, memberOrders.Sum(x => x.OrderCount), memberOrders.Sum(x => x.TotalSpent));
                    break;

                case "city":
                    fileName += "_SehirBazli";
                    sb.Append("<h2>🏙️ Şehir Bazlı Sipariş Raporu</h2>");
                    sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");

                    var cityOrders = db.Orders
                        .GroupBy(o => o.Members.City ?? "Belirtilmemiş")
                        .Select(g => new
                        {
                            City = g.Key,
                            OrderCount = g.Count(),
                            TotalRevenue = g.Sum(x => x.TotalAmount),
                            UniqueMembers = g.Select(x => x.MemberID).Distinct().Count()
                        })
                        .OrderByDescending(x => x.TotalRevenue)
                        .ToList();

                    sb.Append("<table><tr><th>#</th><th>Şehir</th><th>Sipariş Sayısı</th><th>Toplam Gelir</th><th>Benzersiz Müşteri</th></tr>");
                    int j = 1;
                    foreach (var c in cityOrders)
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>₺{3:N2}</td><td>{4}</td></tr>",
                            j++, c.City, c.OrderCount, c.TotalRevenue, c.UniqueMembers);
                    }
                    sb.Append("</table>");
                    break;

                case "product":
                    fileName += "_UrunBazli";
                    sb.Append("<h2>📦 Ürün Bazlı Sipariş Raporu</h2>");
                    sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");

                    var productOrders = db.OrderDetails
                        .GroupBy(od => new { od.ProductID, od.Products.ProductName, od.Products.Categories.CategoryName })
                        .Select(g => new
                        {
                            g.Key.ProductName,
                            Category = g.Key.CategoryName,
                            TotalOrdered = g.Sum(x => x.Quantity),
                            TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice),
                            OrderCount = g.Select(x => x.OrderID).Distinct().Count()
                        })
                        .OrderByDescending(x => x.TotalOrdered)
                        .ToList();

                    sb.Append("<table><tr><th>#</th><th>Ürün</th><th>Kategori</th><th>Satış Adedi</th><th>Sipariş Sayısı</th><th>Toplam Gelir</th></tr>");
                    int k = 1;
                    foreach (var p in productOrders)
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>₺{5:N2}</td></tr>",
                            k++, p.ProductName, p.Category ?? "-", p.TotalOrdered, p.OrderCount, p.TotalRevenue);
                    }
                    sb.Append("</table>");
                    break;
            }

            return Tuple.Create(sb.ToString(), fileName);
        }

        // ========== ÜYE RAPORU ==========
        private Tuple<string, string> GenerateMemberReport(string subType)
        {
            var sb = new StringBuilder();
            sb.Append(ReportStyle());
            string fileName = "Uye_Raporu";

            switch (subType)
            {
                case "city":
                    fileName += "_SehirBazli";
                    sb.Append("<h2>🏙️ Şehir Bazlı Üye Raporu</h2>");
                    sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");

                    var cityMembers = db.Members
                        .GroupBy(m => m.City ?? "Belirtilmemiş")
                        .Select(g => new { City = g.Key, Count = g.Count() })
                        .OrderByDescending(g => g.Count)
                        .ToList();

                    sb.Append("<table><tr><th>#</th><th>Şehir</th><th>Üye Sayısı</th><th>Oran</th></tr>");
                    int total = cityMembers.Sum(c => c.Count);
                    int n = 1;
                    foreach (var c in cityMembers)
                    {
                        double pct = total > 0 ? (double)c.Count / total * 100 : 0;
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>%{3:F1}</td></tr>",
                            n++, c.City, c.Count, pct);
                    }
                    sb.Append("</table>");
                    break;

                case "all":
                default:
                    fileName += "_TumListe";
                    sb.Append("<h2>👥 Tüm Üyeler Listesi</h2>");
                    sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");

                    var allMembers = db.Members.OrderBy(m => m.FirstName).ToList();

                    sb.Append("<table><tr><th>#</th><th>Ad Soyad</th><th>E-posta</th><th>Telefon</th><th>Şehir</th><th>İlçe</th><th>Doğum Tarihi</th></tr>");
                    int idx = 1;
                    foreach (var m in allMembers)
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1} {2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7:dd.MM.yyyy}</td></tr>",
                            idx++, m.FirstName, m.LastName, m.Email, m.Phone ?? "-", m.City ?? "-", m.District ?? "-", m.Birthday);
                    }
                    sb.Append("</table>");
                    break;
            }

            return Tuple.Create(sb.ToString(), fileName);
        }

        // ========== SATIŞ RAPORU ==========
        private Tuple<string, string> GenerateSalesReport(string subType)
        {
            var sb = new StringBuilder();
            sb.Append(ReportStyle());
            string fileName = "Satis_Raporu";

            switch (subType)
            {
                case "monthly":
                    fileName += "_Aylik";
                    sb.Append("<h2>📊 Aylık Satış Raporu</h2>");
                    sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");

                    var monthlySales = db.Orders
                        .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                        .Select(g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            OrderCount = g.Count(),
                            Revenue = g.Sum(x => x.TotalAmount)
                        })
                        .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                        .ToList();

                    sb.Append("<table><tr><th>Dönem</th><th>Sipariş Sayısı</th><th>Toplam Gelir</th></tr>");
                    foreach (var m in monthlySales)
                    {
                        string[] aylar = { "", "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };
                        sb.AppendFormat("<tr><td>{0} {1}</td><td>{2}</td><td>₺{3:N2}</td></tr>",
                            aylar[m.Month], m.Year, m.OrderCount, m.Revenue);
                    }
                    sb.Append("</table>");
                    break;

                case "category":
                    fileName += "_KategoriBazli";
                    sb.Append("<h2>📂 Kategori Bazlı Satış Raporu</h2>");
                    sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");

                    var catSales = db.OrderDetails
                        .GroupBy(od => od.Products.Categories.CategoryName ?? "Kategorisiz")
                        .Select(g => new
                        {
                            Category = g.Key,
                            TotalSold = g.Sum(x => x.Quantity),
                            Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                        })
                        .OrderByDescending(x => x.Revenue)
                        .ToList();

                    sb.Append("<table><tr><th>#</th><th>Kategori</th><th>Satılan Adet</th><th>Toplam Gelir</th></tr>");
                    int ci = 1;
                    foreach (var c in catSales)
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>₺{3:N2}</td></tr>",
                            ci++, c.Category, c.TotalSold, c.Revenue);
                    }
                    sb.Append("</table>");
                    break;

                case "brand":
                    fileName += "_MarkaBazli";
                    sb.Append("<h2>🏷️ Marka Bazlı Satış Raporu</h2>");
                    sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");

                    var brandSales = db.OrderDetails
                        .GroupBy(od => od.Products.Brands.BrandName ?? "Markasız")
                        .Select(g => new
                        {
                            Brand = g.Key,
                            TotalSold = g.Sum(x => x.Quantity),
                            Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                        })
                        .OrderByDescending(x => x.Revenue)
                        .ToList();

                    sb.Append("<table><tr><th>#</th><th>Marka</th><th>Satılan Adet</th><th>Toplam Gelir</th></tr>");
                    int bi = 1;
                    foreach (var b in brandSales)
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>₺{3:N2}</td></tr>",
                            bi++, b.Brand, b.TotalSold, b.Revenue);
                    }
                    sb.Append("</table>");
                    break;
            }

            return Tuple.Create(sb.ToString(), fileName);
        }

        // ========== STOK RAPORU ==========
        private Tuple<string, string> GenerateStockReport(string subType)
        {
            var sb = new StringBuilder();
            sb.Append(ReportStyle());
            string fileName = "Stok_Raporu";

            switch (subType)
            {
                case "critical":
                    fileName += "_KritikStok";
                    sb.Append("<h2>⚠️ Kritik Stok Raporu (10 ve altı)</h2>");
                    sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");

                    var criticalStock = db.Products
                        .Where(p => p.StockQuantity <= 10)
                        .OrderBy(p => p.StockQuantity)
                        .Select(p => new
                        {
                            p.ProductName,
                            p.StockCode,
                            Category = p.Categories.CategoryName,
                            Brand = p.Brands.BrandName,
                            p.StockQuantity,
                            p.SalePrice
                        })
                        .ToList();

                    sb.Append("<table><tr><th>#</th><th>Ürün</th><th>Stok Kodu</th><th>Kategori</th><th>Marka</th><th>Stok</th><th>Fiyat</th><th>Durum</th></tr>");
                    int si = 1;
                    foreach (var p in criticalStock)
                    {
                        string badge = p.StockQuantity == 0
                            ? "<span class='badge badge-danger'>Tükendi</span>"
                            : "<span class='badge badge-warning'>Kritik</span>";
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>₺{6:N2}</td><td>{7}</td></tr>",
                            si++, p.ProductName, p.StockCode ?? "-", p.Category ?? "-", p.Brand ?? "-", p.StockQuantity, p.SalePrice, badge);
                    }
                    sb.Append("</table>");
                    break;

                case "all":
                default:
                    fileName += "_TumStok";
                    sb.Append("<h2>📦 Tüm Ürün Stok Raporu</h2>");
                    sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");

                    var allStock = db.Products
                        .OrderBy(p => p.ProductName)
                        .Select(p => new
                        {
                            p.ProductName,
                            p.StockCode,
                            Category = p.Categories.CategoryName,
                            Brand = p.Brands.BrandName,
                            p.StockQuantity,
                            p.SalePrice,
                            p.PurchasePrice
                        })
                        .ToList();

                    sb.Append("<table><tr><th>#</th><th>Ürün</th><th>Stok Kodu</th><th>Kategori</th><th>Marka</th><th>Stok</th><th>Alış</th><th>Satış</th><th>Durum</th></tr>");
                    int ai = 1;
                    foreach (var p in allStock)
                    {
                        string badge = p.StockQuantity == 0
                            ? "<span class='badge badge-danger'>Tükendi</span>"
                            : p.StockQuantity <= 10
                                ? "<span class='badge badge-warning'>Kritik</span>"
                                : "<span class='badge badge-success'>Yeterli</span>";
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>₺{6:N2}</td><td>₺{7:N2}</td><td>{8}</td></tr>",
                            ai++, p.ProductName, p.StockCode ?? "-", p.Category ?? "-", p.Brand ?? "-", p.StockQuantity, p.PurchasePrice, p.SalePrice, badge);
                    }
                    sb.Append("</table>");
                    break;
            }

            return Tuple.Create(sb.ToString(), fileName);
        }

        // ========== TEDARİKÇİ RAPORU ==========
        private Tuple<string, string> GenerateSupplierReport(string subType)
        {
            var sb = new StringBuilder();
            sb.Append(ReportStyle());
            string fileName = "Tedarikci_Raporu";

            switch (subType)
            {
                case "performance":
                    fileName += "_Performans";
                    sb.Append("<h2>🏆 Tedarikçi Performans Raporu</h2>");
                    sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");

                    var supplierPerf = db.Suppliers
                        .Select(s => new
                        {
                            s.CompanyName,
                            s.ContactPerson,
                            s.Phone,
                            s.Balance,
                            ProductCount = s.Products.Count(),
                            TotalSold = s.Products.SelectMany(p => p.OrderDetails).Sum(od => (int?)od.Quantity) ?? 0,
                            TotalRevenue = s.Products.SelectMany(p => p.OrderDetails).Sum(od => (decimal?)(od.Quantity * od.UnitPrice)) ?? 0
                        })
                        .OrderByDescending(x => x.TotalRevenue)
                        .ToList();

                    sb.Append("<table><tr><th>#</th><th>Firma</th><th>İletişim</th><th>Telefon</th><th>Ürün Sayısı</th><th>Satılan Adet</th><th>Toplam Gelir</th><th>Bakiye</th></tr>");
                    int pi = 1;
                    foreach (var s in supplierPerf)
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>₺{6:N2}</td><td>₺{7:N2}</td></tr>",
                            pi++, s.CompanyName, s.ContactPerson ?? "-", s.Phone ?? "-", s.ProductCount, s.TotalSold, s.TotalRevenue, s.Balance ?? 0);
                    }
                    sb.Append("</table>");
                    break;

                case "all":
                default:
                    fileName += "_TumListe";
                    sb.Append("<h2>🚚 Tüm Tedarikçiler Listesi</h2>");
                    sb.Append("<p class='report-date'>Rapor Tarihi: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>");

                    var allSuppliers = db.Suppliers.OrderBy(s => s.CompanyName).ToList();

                    sb.Append("<table><tr><th>#</th><th>Firma Adı</th><th>İletişim Kişisi</th><th>Telefon</th><th>Bakiye</th><th>Ürün Sayısı</th></tr>");
                    int sui = 1;
                    foreach (var s in allSuppliers)
                    {
                        sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>₺{4:N2}</td><td>{5}</td></tr>",
                            sui++, s.CompanyName, s.ContactPerson ?? "-", s.Phone ?? "-", s.Balance ?? 0, s.Products.Count);
                    }
                    sb.Append("</table>");
                    break;
            }

            return Tuple.Create(sb.ToString(), fileName);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
