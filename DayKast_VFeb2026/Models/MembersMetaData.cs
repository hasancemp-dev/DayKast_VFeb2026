using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DayKast_VFeb2026.Models
{
    [MetadataType(typeof(MembersMetadata))]
    public partial class Members { }

    public class MembersMetadata
    {
        [Key]
        public int MemberID { get; set; }

        [Display(Name = "Ad")]
        [Required(ErrorMessage = "Ad alanı boş bırakılamaz.")]
        [StringLength(30, ErrorMessage = "Adınız en fazla 30 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string FirstName { get; set; }

        [Display(Name = "İkinci Ad")]
        [StringLength(30, ErrorMessage = "İkinci adınız en fazla 30 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string MiddleName { get; set; } // Veritabanında NULL, o yüzden Required yok.

        [Display(Name = "Soyad")]
        [Required(ErrorMessage = "Soyad alanı boş bırakılamaz.")]
        [StringLength(30, ErrorMessage = "Soyadınız en fazla 30 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string LastName { get; set; }

        [Display(Name = "E-Posta")]
        [Required(ErrorMessage = "E-Posta adresi boş bırakılamaz.")]
        [StringLength(100, ErrorMessage = "E-Posta adresiniz en fazla 100 karakter olabilir.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi formatı giriniz.")]
        public string Email { get; set; }

        [Display(Name = "Şifre")]
        [Required(ErrorMessage = "Şifre boş bırakılamaz.")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Şifre en az 8, en fazla 255 karakter olmalıdır.")]
        [DataType(DataType.Password, ErrorMessage = "Geçersiz şifre formatı.")]
        public string Password { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [Required(ErrorMessage = "Doğum tarihi boş bırakılamaz.")]
        [DataType(DataType.Date, ErrorMessage = "Geçerli bir tarih formatı giriniz.")]
        public DateTime Birthday { get; set; }

        [Display(Name = "Ülke")]
        [Required(ErrorMessage = "Ülke boş bırakılamaz.")]
        [StringLength(50, ErrorMessage = "Ülke adı en fazla 50 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string Country { get; set; }

        [Display(Name = "Şehir")]
        [Required(ErrorMessage = "Şehir boş bırakılamaz.")]
        [StringLength(60, ErrorMessage = "Şehir adı en fazla 60 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string City { get; set; }

        [Display(Name = "İlçe")]
        [Required(ErrorMessage = "İlçe boş bırakılamaz.")]
        [StringLength(50, ErrorMessage = "İlçe adı en fazla 50 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string District { get; set; }

        [Display(Name = "Açık Adres")]
        [Required(ErrorMessage = "Açık adres boş bırakılamaz.")]
        [StringLength(400, ErrorMessage = "Adres en fazla 400 karakter olabilir.")]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Güvenlik uyarısı: Zararlı karakterler (<, >) kullanılamaz.")]
        public string Address { get; set; }

        [Display(Name = "Ülke Kodu")]
        [Required(ErrorMessage = "Ülke kodu boş bırakılamaz.")]
        [StringLength(7, ErrorMessage = "Ülke kodu en fazla 7 karakter olabilir (Örn: +90).")]
        [RegularExpression(@"^\+?[0-9]{1,6}$", ErrorMessage = "Geçerli bir ülke kodu giriniz.")]
        public string CountryCode { get; set; }

        [Display(Name = "Telefon Numarası")]
        [Required(ErrorMessage = "Telefon numarası boş bırakılamaz.")]
        [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[0-9\-\s]+$", ErrorMessage = "Telefon numarası sadece rakam, boşluk veya tire içerebilir.")]
        public string Phone { get; set; }

        [Display(Name = "Kullanıcı Rolü")]
        // Veritabanında NULL olabilir. Null ise standart kullanıcı, 1 ise Admin mantığıyla çalışır.
        public byte? MemberRole { get; set; }
    }
}