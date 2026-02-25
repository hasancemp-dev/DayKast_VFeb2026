using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DayKast_VFeb2026.Models
{
    [MetadataType(typeof(OrdersMetadata))]
    public partial class Orders { }

    public class OrdersMetadata
    {
        [Key]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Üye ID boş bırakılamaz.")]
        public int MemberID { get; set; }

        [Display(Name = "Sipariş Tarihi")]
        [Required(ErrorMessage = "Sipariş tarihi boş bırakılamaz.")]
        [DataType(DataType.DateTime, ErrorMessage = "Geçerli bir tarih/saat giriniz.")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Toplam Tutar")]
        [Required(ErrorMessage = "Toplam tutar boş bırakılamaz.")]
        [DataType(DataType.Currency)]
        [Range(0.00, (double)decimal.MaxValue, ErrorMessage = "Toplam tutar negatif bir değer olamaz.")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Sipariş Durumu")]
        [Required(ErrorMessage = "Sipariş durumu boş bırakılamaz.")]
        [StringLength(20, ErrorMessage = "Sipariş durumu en fazla 20 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string OrderStatus { get; set; }

        [Display(Name = "Kargo Takip No")]
        [StringLength(50, ErrorMessage = "Takip numarası en fazla 50 karakter olabilir.")]
        [RegularExpression(@"^(\[[a-zA-ZçğıöşüÇĞİÖŞÜ]+\]\s?)?[a-zA-Z0-9]+$", ErrorMessage = "Güvenlik uyarısı: Takip numarası sadece harf ve rakamlardan oluşabilir.")]
        public string TrackingNumber { get; set; } // NULL olabilir.
    }
}