using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using NuJournalPro.Models.Settings;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class RemoveUserModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly ILogger<AddNewUserModel> _logger;
        private readonly DefaultGraphics _defaultGraphics;
        private readonly IUserService _userService;


        public RemoveUserModel(UserManager<NuJournalUser> userManager,
                                       ILogger<AddNewUserModel> logger,
                                       IOptions<DefaultGraphics> defaultGraphics,
                                       IUserService userService)
        {
            _userManager = userManager;
            _logger = logger;
            _defaultGraphics = defaultGraphics.Value;
            _userService = userService;
        }

        public string AccessDeniedImage { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public List<NuJournalUser> AppUserList { get; set; } = new List<NuJournalUser>();

        [Display(Name = "Select a user to delete")]
        public string SelectedUser { get; set; }

        [Display(Name = "Confirm user deletion by entering the user's email address")]
        [Required]
        public string ConfirmUserName { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var activeUser = await _userManager.GetUserAsync(User);

            if (activeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            AppUserList = _userService.GetAppUserList(activeUser);
            if (AppUserList is not null)
            {
                if (AppUserList.Count == 0)
                {
                    StatusMessage = "There are no users available to delete.";
                    return Page();
                }
                else
                {
                    SelectedUser = AppUserList.FirstOrDefault().Email;
                    ViewData["SelectUserList"] = new SelectList(AppUserList, "Email", "UserNameWithDetails");
                }
            }
            else
            {
                AccessDeniedImage = await _userService.GetAccessDeniedImageAsync();
                StatusMessage = "Error: You do not have access to this resource.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string selectedUser, string confirmUserName)
        {
            if (ModelState.IsValid)
            {
                var activeUser = await _userManager.GetUserAsync(User);
                if (activeUser == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                if (confirmUserName is null || confirmUserName == string.Empty)
                {
                    AppUserList = _userService.GetAppUserList(activeUser);
                    if (AppUserList is not null)
                    {
                        ViewData["SelectUserList"] = new SelectList(AppUserList, "Email", "UserNameWithDetails");
                        StatusMessage = $"Please confirm the Username for the selected user {selectedUser}.";
                    }
                    else
                    {
                        AccessDeniedImage = await _userService.GetAccessDeniedImageAsync();
                        StatusMessage = "Error: You do not have access to this resource.";
                    }

                    return Page();
                }

                if (selectedUser.Contains(confirmUserName))
                {
                    var deleteUser = await _userManager.FindByEmailAsync(selectedUser);
                    var userDeleteResult = await _userManager.DeleteAsync(deleteUser);

                    if (userDeleteResult.Succeeded)
                    {
                        _logger.LogInformation($"User {selectedUser} was deleted by {activeUser.UserName}.");
                        StatusMessage = $"User {selectedUser} was deleted by {activeUser.UserName}.";
                    }
                }
            }

            return Page();
        }
    }
}
