using WowNewsAPI.Data;

namespace WowNewsAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime PostedDate { get; set; }
        public string UserId { get; set; }
        public int NewsId { get; set; }

        public ApplicationUser User { get; set; }
        public News News { get; set; }
    }
}
