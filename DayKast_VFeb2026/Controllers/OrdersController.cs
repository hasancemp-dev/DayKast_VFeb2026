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
    public class OrdersController : Controller
    {
        DKEntities db = new DKEntities();

        // GET: Orders
        public async Task<ActionResult> Index()
        {
            var orders = db.Orders.Include(o => o.Members);
            return View(await orders.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Orders orders = await db.Orders
                .Include("Members")
                .Include("OrderDetails")
                .Include("OrderDetails.Products")
                .Where(o => o.OrderID == id)
                .FirstOrDefaultAsync();
            if (orders == null)
            {
                return HttpNotFound();
            }
            return View(orders);
        }

        // GET: Orders/Create
        public ActionResult Create()
        {
            ViewBag.MemberID = new SelectList(db.Members, "MemberID", "FirstName");
            return View();
        }

        // POST: Orders/Create 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "OrderID,MemberID,OrderDate,TotalAmount,OrderStatus,TrackingNumber")] Orders orders)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(orders);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.MemberID = new SelectList(db.Members, "MemberID", "FirstName", orders.MemberID);
            return View(orders);
        }

        // GET: Orders/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Orders orders = await db.Orders.Include("Members").Where(o => o.OrderID == id).FirstOrDefaultAsync();
            if (orders == null)
            {
                return HttpNotFound();
            }
            return View(orders);
        }

        // POST: Orders/Edit/5 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "OrderID,MemberID,OrderDate,TotalAmount,OrderStatus,TrackingNumber")] Orders orders)
        {
            // Kargoda durumunda takip numarası zorunlu
            if (orders.OrderStatus == "Kargoda" && string.IsNullOrWhiteSpace(orders.TrackingNumber))
            {
                ModelState.AddModelError("TrackingNumber", "Kargoda durumu için takip numarası zorunludur!");
            }

            if (ModelState.IsValid)
            {
                db.Entry(orders).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            // Edit view'da Members bilgisi göstermek için tekrar yükle
            var fullOrder = await db.Orders.Include("Members").Where(o => o.OrderID == orders.OrderID).FirstOrDefaultAsync();
            if (fullOrder != null)
            {
                orders.Members = fullOrder.Members;
            }
            return View(orders);
        }

        // GET: Orders/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Orders orders = await db.Orders.FindAsync(id);
            if (orders == null)
            {
                return HttpNotFound();
            }
            return View(orders);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Orders orders = await db.Orders.FindAsync(id);
            db.Orders.Remove(orders);
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
