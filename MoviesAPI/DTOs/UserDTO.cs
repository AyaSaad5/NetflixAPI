using Microsoft.AspNetCore.Identity;

namespace MoviesAPI.DTOs
{
    public class UserDTO : IdentityUser
    {
        [MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string LastName { get; set; }
    }
}
