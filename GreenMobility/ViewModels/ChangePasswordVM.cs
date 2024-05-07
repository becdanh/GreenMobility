using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GreenMobility.ViewModels
{
    public class ChangePasswordVM
    {
        [Key]
        public int UserId { get; set; }

        [DisplayName("CurrentPassword")]
        [Required(ErrorMessage = "CurrentPasswordRequired")]
        public string CurrentPassword { get; set; }

        [DisplayName("NewPassword")]
        [Required(ErrorMessage = "NewPasswordRequired")]
        [MinLength(5, ErrorMessage = "PasswordMinLenght")]
        public string NewPassword { get; set; }


        [DisplayName("ConfirmPassword")]
        [Compare("NewPassword", ErrorMessage = "CompareConfirmAndNewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
