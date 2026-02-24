using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DayKast_VFeb2026.Models
{
    [MetadataType(typeof(ProductImagesMetadata))]
    public partial class ProductImages { }

    public class ProductImagesMetadata
    {
        [Key]
        public int ImageID { get; set; }

        [Required(ErrorMessage = "Ürün ID boş bırakılamaz.")]
        public int ProductID { get; set; }

        [Display(Name = "Görsel Yolu")]
        [StringLength(255, ErrorMessage = "Görsel yolu en fazla 255 karakter olabilir.")]
        [RegularExpression(@"^(/[a-zA-Z0-9_\-\.]+)+\.(jpg|jpeg|png|webp|gif)$",
            ErrorMessage = "Güvenlik uyarısı: Sadece güvenli dosya yollarına ve standart resim formatlarına izin verilir.")]
        public string ImagePath { get; set; } // NULL olabilir.

        [Display(Name = "Kapak Görseli Mi?")]
        public bool? IsCoverImage { get; set; } // NULL olabilir.
    }
}
