using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DayKast_VFeb2026.Models
{
    [MetadataType(typeof(CategoriesMetadata))]
    public partial class Categories { }

    public class CategoriesMetadata
    {
        [Key]
        public int CategoryID { get; set; }

        [Display(Name = "Kategori Adı")]
        [Required(ErrorMessage = "Kategori adı boş bırakılamaz.")]
        [StringLength(50, ErrorMessage = "Kategori adı en fazla 50 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string CategoryName { get; set; }
    }
}