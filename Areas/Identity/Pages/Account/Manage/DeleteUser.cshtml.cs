#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class DeleteUserModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly ILogger<DeleteUserModel> _logger;
        private readonly IUserService _userService;


        public DeleteUserModel(UserManager<NuJournalUser> userManager,
                                       ILogger<DeleteUserModel> logger,
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

        [Display(Name = "Confirm user deletion by entering the user's email address")]
        [Required]
        public string ConfirmUserName { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var administrativeUser = await _userManager.GetUserAsync(User);

            if (administrativeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (_userService.IsAdministration(administrativeUser))
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

            if (_userService.IsAdministration(administrativeUser))
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
                        var deleteUser = await _userManager.FindByEmailAsync(selectedUser);

                        var userRole = new NuJournalUserRole();

                        userRole = await _userService.GetDefaultUserRoleAsync(deleteUser);

                        var roleRemovalResult = await _userManager.RemoveFromRoleAsync(deleteUser, userRole.ToString());
                        if (!roleRemovalResult.Succeeded)
                        {                            
                            ViewData["SelectUserList"] = new SelectList(AppUserList, "Email", "UserNameWithDetails");
                            StatusMessage = $"Error: Unable to delete user {deleteUser} from the {roleRemovalResult.ToString()} role.";
                            return Page();
                        }

                        var roleAdditionResult = await _userManager.AddToRoleAsync(deleteUser, NuJournalUserRole.Deleted.ToString());
                        if (!roleAdditionResult.Succeeded)
                        {
                            ViewData["SelectUserList"] = new SelectList(AppUserList, "Email", "UserNameWithDetails");
                            StatusMessage = $"Error: Unable to delete user {deleteUser}.";                                                        
                            return Page();
                        }

                        var deletedUserRoles = new List<string>();
                        deletedUserRoles.Add(NuJournalUserRole.Deleted.ToString());
                        deleteUser.UserRoles = deletedUserRoles;

                        var deleteUserUpdateResult = await _userManager.UpdateAsync(deleteUser);

                        if (deleteUserUpdateResult.Succeeded)
                        {
                            _logger.LogInformation($"User {selectedUser} was successfully deleted by {administrativeUser.UserName}.");
                            StatusMessage = $"User {selectedUser} was successfully deleted by {administrativeUser.UserName}.";
                        }
                        else
                        {
                            foreach (var error in deleteUserUpdateResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                                _logger.LogError(error.Description);
                            }

                            StatusMessage = $"Unable to delete user {selectedUser}.";
                        }

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
