#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class RemoveUserModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly ILogger<RemoveUserModel> _logger;
        private readonly IUserService _userService;


        public RemoveUserModel(UserManager<NuJournalUser> userManager,
                                       ILogger<RemoveUserModel> logger,
                                       IUserService userService)
        {
            _userManager = userManager;
            _logger = logger;
            _userService = userService;
        }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public string AccessDeniedImage { get; set; }

        public List<NuJournalUser> AppUserList { get; set; } = new List<NuJournalUser>();

        [Display(Name = "Select a user to delete")]
        public string SelectedUser { get; set; }

        [Display(Name = "Confirm user removal by entering the user's email address")]
        [Required]
        public string ConfirmUserName { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var administrativeUser = await _userManager.GetUserAsync(User);

            if (administrativeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (_userService.IsOwner(administrativeUser))
            {
                AppUserList = _userService.GetAppUserList(administrativeUser);
                if (AppUserList != null)
                {
                    ViewData["SelectUserList"] = new SelectList(AppUserList, "Email", "UserNameWithDetails", AppUserList.FirstOrDefault().Email);
                }
                else
                {
                    ViewData["SelectUserList"] = null;
                }
            }
            else
            {
                AccessDeniedImage = await _userService.GetAccessDeniedImageAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string selectedUser, string confirmUserName)
        {
            var administrativeUser = await _userManager.GetUserAsync(User);
            if (administrativeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (_userService.IsOwner(administrativeUser))
            {
                if (ModelState.IsValid)
                {
                    if (confirmUserName == null)
                    {
                        AppUserList = _userService.GetAppUserList(administrativeUser);
                        if (AppUserList != null)
                        {
                            ViewData["SelectUserList"] = new SelectList(AppUserList, "Email", "UserNameWithDetails");
                            StatusMessage = $"Please confirm the Username for the selected user {selectedUser}.";
                        }
                        else
                        {
                            ViewData["SelectUserList"] = null;
                        }
                    }
                    else if (selectedUser.Contains(confirmUserName))
                    {
                        var removeUser = await _userManager.FindByEmailAsync(selectedUser);
                        var userRemovalResult = await _userManager.DeleteAsync(removeUser);
                        if (userRemovalResult.Succeeded)
                        {
                            _logger.LogInformation($"User {selectedUser} was successfully removed by {administrativeUser.UserName}.");
                            StatusMessage = $"User {selectedUser} was successfully removed by {administrativeUser.UserName}.";
                        }
                        else
                        {
                            foreach (var error in userRemovalResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                                _logger.LogError(error.Description);                                
                            }

                            StatusMessage = $"Unable to remove user {selectedUser}.";
                        }
                    }
                }
            }
            else
            {
                AccessDeniedImage = await _userService.GetAccessDeniedImageAsync();
            }

            return Page();
        }
    }
}
