using Microsoft.AspNetCore.Mvc;

namespace EcommerceIdentityServerCMS.Models.DTOs.SignIn
{
    public class ExchangeRequest
    {

        [FromForm(Name = "code")]
        public string Code { get; set; } = string.Empty;

        // Code Verifier để kiểm tra tính hợp lệ của Code (PKCE)
        [FromForm(Name = "code_verifier")]
        public string CodeVerifier { get; set; } = string.Empty;
        [FromForm(Name = "redirect_uri")]
        public string RedirectUri { get; set; } = string.Empty;
    }
}
