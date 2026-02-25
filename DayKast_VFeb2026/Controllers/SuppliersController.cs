using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DayKast_VFeb2026.Models;
using DayKast_VFeb2026.Filters;

namespace DayKast_VFeb2026.Controllers
{
    [AdminAuthFilter]
    public class SuppliersController : Controller
    {
        DKEntities db = new DKEntities();

        // GET: Suppliers
        public async Task<ActionResult> Index()
        {
            return View(await db.Suppliers.ToListAsync());
        }

        // GET: Suppliers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Suppliers suppliers = await db.Suppliers.FindAsync(id);
            if (suppliers == null)
            {
                return HttpNotFound();
            }
            return View(suppliers);
        }

        // GET: Suppliers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Suppliers/Create 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "SupplierID,CompanyName,ContactPerson,Phone,Balance")] Suppliers suppliers)
        {
            if (ModelState.IsValid)
            {
                db.Suppliers.Add(suppliers);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(suppliers);
        }

        // GET: Suppliers/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Suppliers suppliers = await db.Suppliers.FindAsync(id);
            if (suppliers == null)
            {
                return HttpNotFound();
            }
            return View(suppliers);
        }

        // POST: Suppliers/Edit/5 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "SupplierID,CompanyName,ContactPerson,Phone,Balance")] Suppliers suppliers)
        {
            if (ModelState.IsValid)
            {
                db.Entry(suppliers).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(suppliers);
        }

        // GET: Suppliers/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Suppliers suppliers = await db.Suppliers.FindAsync(id);
            if (suppliers == null)
            {
                return HttpNotFound();
            }
            return View(suppliers);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Suppliers suppliers = await db.Suppliers.FindAsync(id);
            db.Suppliers.Remove(suppliers);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: Suppliers/UpdateBalance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateBalance(int supplierId, decimal amount, string operation)
        {
            var supplier = await db.Suppliers.FindAsync(supplierId);
            if (supplier == null)
            {
                return HttpNotFound();
            }

            decimal currentBalance = supplier.Balance ?? 0;

            if (operation == "+")
            {
                // + = ödeme yapıldı, bakiyeden düş
                supplier.Balance = currentBalance - amount;
            }
            else
            {
                // - = borç eklendi, bakiye artsın
                supplier.Balance = currentBalance + amount;
            }

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
