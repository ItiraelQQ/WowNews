using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using WowNewsAPI.Models;

public class NewsService
{
    private readonly HttpClient _httpClient;

    public NewsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Получение всех новостей
    public async Task<List<News>> GetAllNewsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<News>>("https://localhost:44366/api/news");
    }

    // Получение новости по ID
    public async Task<News> GetNewsByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<News>($"https://localhost:44366/api/news/{id}");
    }

    // Создание новости
    public async Task CreateNewsAsync(News news)
    {
        // Отправляем данные на API
        var response = await _httpClient.PostAsJsonAsync("https://localhost:44366/api/news", news);

        // Если ответ не успешный, выбрасываем исключение
        response.EnsureSuccessStatusCode();
    }


    // Редактирование новости
    public async Task UpdateNewsAsync(int id, News news)
    {
        var response = await _httpClient.PutAsJsonAsync($"https://localhost:44366/api/news/{id}", news);
        response.EnsureSuccessStatusCode();
    }

    // Удаление новости
    public async Task DeleteNewsAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"https://localhost:44366/api/news/{id}");
        response.EnsureSuccessStatusCode();
    }
}
