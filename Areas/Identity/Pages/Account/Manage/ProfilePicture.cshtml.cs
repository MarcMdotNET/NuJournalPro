#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuJournalPro.Data;
using NuJournalPro.Models.Identity;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class ProfilePictureModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly ILogger<ProfilePictureModel> _logger;
        private readonly IImageService _imageService;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public ProfilePictureModel(UserManager<NuJournalUser> userManager,
                                   ILogger<ProfilePictureModel> logger,
                                   IImageService imageService,
                                   IUserService userService,
                                   ApplicationDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _imageService = imageService;
            _userService = userService;
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public bool DeleteProfilePictureCheckbox { get; set; }

        public UserInfo UserInfo { get; set; }

        public IFormFile ProfilePictureFile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            UserInfo = await _userService.GetUserInfoAsync(user);

            if (UserInfo == null)
            {
                StatusMessage = $"Error: Unable to load user info for user {user.UserName}!";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile profilePictureFile, bool deleteProfilePictureCheckbox)
        {
            StatusMessage = string.Empty;
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {                
                StatusMessage = "Error: Your profile picture was not updated.";                
            }
            else
            {
                if (deleteProfilePictureCheckbox)
                {
                    if (!_userService.DeleteProfilePicture(user))
                    {
                        ModelState.AddModelError("ProfilePictureFile", "Error: Unable to delete the user's avatar.");
                    }
                    else
                    {
                        StatusMessage = "Your profile picture has been deleted.";
                    }
                }
                else if (profilePictureFile != null)
                {
                    if (!await _userService.ChangeProfilePictureAsync(user, profilePictureFile))
                    {
                        StatusMessage = "Error: Unable to change your profile picture.";
                    }
                    else
                    {
                        StatusMessage = "Your profile picture has been updated";
                    }
                }
                else
                {
                    StatusMessage = "Error: Your profile picture was not updated.";
                    UserInfo = await _userService.GetUserInfoAsync(user);
                    return Page();
                }

                user.ModifiedByUser = user.UserName;
                user.ModifiedByRoles = user.UserRoles;
                user.Modified = DateTime.UtcNow;
                var userUpdateResult = await _userManager.UpdateAsync(user);

                if (!userUpdateResult.Succeeded)
                {
                    var profilePictureUpdateFailed = $"Error: Due to error {userUpdateResult.Errors.FirstOrDefault().Code}, your profile picture was not updated.";
                    StatusMessage = profilePictureUpdateFailed;
                    _logger.LogError(profilePictureUpdateFailed);
                }
            }

            UserInfo = await _userService.GetUserInfoAsync(user);
            return Page();
        }
    }
}
