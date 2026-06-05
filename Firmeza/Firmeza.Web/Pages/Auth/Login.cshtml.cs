using Firmeza.Web.Models;
using Firmeza.Web.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.Web.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Admin/Dashboard");

        await _signInManager.SignOutAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            var result = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user is not null && await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToPage("/Admin/Dashboard");

                await _signInManager.SignOutAsync();
                ErrorMessage = "Acceso denegado. Esta área es solo para administradores.";
                return Page();
            }

            ErrorMessage = "Correo o contraseña incorrectos.";
        }
        catch (Exception)
        {
            ErrorMessage = "Ocurrió un error al iniciar sesión. Intente nuevamente.";
        }

        return Page();
    }
}
