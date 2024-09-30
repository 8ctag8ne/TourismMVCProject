using System.ComponentModel.DataAnnotations;

namespace Tourism.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name ="Email")]
        [DataType(DataType.EmailAddress)]
        public string Email {get; set; } = null!;

        [Required]
        [Display(Name ="Логін")]
        public string Login {get; set;} = null!;
        
        [Required]
        [Display(Name ="Пароль")]
        public string Password {get; set;} = null!;

        [Required]
        [Compare("Password", ErrorMessage = "Паролі не збігаються")]
        [Display(Name = "Підтвердження паролю")]
        [DataType(DataType.Password)]
        public string PasswordConfirm {get; set;} = null!;
    }
}