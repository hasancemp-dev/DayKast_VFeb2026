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
