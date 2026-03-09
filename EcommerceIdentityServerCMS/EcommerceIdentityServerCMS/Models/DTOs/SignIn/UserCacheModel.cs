namespace EcommerceIdentityServerCMS.Models.DTOs.SignIn
{
    public class UserCacheModel
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> Scopes { get; set; } = new();
        public int? WorkplaceId { get; set; }
        public string WorkplaceType { get; set; } = string.Empty;

    }
}
