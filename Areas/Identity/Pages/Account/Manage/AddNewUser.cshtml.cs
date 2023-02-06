#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using NuJournalPro.Enums;
using NuJournalPro.Models;
using NuJournalPro.Models.Identity;
using NuJournalPro.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class AddNewUserModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;        
        private readonly ILogger<AddNewUserModel> _logger;
        private readonly IUserEmailStore<NuJournalUser> _newUserEmailStore;
        private readonly IUserStore<NuJournalUser> _newUserStore;
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;

        public AddNewUserModel(UserManager<NuJournalUser> userManager,
                               ILogger<AddNewUserModel> logger,
                               IUserStore<NuJournalUser> newUserStore,
                               IUserService userService,
                               IEmailSender emailSender)
        {
            _userManager = userManager;
            _logger = logger;
            _newUserStore = newUserStore;
            _newUserEmailStore = GetEmailStore();
            _userService = userService;
            _emailSender = emailSender;
        }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public bool EmailConfirmedCheckbox { get; set; }

        public string AccessDeniedImage { get; set; }

        [Display(Name = "Select User Role")]
        public NuJournalUserRole NewUserRole { get; set; } = NuJournalUserRole.Reader;

        [BindProperty]
        public InputModel NewUserInput { get; set; }

        public class InputModel : UserInputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public new string Password { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var administrativeUser = await _userManager.GetUserAsync(User);            
            if (administrativeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (_userService.IsAdministration(administrativeUser))
            {
                NewUserInput = new InputModel
                {
                    ProfilePicture = await _userService.GetDefaultProfilePictureAsync()
                };

                ViewData["UserRolesList"] = _userService.CreateUserRolesList(administrativeUser);
                NewUserRole = NuJournalUserRole.Reader;
            }
            else
            {
                AccessDeniedImage = await _userService.GetAccessDeniedImageAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(NuJournalUserRole newUserRole, bool emailConfirmedCheckbox)
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
                    var newUser = await _userService.CreateUserAsync(NewUserInput, administrativeUser, NewUserInput.ImageFile, emailConfirmedCheckbox);

                    var verifyDisplayNameResult = _userService.VerifyDisplayName(NewUserInput.DisplayName);

                    if (verifyDisplayNameResult != string.Empty)
                    {
                        ModelState.AddModelError("NewUserInput.DisplayName", verifyDisplayNameResult);
                        return Page();
                    }

                    NewUserInput.PhoneNumber = _userService.FormatPhoneNumber(NewUserInput.PhoneNumber);

                    await _newUserStore.SetUserNameAsync(newUser, NewUserInput.Email, CancellationToken.None);
                    await _newUserEmailStore.SetEmailAsync(newUser, NewUserInput.Email, CancellationToken.None);

                    if (newUserRole.Equals(NuJournalUserRole.Owner))
                    {
                        newUserRole = NuJournalUserRole.Reader;
                    }

                    newUser.UserRoles.Add(newUserRole.ToString());

                    var userCreationResult = await _userManager.CreateAsync(newUser, NewUserInput.Password);

                    if (userCreationResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(newUser, newUserRole.ToString());

                        _logger.LogInformation($"User {administrativeUser.UserName} created a new account with the username {newUser.UserName} and assigned this account the {newUserRole.ToString()} role.");

                        StatusMessage = $"User {newUser.UserName} with the role {newUserRole.ToString()} has been successfully created.";

                        if (!emailConfirmedCheckbox)
                        {
                            var returnUrl = Url.Content("~/");
                            var newUserId = await _userManager.GetUserIdAsync(newUser);
                            var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                            var callbackUrl = Url.Page("/Account/ConfirmEmail", pageHandler: null, values: new { area = "Identity", userId = newUserId, code = code, returnUrl = returnUrl }, protocol: Request.Scheme);

                            await _emailSender.SendEmailAsync(newUser.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                        }

                        return RedirectToPage();
                    }
                    else
                    {
                        foreach (var error in userCreationResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                            _logger.LogError(error.Description);
                        }
                    }
                }

                NewUserInput.ProfilePicture = await _userService.GetDefaultProfilePictureAsync();
            }
            else
            {
                AccessDeniedImage = await _userService.GetAccessDeniedImageAsync();
            }
            
            return Page();
        }

        private IUserEmailStore<NuJournalUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<NuJournalUser>)_newUserStore;
        }
    }
}
