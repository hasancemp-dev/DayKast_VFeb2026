using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DayKast_VFeb2026.Models
{
    [MetadataType(typeof(ProductsMetadata))]
    public partial class Products { }

    public class ProductsMetadata
    {
        [Key]
        public int ProductID { get; set; }

        [Display(Name = "Ürün Adı")]
        [Required(ErrorMessage = "Ürün adı boş bırakılamaz.")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string ProductName { get; set; }

        [Display(Name = "Stok Kodu")]
        [Required(ErrorMessage = "Stok kodu boş bırakılamaz.")]
        [StringLength(50, ErrorMessage = "Stok kodu en fazla 50 karakter olabilir.")]
        [RegularExpression(@"^[a-zA-Z0-9\-_]*$", ErrorMessage = "Stok kodu sadece harf, rakam, tire veya alt çizgi içerebilir.")]
        public string StockCode { get; set; }

        [Display(Name = "Kategori")]
        [Required(ErrorMessage = "Kategori ID boş bırakılamaz.")]
        public int CategoryID { get; set; }

        [Display(Name = "Marka")]
        [Required(ErrorMessage = "Marka ID boş bırakılamaz.")]
        public int BrandID { get; set; }

        [Display(Name = "Tedarikçi")]
        [Required(ErrorMessage = "Tedarikçi ID boş bırakılamaz.")]
        public int SupplierID { get; set; }

        [Display(Name = "Ölçek")]
        [StringLength(10, ErrorMessage = "Ölçek en fazla 10 karakter olabilir (Örn: 1:18).")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string Scale { get; set; } // NULL olabilir.

        [Display(Name = "Alış Fiyatı")]
        [Required(ErrorMessage = "Alış fiyatı boş bırakılamaz.")]
        [DataType(DataType.Currency)]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Alış fiyatı sıfırdan büyük olmalıdır.")]
        public decimal PurchasePrice { get; set; }

        [Display(Name = "Satış Fiyatı")]
        [Required(ErrorMessage = "Satış fiyatı boş bırakılamaz.")]
        [DataType(DataType.Currency)]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Satış fiyatı sıfırdan büyük olmalıdır.")]
        public decimal SalePrice { get; set; }

        [Display(Name = "Stok Miktarı")]
        [Required(ErrorMessage = "Stok miktarı boş bırakılamaz.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı negatif bir değer olamaz.")]
        public int StockQuantity { get; set; }

        [Display(Name = "Ürün Tipi")]
        [Range(0.01, byte.MaxValue, ErrorMessage = "Ürün tipi en fazla 255 olabilir.")]

        public byte? ProductType { get; set; } // NULL olabilir.

        [Display(Name = "Kondisyon")]
        [Range(0, 1, ErrorMessage = "Kondisyon için 0 ya da 1 tercih edilmelidir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Açıklama kısmında HTML etiketleri kullanılamaz.")]
        public byte? Condition { get; set; } // NULL olabilir.

        [Display(Name = "Ürün Açıklaması")]
        [DataType(DataType.MultilineText)]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Açıklama kısmında HTML etiketleri kullanılamaz.")]
        public string Description { get; set; } // NULL olabilir.

        [Display(Name = "Aktif Mi?")]
        public bool? IsActive { get; set; } // NULL olabilir.
    }
}