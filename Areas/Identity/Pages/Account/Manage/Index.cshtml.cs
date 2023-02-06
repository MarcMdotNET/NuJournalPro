// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Net.WebSockets;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
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
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public UserInfo UserInput { get; set; }

        public IFormFile ProfilePictureFile { get; set; }

        public bool DeleteProfilePictureCheckbox { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            UserInput = await _userService.GetUserInfoAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostSaveProfileChangesAsync(UserInfo userInput)
        {            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                StatusMessage = "Error: Your profile information was not updated.";
            }
            else
            {
                if (userInput.FirstName == user.FirstName &&
                    userInput.MiddleName == user.MiddleName &&
                    userInput.LastName == user.LastName &&
                    userInput.DisplayName == user.DisplayName &&
                    userInput.PhoneNumber == user.PhoneNumber)
                {
                    StatusMessage = "Error: Your user profile information was not updated.";
                }
                else
                {
                    if (user.FirstName != userInput.FirstName)
                    {
                        user.FirstName = userInput.FirstName;
                    }
                    if (user.MiddleName != userInput.MiddleName)
                    {
                        user.MiddleName = userInput.MiddleName;
                    }
                    if (user.LastName != userInput.LastName)
                    {
                        user.LastName = userInput.LastName;
                    }
                    if (user.DisplayName != userInput.DisplayName)
                    {
                        var verifyDisplayNameResult = _userService.VerifyDisplayName(userInput.DisplayName, user);
                        if (verifyDisplayNameResult != string.Empty)
                        {
                            ModelState.AddModelError(UserInput.DisplayName, verifyDisplayNameResult);
                            UserInput = await _userService.GetUserInfoAsync(user);
                            return Page();
                        }
                        else
                        {
                            user.DisplayName = userInput.DisplayName;
                        }
                    }

                    var formatedInputPhoneNumber = _userService.FormatPhoneNumber(userInput.PhoneNumber);

                    if (user.PhoneNumber != formatedInputPhoneNumber)
                    {
                        user.PhoneNumber = formatedInputPhoneNumber;
                    }
                    
                    user.ModifiedByUser = user.UserName;
                    user.ModifiedByRoles = user.UserRoles;
                    user.Modified = DateTime.UtcNow;
                    var userUpdateResult = await _userManager.UpdateAsync(user);

                    if (!userUpdateResult.Succeeded)
                    {
                        StatusMessage = $"Error: Due to one or more errors, your user profile information was not saved.";

                        foreach (var error in userUpdateResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                            _logger.LogError(error.Description);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("User profile information has been updated.");
                    }

                    StatusMessage = "Your user profile information has been updated.";
                    return RedirectToPage();
                }
            }

            UserInput = await _userService.GetUserInfoAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfilePictureAsync(IFormFile profilePictureFile, bool deleteProfilePictureCheckbox)
        {            
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
                        StatusMessage = "Your profile picture has been updated.";
                    }
                }
                else
                {
                    StatusMessage = "Error: Your profile picture was not updated.";
                    UserInput = await _userService.GetUserInfoAsync(user);
                    return Page();
                }

                user.ModifiedByUser = user.UserName;
                user.ModifiedByRoles = user.UserRoles;
                user.Modified = DateTime.UtcNow;
                var userUpdateResult = await _userManager.UpdateAsync(user);

                if (!userUpdateResult.Succeeded)
                {
                    StatusMessage = $"Error: Due to one or more errors, your user profile information was not completely updated.";

                    foreach (var error in userUpdateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        _logger.LogError(error.Description);
                    }
                }
                else
                {
                    _logger.LogInformation("User profile picture has been updated.");
                }
            }

            UserInput = await _userService.GetUserInfoAsync(user);
            return Page();
        }
    }
}
