using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WowNewsAPI.Models;
using WowNewsUI;

namespace WowNewsUI.Controllers
{
    public class AdminNewsController : Controller
    {
        private readonly NewsService _newsService;

        public AdminNewsController(NewsService newsService)
        {
            _newsService = newsService;
        }

        // Отображение всех новостей в обратном порядке по дате создания
        [AllowAnonymous] // Разрешаем доступ к просмотру новостей всем пользователям
        public async Task<IActionResult> Index()
        {
            var newsList = await _newsService.GetAllNewsAsync();
            return View(newsList.OrderByDescending(n => n.CreatedAt));
        }

        // Отображение страницы создания новости
        public IActionResult Create()
        {
            return View();
        }

        // Обработка создания новости
        [HttpPost]
        public async Task<IActionResult> Create(News news)
        {
            if (ModelState.IsValid)

            {
                news.CreatedAt = DateTime.Now;
                await _newsService.CreateNewsAsync(news);
                return RedirectToAction("Index");
            }
            return View(news);
        }

        // Отображение страницы редактирования новости
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null) return NotFound();
            return View(news);
        }

        // Обработка редактирования новости
        [HttpPost]
        public async Task<IActionResult> Edit(News news)
        {
            if (ModelState.IsValid)
            {
                var token = HttpContext.Session.GetString("jwtToken"); // Получаем JWT-токен из сессии или localStorage

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized();
                }

                // Создаем HttpClient для отправки PUT-запроса
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Подготовка URL и данных
                var apiUrl = $"https://localhost:44366/api/news/{news.Id}";
                var jsonContent = new StringContent(JsonConvert.SerializeObject(news), Encoding.UTF8, "application/json");

                // Отправляем PUT-запрос
                var response = await client.PutAsync(apiUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Ошибка обновления: {errorMessage}");
                }
            }
            return View(news);
        }


        // Отображение страницы подтверждения удаления новости
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null) return NotFound();
            return View(news);
        }

        // Обработка удаления новости
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _newsService.DeleteNewsAsync(id);
            return RedirectToAction("Index");
        }
    }
}
