using System.ComponentModel.DataAnnotations;

namespace GreenMobility.ViewModels
{
    public class CheckoutVM
    {
        public int CustomerId { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
