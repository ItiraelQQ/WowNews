using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WowNewsAPI.Data;
using WowNewsAPI.Models;

namespace WowNewsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(AppDbContext context, ILogger<CommentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{newsId}")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments(int newsId)
        {
            var comments = await _context.Comments
                .Where(c => c.NewsId == newsId)
                .Include(c => c.User)
                .ToListAsync();

            return Ok(comments);
        }

        [Authorize]
        [HttpPost("{newsId}")]
        public async Task<ActionResult<Comment>> PostComment(int newsId, [FromBody] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Комментарий не может быть пустым.");
            }

          
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            

            // Логируем UserId, чтобы убедиться, что он корректно передается
            _logger.LogInformation("UserId: {UserId} извлечен из токена.", userId);

            // Проверка существования новости
            var newsExists = await _context.News.AnyAsync(n => n.Id == newsId);
            if (!newsExists)
            {
                return NotFound("Новость не найдена.");
            }

            // Создаем комментарий
            var comment = new Comment
            {
                Content = content,
                PostedDate = DateTime.UtcNow,
                UserId = userId,  // Привязываем UserId к комментарию
                NewsId = newsId
            };

            try
            {
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetComments), new { newsId = comment.NewsId }, comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении комментария.");

                // Временно добавляем подробное сообщение об ошибке в ответ
                return StatusCode(500, new { Message = "Произошла ошибка при сохранении комментария.", ExceptionDetails = ex.ToString() });
            }
        }



    }
}
