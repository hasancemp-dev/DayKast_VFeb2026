using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DayKast_VFeb2026.Models
{
    [MetadataType(typeof(OrderDetailsMetadata))]
    public partial class OrderDetails { }

    public class OrderDetailsMetadata
    {
        [Key]
        public int DetailID { get; set; }

        [Required(ErrorMessage = "Sipariş ID boş bırakılamaz.")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Ürün ID boş bırakılamaz.")]
        public int ProductID { get; set; }

        [Display(Name = "Miktar")]
        [Required(ErrorMessage = "Miktar boş bırakılamaz.")]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır. (Negatif veya sıfır değer girilemez).")]
        public int Quantity { get; set; }

        [Display(Name = "Birim Fiyat")]
        [Required(ErrorMessage = "Birim fiyat boş bırakılamaz.")]
        [DataType(DataType.Currency)]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Birim fiyat 0'dan büyük olmalıdır.")]
        public decimal UnitPrice { get; set; }
    }
}