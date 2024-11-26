using System.ComponentModel.DataAnnotations;

namespace WowNewsAPI.Models
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ImageUrl { get; set; }

        public NewsCategory Category { get; set; }

        
    }

    public enum NewsCategory
    {
        Retail,
        ClassicProgressive,
        Vanilla
    }
}
