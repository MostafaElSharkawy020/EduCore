using System.ComponentModel.DataAnnotations;

namespace EduCore.ViewModels
{
    public class CardFormViewModel
    {
        [Required(ErrorMessage = "Cardholder name is required.")]
        [Display(Name = "Cardholder Name")]
        public string CardholderName { get; set; }

        [Required(ErrorMessage = "Card number is required.")]
        [CreditCard(ErrorMessage = "Enter a valid card number.")]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Expiry date is required.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Use MM/YY format.")]
        [Display(Name = "Expiry (MM/YY)")]
        public string Expiry { get; set; }

        [Required(ErrorMessage = "CVV is required.")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        [Display(Name = "CVV")]
        public string CVV { get; set; }
    }
}
