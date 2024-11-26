using Microsoft.AspNetCore.Identity;

namespace WowNewsAPI.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? AvatarUrl { get; set; }
        
    }
}
