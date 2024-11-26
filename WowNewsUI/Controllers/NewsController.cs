using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using WowNewsAPI.Models;

public class NewsController : Controller
{
    private readonly NewsService _newsService;

    public NewsController(NewsService newsService)
    {
        _newsService = newsService;
    }

    // GET: /news
    public async Task<IActionResult> Index(string category)
    {
        var newsList = await _newsService.GetAllNewsAsync();

        // Передаем список категорий в представление через ViewBag
        ViewBag.Categories = new SelectList(Enum.GetValues(typeof(NewsCategory)), category);

        if (!string.IsNullOrEmpty(category))
        {
            newsList = newsList.Where(n => n.Category.ToString() == category).ToList();
        }

        return View(newsList.OrderByDescending(n => n.CreatedAt));
    }





    // GET: /news/details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var newsItem = await _newsService.GetNewsByIdAsync(id);
        if (newsItem == null) return NotFound();

        return View(newsItem);
    }

    // GET: /news/create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /news/create
    [HttpPost]
    public async Task<IActionResult> Create(News news)
    {
        if (ModelState.IsValid)
        {
            await _newsService.CreateNewsAsync(news);
            return RedirectToAction(nameof(Index));
        }
        return View(news);
    }
    public static class NewsCategoryHelper
    {
        // Метод для перевода категорий на русский
        public static string GetCategoryName(NewsCategory category)
        {
            return category switch
            {
                NewsCategory.ClassicProgressive => "Прогрессивная классика",
                NewsCategory.Vanilla => "Классика",
                NewsCategory.Retail => "Розничная версия",
                // Добавьте другие категории по мере необходимости
                _ => "Неизвестная категория"
            };
        }
    }

    // Другие методы для редактирования и удаления новостей
}
