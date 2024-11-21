using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Test_Demo_Ex.Models;



var jwtSecret = "k9Sxxy+qgx8GjbhZbqVLO2V5lLOklDJhY7J5vIRjYlI="; // Минимум 32 символа
var jwtExpirationMinutes = 60;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // Разрешить любые источники
              .AllowAnyMethod()  // Разрешить любые методы
              .AllowAnyHeader(); // Разрешить любые заголовки
    });
});
    

builder.Services.AddSingleton(new UserRepository());
builder.Services.AddSingleton(new JwtService(jwtSecret, jwtExpirationMinutes));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowAll");


app.UseAuthentication();
app.UseAuthorization();

List<Order> repo = new List<Order>()
{
    //new order(1, "Èâàí Èâàíîâ", "1234567890", "Íèêàêèõ ïîæåëàíèé", "Ìîñêâà, óë. Ïóøêèíà, ä. 1", "101" ,DateTime.Now, DateTime.Now.AddDays(1), "Ïðèìåð ïîæåëàíèÿ", "Àäìèíèñòðàòîð 1")
};

app.MapGet("/", () => repo);
app.MapPost("/", (Order o) => repo.Add(o));

app.MapPut("/{id}", (int id, OrderUpdateDTO dto) =>
{
    var existingOrder = repo.FirstOrDefault(o => o.Num == id);
    if (existingOrder is null)
    {
        return Results.NotFound();
    }
    if (dto.CheckInDate.HasValue)
        existingOrder.CheckInDate = dto.CheckInDate.Value;
    if (dto.CheckOutDate.HasValue)
        existingOrder.CheckOutDate = dto.CheckOutDate.Value;
    if (!string.IsNullOrEmpty(dto.AdditionalWishes))
        existingOrder.AdditionalWishes = dto.AdditionalWishes;
    return Results.Ok(existingOrder);
});
app.MapGet("/{num}", (int num) => repo.Find(o => o.Num == num));
app.MapGet("/filter/{param}", (string param) => repo.FindAll(o =>
    o.Name == param ||
    o.Num_tel == param ||
    o.Wishes == param ||
    o.Address == param ||
    o.ApartmentNumber == param ||
    o.Admin == param));

app.MapGet("/stats/completed", () =>
{
    var completedOrders = repo.Count(o => o.CheckOutDate < DateTime.Now);
    return Results.Json(completedOrders);
});

app.MapGet("/stats/average-stay", () =>
{
    var completedOrders = repo.Where(o => o.CheckOutDate < DateTime.Now && o.CheckOutDate > o.CheckInDate);
    double averageStay = completedOrders.Any() ? completedOrders.Average(o => (o.CheckOutDate - o.CheckInDate).TotalDays) : 0;
    return Results.Json(averageStay);
});

app.MapGet("/stats/occupancy", () =>
{
    var occupancyStats = repo.GroupBy(o => o.ApartmentNumber)
                             .Select(g => new { ApartmentNumber = g.Key, Count = g.Count() })
                             .ToList();
    return Results.Json(occupancyStats);
});


// Регистрация
app.MapPost("/register", (UserRegisterDTO dto, UserRepository repo) =>
{
    if (repo.GetUserByUsername(dto.Username) != null)
        return Results.BadRequest("Пользователь с таким именем уже существует.");

    var passwordHash = HashPassword(dto.Password);
    var user = new User
    {
        Username = dto.Username,
        PasswordHash = passwordHash
    };

    repo.AddUser(user);
    return Results.Ok("Регистрация прошла успешно.");
});

// Логин
app.MapPost("/login", (UserLoginDTO dto, UserRepository repo, JwtService jwt) =>
{
    var user = repo.GetUserByUsername(dto.Username);
    if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
        return Results.Unauthorized();

    var token = jwt.GenerateToken(user.Id, user.Role);
    return Results.Json(new { Token = token });
});

// Пример защищённого маршрута
app.MapGet("/secure", () => "Добро пожаловать в защищённую область!")
    .RequireAuthorization();

app.Run();

// Методы для работы с паролями
static string HashPassword(string password)
{
    using var sha256 = SHA256.Create(); // Создаем объект SHA256
    var bytes = Encoding.UTF8.GetBytes(password);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash); // Возвращаем хэш в виде строки Base64
}

static bool VerifyPassword(string password, string storedHash)
{
    return HashPassword(password) == storedHash;
}

public class UserRepository
{
    private readonly List<User> _users = new();

    public User? GetUserByUsername(string username) =>
        _users.FirstOrDefault(u => u.Username == username);

    public void AddUser(User user) => _users.Add(user);
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // Пароль в виде хэша
    public string Role { get; set; } = "User"; // По умолчанию роль "User"
}

public class UserRegisterDTO
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserLoginDTO
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class JwtService
{
    private readonly string _secret;
    private readonly int _accessTokenExpirationMinutes;

    public JwtService(string secret, int accessTokenExpirationMinutes)
    {
        _secret = secret;
        _accessTokenExpirationMinutes = accessTokenExpirationMinutes;
    }

    public string GenerateToken(Guid userId, string role)
    {
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), // Уникальный идентификатор пользователя
            new Claim(ClaimTypes.Role, role), // Роль пользователя
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Идентификатор токена
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
