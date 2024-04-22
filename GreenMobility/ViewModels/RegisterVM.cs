using Humanizer.Localisation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GreenMobility.ModelViews
{
    public class RegisterVM
    {
        [Key]
        public int CustomerId { get; set; }
        [Display(Name = "FullName")]
        [Required(ErrorMessage = "FullNameRequired")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "EmailRequired")]
        [MaxLength(150)]
        [DataType(DataType.EmailAddress)]
        [Remote(action: "ValidateEmail", controller: "Accounts")]
        public string Email { get; set; }
        [MaxLength(11)]
        [Required(ErrorMessage = "PhoneRequired")]
        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        [Remote(action: "ValidatePhone", controller: "Accounts")]
        public string Phone { get; set; }
        [Display(Name = "Password")]
        [Required(ErrorMessage = "PasswordRequired")]
        [MinLength(5, ErrorMessage = "MinPassword")]
        public string Password { get; set; }
        [Display(Name = "ConfirmPassword")]
        [Compare("Password", ErrorMessage = "ComparePassword")]
        public string ConfirmPassword { get; set; }
    }
}