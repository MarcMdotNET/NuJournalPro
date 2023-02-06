#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class OwnerPanelModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;                
        private readonly IUserService _userService;

        public OwnerPanelModel(UserManager<NuJournalUser> userManager,                                                              
                               IUserService userService)
        {
            _userManager = userManager;                        
            _userService = userService;
        }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public string AccessDeniedImage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userRoles = _userManager.GetRolesAsync(user);
            
            if (!userRoles.ToString().Contains(NuJournalUserRole.Owner.ToString()))
            {
                AccessDeniedImage = await _userService.GetAccessDeniedImageAsync();
            }                

            return Page();
        }
    }
}
