using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// ����������� ������
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ����� �������� ��� ������
    options.Cookie.HttpOnly = true; // ��������� ������ � ���������� ���� �� JavaScript
    options.Cookie.IsEssential = true; // ���� ���������� ��� ������ ������
});

// ����������� HttpClient ��� NewsService
builder.Services.AddHttpClient<NewsService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:44366/"); // �������� �� URL ������ API
});

// ����������� HttpClient ��� AuthService
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:44366/"); // �������� �� URL ������ API
});

// ��������� �������������� � ������
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.LoginPath = "/Account/Login"; // ������� ���� ��� ��������������� ��� ��������������������� �������
        config.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ������ ��� HTTPS
        config.Cookie.HttpOnly = true; // ������ �� ������ � ���� �� JavaScript
        config.ExpireTimeSpan = TimeSpan.FromMinutes(60); // ����� ����� ����
        config.SlidingExpiration = true; // ���������� ������� ����� ���� ��� ���������� ������������
    });

// ����������� IHttpContextAccessor ��� ������ � �������
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// ����������� AuthService ��� ������ � ���������������
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

// �������� ������
app.UseSession();

// ��������� �������������� � �����������
app.UseAuthentication();
app.UseAuthorization();

// ��������� �������� ��� ������������
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=News}/{action=Index}/{id?}");

app.Run();
