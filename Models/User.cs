namespace Test_Demo_Ex.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Пароль в виде хэша
        public string Role { get; set; } = "User"; // По умолчанию роль "User"
    }
}
