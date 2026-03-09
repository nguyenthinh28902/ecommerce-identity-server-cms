namespace EcommerceIdentityServerCMS.Models.DTOs.SignIn
{
    public class SignInResponseDto
    {
        // Sử dụng kiểu string cho Id nếu bạn có ý định dùng GUID sau này, 
        // hoặc giữ int nhưng nên nhất quán với DB.
        public int Id { get; set; }

        public string? Email { get; set; }

        // Kỹ thuật đúng: Một User có thể có nhiều Role. 
        // Khởi tạo sẵn List trống để tránh lỗi NullReferenceException khi dùng.
        public List<string> Roles { get; set; } = new();

        // WorkplaceId có thể null nếu là Admin tổng không thuộc chi nhánh nào.
        public int? WorkplaceId { get; set; }
        // WorkplaceType loại văn phòng trưc thuộc
        public string WorkplaceType { get; set; } = string.Empty;

        // Danh sách các quyền (Scopes) mà User này được phép sử dụng.
        public List<string> Scopes { get; set; } = new();
    }
}
