using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DayKast_VFeb2026.Models
{
    [MetadataType(typeof(SuppliersMetadata))]
    public partial class Suppliers { }

    public class SuppliersMetadata
    {
        [Key]
        public int SupplierID { get; set; }

        [Display(Name = "Tedarikçi Firma Adı")]
        [Required(ErrorMessage = "Firma adı boş bırakılamaz.")]
        [StringLength(50, ErrorMessage = "Firma adı en fazla 50 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string CompanyName { get; set; }

        [Display(Name = "Yetkili Kişi")]
        [Required(ErrorMessage = "Yetkili kişi boş bırakılamaz.")]
        [StringLength(30, ErrorMessage = "Yetkili kişi adı en fazla 30 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string ContactPerson { get; set; }

        [Display(Name = "Telefon Numarası")]
        [Required(ErrorMessage = "Telefon numarası boş bırakılamaz.")]
        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\+?[0-9\-\s]+$", ErrorMessage = "Geçerli bir telefon numarası formatı giriniz.")]
        public string Phone { get; set; }

        [Display(Name = "Bakiye / Cari Hesap")]
        [DataType(DataType.Currency)]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Bakiye sıfırdan büyük olmalıdır.")]
        public decimal? Balance { get; set; } // NULL olabilir.
    }
}