using palgineer.models2;

namespace palgineer.DTO
{
    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public Engineer Engineer { get; set; } = null!;
    }
}
