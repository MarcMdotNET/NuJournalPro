// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly SignInManager<NuJournalUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly IUserService _userService;

        public DeletePersonalDataModel(
            UserManager<NuJournalUser> userManager,
            SignInManager<NuJournalUser> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            IUserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _userService = userService;
        }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public string AccessDeniedImage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!_userService.IsOwner(user))
            {
                RequirePassword = await _userManager.HasPasswordAsync(user);
            }
            else
            {
                AccessDeniedImage = await _userService.GetAccessDeniedImageAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (_userService.IsOwner(user))
            {
                AccessDeniedImage = await _userService.GetAccessDeniedImageAsync();
                return Page();
            }
            else
            {
                RequirePassword = await _userManager.HasPasswordAsync(user);
                if (RequirePassword)
                {
                    if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                    {
                        ModelState.AddModelError(string.Empty, "Incorrect password.");
                        return Page();
                    }
                }

                var userId = await _userManager.GetUserIdAsync(user);
                var userRole = new NuJournalUserRole();
                userRole = await _userService.GetDefaultUserRoleAsync(user);
                var roleRemovalResult = await _userManager.RemoveFromRoleAsync(user, userRole.ToString());

                if (!roleRemovalResult.Succeeded)
                {
                    StatusMessage = $"Error: Unable to make changes to user {user}.";
                    return Page();
                }

                var roleAdditionResult = await _userManager.AddToRoleAsync(user, NuJournalUserRole.Deleted.ToString());
                if (!roleAdditionResult.Succeeded)
                {
                    StatusMessage = $"Error: Unable to delete user {user}.";
                    return Page();
                }

                var deletedUserRoles = new List<string>();
                deletedUserRoles.Add(NuJournalUserRole.Deleted.ToString());
                user.UserRoles = deletedUserRoles;

                var deleteUserUpdateResult = await _userManager.UpdateAsync(user);

                if (deleteUserUpdateResult.Succeeded)
                {
                    _logger.LogInformation($"User {user} with ID {userId} successfully deleted themselves.", userId);
                }
                else
                {
                    foreach (var error in deleteUserUpdateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        _logger.LogError(error.Description);
                    }

                    StatusMessage = $"Error: Unable to delete user {user}.";
                }

                await _signInManager.SignOutAsync();

                return Redirect("~/");
            }
        }
    }
}
