using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Test_Demo_Ex.Models;
using Test_Demo_Ex.Service;
using Microsoft.EntityFrameworkCore;
using Test_Demo_Ex.Data;



var jwtSecret = "k9Sxxy+qgx8GjbhZbqVLO2V5lLOklDJhY7J5vIRjYlI="; // Минимум 32 символа
var jwtExpirationMinutes = 60;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // Разрешить любые источники
              .AllowAnyMethod()  // Разрешить любые методы
              .AllowAnyHeader(); // Разрешить любые заголовки
    });
});
    

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}


app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", async (ApplicationDbContext db) =>
    await db.Orders.ToListAsync());

app.MapPost("/", async (Order o, ApplicationDbContext db) =>
{
    db.Orders.Add(o);
    await db.SaveChangesAsync();
    return Results.Ok(o);
});

app.MapPut("/{id}", async (int id, OrderUpdateDTO dto, ApplicationDbContext db) =>
{
    var existingOrder = await db.Orders.FirstOrDefaultAsync(o => o.Num == id);
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

    await db.SaveChangesAsync();
    return Results.Ok(existingOrder);
});

app.MapGet("/{num}", async (int num, ApplicationDbContext db) =>
    await db.Orders.FindAsync(num));

app.MapGet("/filter/{param}", async (string param, ApplicationDbContext db) =>
{
    var orders = await db.Orders
        .Where(o => o.Name == param ||
                    o.Num_tel == param ||
                    o.Wishes == param ||
                    o.Address == param ||
                    o.ApartmentNumber == param ||
                    o.Admin == param)
        .ToListAsync();
    return Results.Json(orders);
});

app.MapGet("/stats/completed", async (ApplicationDbContext db) =>
{
    var completedOrders = await db.Orders.CountAsync(o => o.CheckOutDate < DateTime.Now);
    return Results.Json(completedOrders);
});

app.MapGet("/stats/average-stay", async (ApplicationDbContext db) =>
{
    var completedOrders = await db.Orders
        .Where(o => o.CheckOutDate < DateTime.Now && o.CheckOutDate > o.CheckInDate)
        .ToListAsync();
    double averageStay = completedOrders.Any()
        ? completedOrders.Average(o => (o.CheckOutDate - o.CheckInDate).TotalDays)
        : 0;
    return Results.Json(averageStay);
});

app.MapGet("/stats/occupancy", async (ApplicationDbContext db) =>
{
    var occupancyStats = await db.Orders
        .GroupBy(o => o.ApartmentNumber)
        .Select(g => new { ApartmentNumber = g.Key, Count = g.Count() })
        .ToListAsync();
    return Results.Json(occupancyStats);
});


// Регистрация
app.MapPost("/register", async (UserRegisterDTO dto, ApplicationDbContext db) =>
{
    // Проверка наличия пользователя с таким же именем
    if (await db.Users.AnyAsync(u => u.Username == dto.Username))
    {
        return Results.BadRequest("Пользователь с таким именем уже существует.");
    }

    // Хэшируем пароль
    var passwordHash = HashPassword(dto.Password);

    // Создаем нового пользователя
    var user = new User
    {
        Username = dto.Username,
        PasswordHash = passwordHash
    };

    // Добавляем пользователя в базу данных
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok("Регистрация прошла успешно.");
});

// Логин
app.MapPost("/login", async (UserLoginDTO dto, ApplicationDbContext db, JwtService jwt) =>
{
    // Поиск пользователя по имени
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
    if (user == null)
    {
        // Если пользователь не найден
        return Results.Json(new { Message = "Неверное имя пользователя или пароль." }, statusCode: 401);
    }

    // Проверка пароля
    if (!VerifyPassword(dto.Password, user.PasswordHash))
    {
        // Если пароль неверный
        return Results.Json(new { Message = "Неверное имя пользователя или пароль." }, statusCode: 401);
    }

    // Генерация JWT токена
    var token = jwt.GenerateToken(user.Id, user.Role);

    // Возвращаем токен в ответе
    return Results.Json(new
    {
        Message = "Авторизация успешна.",
        Token = token,
        Username = user.Username,
        Role = user.Role
    });
});


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
