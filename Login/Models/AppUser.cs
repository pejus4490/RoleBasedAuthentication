using Microsoft.AspNetCore.Identity;

namespace Login.Models;

public class AppUser : IdentityUser
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }

}
