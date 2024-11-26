using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Регистрация сессии
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Время ожидания для сессии
    options.Cookie.HttpOnly = true; // Запрещает доступ к сессионной куки из JavaScript
    options.Cookie.IsEssential = true; // Куки необходимы для работы сессии
});

// Регистрация HttpClient для NewsService
builder.Services.AddHttpClient<NewsService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:44366/"); // Замените на URL вашего API
});

// Регистрация HttpClient для AuthService
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:44366/"); // Замените на URL вашего API
});

// Настройка аутентификации с куками
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.LoginPath = "/Account/Login"; // Укажите путь для перенаправления при неаутентифицированном доступе
        config.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Только для HTTPS
        config.Cookie.HttpOnly = true; // Запрет на доступ к куке из JavaScript
        config.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Время жизни куки
        config.SlidingExpiration = true; // Обновление времени жизни куки при активности пользователя
    });

// Регистрация IHttpContextAccessor для работы с сессией
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Регистрация AuthService для работы с аутентификацией
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NewsService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Включаем сессию
app.UseSession();

// Включение аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

// Настройка маршрута для контроллеров
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=News}/{action=Index}/{id?}");

app.Run();
