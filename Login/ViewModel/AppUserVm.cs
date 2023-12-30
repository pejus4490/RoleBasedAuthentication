using Microsoft.AspNetCore.Identity;

namespace Login.ViewModel;

public class AppUserVm : IdentityUser
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Address { get; set; }
}
