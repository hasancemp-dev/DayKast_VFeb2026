using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DayKast_VFeb2026.Models
{
    [MetadataType(typeof(BrandsMetadata))]
    public partial class Brands { }

    public class BrandsMetadata
    {
        [Key]
        public int BrandID { get; set; }

        [Display(Name = "Marka Adı")]
        [Required(ErrorMessage = "Marka adı boş bırakılamaz.")]
        [StringLength(50, ErrorMessage = "Marka adı en fazla 50 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string BrandName { get; set; }

        [Display(Name = "Menşei (Ülke)")]
        [Required(ErrorMessage = "Menşei boş bırakılamaz.")]
        [StringLength(50, ErrorMessage = "Menşei en fazla 50 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string Origin { get; set; }
    }
}

