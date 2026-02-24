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
    public class ProductsController : Controller
    {
        DKEntities db = new DKEntities();

        // GET: Products
        public async Task<ActionResult> Index()
        {
            var products = db.Products.Include(p => p.Brands).Include(p => p.Categories).Include(p => p.Suppliers);
            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = await db.Products.FindAsync(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.BrandID = new SelectList(db.Brands, "BrandID", "BrandName");
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ProductID,ProductName,StockCode,CategoryID,BrandID,SupplierID,Scale,PurchasePrice,SalePrice,StockQuantity,ProductType,Condition,Description,IsActive")] Products products,
            int? coverImageIndex)
        {
            try
            {
                // ProductType ve Condition nullable alanlar için ModelState hatalarını temizle
                if (products.ProductType == null)
                    ModelState.Remove("ProductType");
                if (products.Condition == null)
                    ModelState.Remove("Condition");

                if (ModelState.IsValid)
                {
                    db.Products.Add(products);
                    await db.SaveChangesAsync();

                    // Resim yükleme işlemi - Request.Files ile dosyaları al
                    var fileList = new List<HttpPostedFileBase>();
                    for (int f = 0; f < Request.Files.Count; f++)
                    {
                        if (Request.Files.GetKey(f) == "productImages")
                        {
                            fileList.Add(Request.Files[f]);
                        }
                    }

                    if (fileList.Count > 0)
                    {
                        string uploadDir = Server.MapPath("~/Uploads/Products");

                        // Klasör yoksa oluştur
                        if (!System.IO.Directory.Exists(uploadDir))
                        {
                            System.IO.Directory.CreateDirectory(uploadDir);
                        }

                        for (int i = 0; i < fileList.Count; i++)
                        {
                            var file = fileList[i];
                            if (file != null && file.ContentLength > 0)
                            {
                                // Benzersiz dosya adı oluştur
                                string extension = System.IO.Path.GetExtension(file.FileName);
                                string fileName = Guid.NewGuid().ToString() + extension;
                                string filePath = System.IO.Path.Combine(uploadDir, fileName);

                                // Dosyayı diske kaydet
                                file.SaveAs(filePath);

                                // Veritabanına yol olarak kaydet
                                var productImage = new ProductImages
                                {
                                    ProductID = products.ProductID,
                                    ImagePath = "/Uploads/Products/" + fileName,
                                    IsCoverImage = (coverImageIndex.HasValue && coverImageIndex.Value == i)
                                };

                                db.ProductImages.Add(productImage);
                            }
                        }
                        await db.SaveChangesAsync();
                    }

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kaydetme sırasında hata oluştu: " + ex.Message);
            }

            ViewBag.BrandID = new SelectList(db.Brands, "BrandID", "BrandName", products.BrandID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", products.CategoryID);
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName", products.SupplierID);
            return View(products);
        }

        // GET: Products/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = await db.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.ProductID == id);
            if (products == null)
            {
                return HttpNotFound();
            }
            ViewBag.BrandID = new SelectList(db.Brands, "BrandID", "BrandName", products.BrandID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", products.CategoryID);
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName", products.SupplierID);
            ViewBag.ExistingImages = products.ProductImages != null ? products.ProductImages.ToList() : new List<ProductImages>();
            return View(products);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ProductID,ProductName,StockCode,CategoryID,BrandID,SupplierID,Scale,PurchasePrice,SalePrice,StockQuantity,ProductType,Condition,Description,IsActive")] Products products,
            int? coverImageId, string deleteImageIds)
        {
            try
            {
                if (products.ProductType == null)
                    ModelState.Remove("ProductType");
                if (products.Condition == null)
                    ModelState.Remove("Condition");

                if (ModelState.IsValid)
                {
                    db.Entry(products).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    // Silinecek resimler
                    if (!string.IsNullOrEmpty(deleteImageIds))
                    {
                        var idsToDelete = deleteImageIds.Split(',')
                            .Where(s => !string.IsNullOrEmpty(s))
                            .Select(s => int.Parse(s.Trim()))
                            .ToList();

                        foreach (var imgId in idsToDelete)
                        {
                            var img = await db.ProductImages.FindAsync(imgId);
                            if (img != null)
                            {
                                // Dosyayı diskten sil
                                var physicalPath = Server.MapPath("~" + img.ImagePath);
                                if (System.IO.File.Exists(physicalPath))
                                {
                                    System.IO.File.Delete(physicalPath);
                                }
                                db.ProductImages.Remove(img);
                            }
                        }
                        await db.SaveChangesAsync();
                    }

                    // Kapak resmi güncelle
                    if (coverImageId.HasValue)
                    {
                        var allImages = db.ProductImages.Where(pi => pi.ProductID == products.ProductID).ToList();
                        foreach (var img in allImages)
                        {
                            img.IsCoverImage = (img.ImageID == coverImageId.Value);
                        }
                        await db.SaveChangesAsync();
                    }

                    // Yeni resim yükleme
                    var fileList = new List<HttpPostedFileBase>();
                    for (int f = 0; f < Request.Files.Count; f++)
                    {
                        if (Request.Files.GetKey(f) == "newImages")
                        {
                            fileList.Add(Request.Files[f]);
                        }
                    }

                    if (fileList.Count > 0)
                    {
                        string uploadDir = Server.MapPath("~/Uploads/Products");
                        if (!System.IO.Directory.Exists(uploadDir))
                        {
                            System.IO.Directory.CreateDirectory(uploadDir);
                        }

                        foreach (var file in fileList)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                string extension = System.IO.Path.GetExtension(file.FileName);
                                string fileName = Guid.NewGuid().ToString() + extension;
                                string filePath = System.IO.Path.Combine(uploadDir, fileName);

                                file.SaveAs(filePath);

                                var productImage = new ProductImages
                                {
                                    ProductID = products.ProductID,
                                    ImagePath = "/Uploads/Products/" + fileName,
                                    IsCoverImage = false
                                };
                                db.ProductImages.Add(productImage);
                            }
                        }
                        await db.SaveChangesAsync();
                    }

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kaydetme sırasında hata oluştu: " + ex.Message);
            }

            ViewBag.BrandID = new SelectList(db.Brands, "BrandID", "BrandName", products.BrandID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", products.CategoryID);
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName", products.SupplierID);
            ViewBag.ExistingImages = db.ProductImages.Where(pi => pi.ProductID == products.ProductID).ToList();
            return View(products);
        }

        // GET: Products/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = await db.Products.FindAsync(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Products products = await db.Products.FindAsync(id);
            db.Products.Remove(products);
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
