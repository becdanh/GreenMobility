using System.ComponentModel.DataAnnotations;

namespace GreenMobility.ModelViews
{
    public class LoginVM
    {
        [Key]
        [MaxLength(100)]

        [Display(Name = "Địa chỉ Email")]

        public string UserName { get; set; }

        [Display(Name = "Mật khẩu")]

        public string Password { get; set; }
    }
}
