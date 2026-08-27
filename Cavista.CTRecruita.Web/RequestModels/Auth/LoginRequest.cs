using System.ComponentModel.DataAnnotations;

namespace Cavista.CTRecruita.Web.RequestModels.Auth
{
    public class Login
    {
        [Required(ErrorMessage = "EmailAddress is required")]
        [EmailAddress(ErrorMessage = "Please supply an email address")]
        public string EmailAddress { get; set; }

        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }    
    }
}
