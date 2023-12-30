using AspNetCore.Reporting;
using Login.Models;
using Login.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Text;


namespace Login.Controllers;

public class AccountController(SignInManager<AppUser> inManager, UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment) : Controller
{
    
    public IActionResult Login()
    {
        return View();
    }
    public IActionResult Reg()
    {
        return View();
    }
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reg(RegistrarVm registrar)
    {
        if (ModelState.IsValid)
        {
            AppUser appUser = new()
            {
                UserName = registrar.Email,
                Name = registrar.Name,
                Email = registrar.Email,
                Address = registrar.Address,

            };
            var result = await userManager.CreateAsync(appUser, registrar.Password!);
            if (result.Succeeded)
            {
                await inManager.SignInAsync(appUser, false);
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

        }

        return View();
    }




    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm loginVm)
    {
        if (ModelState.IsValid)
        {
            var result =
                await inManager.PasswordSignInAsync(loginVm.UserName!, loginVm.Password!, loginVm.RememberMe, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(loginVm);
        }
        return View(loginVm);
    }



    public async Task<IActionResult> Logout()
    {
        await inManager.SignOutAsync();

        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);



        return RedirectToAction("Login", "Account");
    }

    [Authorize]
    
    public async Task<IActionResult> Print()
    {
        //if (User.Identity.IsAuthenticated);
     
        {
            var data = await userManager.Users.ToListAsync();
            string reportName = "TestReport.pdf";
            string reportPath = Path.Combine(webHostEnvironment.ContentRootPath, "Report", "Registration.rdlc");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding.GetEncoding("utf-8");
            LocalReport report = new LocalReport(reportPath);
            report.AddDataSource("RegistrationDbSet", data.ToList());

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            var result = report.Execute(RenderType.Pdf, 2, parameters);
            var content = result.MainStream.ToArray();
            var contentDisposition = new ContentDisposition
            {
                FileName = reportName,
                Inline = true,
            };
            Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
            return File(content, MediaTypeNames.Application.Pdf);
        }
        
    }

    [HttpGet]
    public async Task<ActionResult> Edit(string username)
    {
        var result = await userManager.FindByNameAsync(username);

        AppUserVm appUserVm = new()
        {

            Name = result.Name,
            UserName = result.UserName,
            Address = result.Address,
            Email = result.Email,
            Id = result.Id,


        };

        return View(appUserVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(AppUserVm appUser)
    {
        if (ModelState.IsValid)
        {
            var userToUpdate = userManager.Users.FirstOrDefault(u => u.Id == appUser.Id);


            if (userToUpdate != null)
            {
                userToUpdate.UserName = appUser.UserName;
                userToUpdate.Address = appUser.Address;
                userToUpdate.Email = appUser.Email;
                userToUpdate.Name = appUser.Name;



                var result = await userManager.UpdateAsync(userToUpdate);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "User not found.");
            }
        }

        return View();
    }

}

