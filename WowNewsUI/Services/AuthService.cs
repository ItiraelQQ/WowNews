using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WowNewsAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Net.Http.Headers;
using WowNewsUI.Models;
using Microsoft.Extensions.Logging;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(HttpClient httpClient, IHttpContextAccessor contextAccessor, ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _contextAccessor = contextAccessor;
        _logger = logger;
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        var registerData = new { Username = username, Password = password };
        var content = new StringContent(JsonSerializer.Serialize(registerData), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://localhost:44366/api/account/register", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        var loginData = new { Username = username, Password = password };
        var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://localhost:44366/api/account/login", content);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(responseData);

            // Извлекаем токен из ответа
            string? token = json.GetProperty("token").GetString();

            if (string.IsNullOrEmpty(token))
            {
                return "Token is missing in response.";
            }

            // Сохраняем токен в сессии
            _contextAccessor.HttpContext.Session.SetString("jwtToken", token);

            // Сохраняем токен в заголовке для будущих запросов
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return "Success";
        }
        else
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            return $"Error: {errorMessage}";
        }
    }

    public async Task<bool> CheckLoginStatusAsync()
    {
        var response = await _httpClient.GetAsync("https://localhost:44366/api/account/check-login-status");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CheckLoginStatusResponse>(content);
            return result?.IsLoggedIn ?? false;
        }

        // Возвращаем подробности ошибки для диагностики
        var errorMessage = await response.Content.ReadAsStringAsync();
        _logger.LogError("CheckLoginStatus failed: {ErrorMessage}", errorMessage);
        return false;
    }

    public async Task<string> GetProfileAsync()
    {
        var token = _contextAccessor.HttpContext.Session.GetString("jwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return "No token found.";
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync("https://localhost:44366/api/account/profile");

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();

            try
            {
                var profileData = JsonSerializer.Deserialize<ProfileData>(responseData);
                if (profileData == null)
                {
                    return "Ошибка: неверные данные профиля.";
                }

                return $"Username: {profileData.Username}, Avatar URL: {profileData.AvatarUrl}";
            }
            catch (JsonException ex)
            {
                _logger.LogError("Ошибка десериализации профиля: {Exception}", ex);
                return $"Ошибка десериализации: {ex.Message}";
            }
        }
        else
        {
            _logger.LogError("Ошибка API: {StatusCode} - {Content}", response.StatusCode, await response.Content.ReadAsStringAsync());
            return $"API Error: {response.StatusCode}";
        }
    }



    // Добавьте класс для десериализации ответа
    public class CheckLoginStatusResponse
    {
        public bool IsLoggedIn { get; set; }
        public string Username { get; set; }
    }

   
    
}
