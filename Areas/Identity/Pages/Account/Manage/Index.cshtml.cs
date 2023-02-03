// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuJournalPro.Models;
using NuJournalPro.Models.Identity;
using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class PersonalProfileModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly SignInManager<NuJournalUser> _signInManager;
        private readonly ILogger<PersonalProfileModel> _logger;
        private readonly IUserService _userService;

        public PersonalProfileModel(
            UserManager<NuJournalUser> userManager,
            SignInManager<NuJournalUser> signInManager,
            ILogger<PersonalProfileModel> logger,
            IUserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _userService = userService;            
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public UserInfo UserInfo { get; set; }        

        public IFormFile ProfilePictureFile { get; set; }
        
        public bool DeleteProfilePictureCheckbox { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            StatusMessage = string.Empty;
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            UserInfo = await _userService.GetUserInfoAsync(user);

            return Page();
        }
        
        public async Task<IActionResult> OnPostSaveProfileChangesAsync()
        {
            StatusMessage = string.Empty;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            
            


            if (!ModelState.IsValid)
            {
                UserInfo = await _userService.GetUserInfoAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (UserInfo.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, UserInfo.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateProfilePictureAsync(IFormFile profilePictureFile, bool deleteProfilePictureCheckbox)
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
