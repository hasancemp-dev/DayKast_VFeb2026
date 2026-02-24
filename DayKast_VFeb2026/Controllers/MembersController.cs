using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using DayKast_VFeb2026.Models;

namespace DayKast_VFeb2026.Controllers
{
    public class MembersController : Controller
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

        // GET: Members
        public async Task<ActionResult> Index()
        {
            return View(await db.Members.ToListAsync());
        }

        // GET: Members/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Members members = await db.Members.FindAsync(id);
            if (members == null)
            {
                return HttpNotFound();
            }
            return View(members);
        }

        // GET: Members/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Members/Create 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "MemberID,FirstName,MiddleName,LastName,Email,Password,Birthday,Country,City,District,Address,CountryCode,Phone,MemberRole")] Members members)
        {
            if (ModelState.IsValid)
            {
                // Şifreyi hashle
                if (!string.IsNullOrEmpty(members.Password))
                {
                    members.Password = HashPassword(members.Password);
                }
                db.Members.Add(members);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(members);
        }

        // GET: Members/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Members members = await db.Members.FindAsync(id);
            if (members == null)
            {
                return HttpNotFound();
            }
            return View(members);
        }

        // POST: Members/Edit/5 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "MemberID,FirstName,MiddleName,LastName,Email,Birthday,Country,City,District,Address,CountryCode,Phone,MemberRole")] Members members, string OldPassword, string NewPassword, string CurrentPasswordHash)
        {
            if (ModelState.IsValid)
            {
                // Eski şifre doğrulaması
                string oldPasswordHash = HashPassword(OldPassword);
                if (oldPasswordHash != CurrentPasswordHash)
                {
                    ViewBag.PasswordError = "Eski şifre yanlış!";
                    // Veritabanından güncel üye bilgilerini yükle
                    var currentMember = await db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.MemberID == members.MemberID);
                    if (currentMember != null)
                    {
                        members.Password = currentMember.Password;
                    }
                    return View(members);
                }

                // Yeni şifre varsa hashle, yoksa eski şifreyi koru
                if (!string.IsNullOrEmpty(NewPassword))
                {
                    members.Password = HashPassword(NewPassword);
                }
                else
                {
                    members.Password = CurrentPasswordHash;
                }

                db.Entry(members).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(members);
        }

        // GET: Members/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Members members = await db.Members.FindAsync(id);
            if (members == null)
            {
                return HttpNotFound();
            }
            return View(members);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Members members = await db.Members.FindAsync(id);
            db.Members.Remove(members);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
