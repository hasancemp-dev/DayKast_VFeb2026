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

namespace DayKast_VFeb2026.Controllers
{
    public class BrandsController : Controller
    {
        DKEntities db = new DKEntities();

        // GET: Brands
        public async Task<ActionResult> Index(string msj)
        {
            return View(await db.Brands.ToListAsync());
        }

        // GET: Brands/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Brands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "BrandID,BrandName,Origin")] Brands brands)
        {
            if (ModelState.IsValid)
            {
                db.Brands.Add(brands);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(brands);
        }

        // GET: Brands/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Brands brands = await db.Brands.FindAsync(id);
            if (brands == null)
            {
                return HttpNotFound();
            }
            return View(brands);
        }

        // POST: Brands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "BrandID,BrandName,Origin")] Brands brands)
        {
            if (ModelState.IsValid)
            {
                db.Entry(brands).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(brands);
        }

        // GET: Brands/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Brands brands = await db.Brands.FindAsync(id);
            if (brands == null)
            {
                return HttpNotFound();
            }
            return View(brands);
        }

        // POST: Brands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Brands brands = await db.Brands.FindAsync(id);
            db.Brands.Remove(brands);
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
