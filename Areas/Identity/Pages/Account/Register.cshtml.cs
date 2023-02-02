// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Models.Identity;
using NuJournalPro.Models.Settings;
using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<NuJournalUser> _signInManager;
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly IUserStore<NuJournalUser> _userStore;
        private readonly IUserEmailStore<NuJournalUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly DefaultUserSettings _defaultUserSettings;
        private readonly IUserService _userService;

        public RegisterModel(
            UserManager<NuJournalUser> userManager,
            IUserStore<NuJournalUser> userStore,
            SignInManager<NuJournalUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IOptions<DefaultUserSettings> defaultUserSettings,
            IUserService userService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _defaultUserSettings = defaultUserSettings.Value;
            _userService = userService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel : UserInputModel
        {
            [Required]
            [StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public new string Password { get; set; }
        }
        

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                if (_userService.IsDisplayNameForbidden(Input.DisplayName))
                {
                    ModelState.AddModelError("Input.DisplayName", $"The public display name {Input.DisplayName} is not allowed.");
                    return Page();
                }


                if (!_userService.IsDisplayNameUnique(Input.DisplayName))
                {
                    ModelState.AddModelError("Input.DisplayName", $"The {Input.DisplayName} public display name is already in use.");
                    return Page();
                }

                if (_userService.IsDisplayNameSimilar(Input.DisplayName))
                {
                    ModelState.AddModelError("Input.DisplayName", $"A similar public display name to {Input.DisplayName} already exists.");
                    return Page();
                }
                
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var defaultUserRole = _defaultUserSettings.Role ?? Environment.GetEnvironmentVariable("Role");
                
                if (defaultUserRole == null || defaultUserRole == string.Empty)
                {
                    user.UserRoles.Add(NuJournalUserRole.Reader.ToString());
                }
                else
                {
                    foreach (var userRole in Enum.GetNames(typeof(NuJournalUserRole)))
                    {
                        if (userRole == defaultUserRole)
                        {
                            user.UserRoles.Add(defaultUserRole);
                        }
                    }
                }

                if (user.UserRolesString == null || user.UserRolesString == string.Empty)
                {
                    user.UserRoles.Add(NuJournalUserRole.Reader.ToString());
                }

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    IdentityResult addRoleToUserResult = new();

                    foreach (var userRole in Enum.GetNames(typeof(NuJournalUserRole)))
                    {
                        if (userRole == defaultUserRole)
                        {
                            addRoleToUserResult = await _userManager.AddToRoleAsync(user, userRole);
                        }
                    }

                    if (!addRoleToUserResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, NuJournalUserRole.Reader.ToString());
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private NuJournalUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<NuJournalUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(NuJournalUser)}'. " +
                    $"Ensure that '{nameof(NuJournalUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<NuJournalUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<NuJournalUser>)_userStore;
        }
    }
}
