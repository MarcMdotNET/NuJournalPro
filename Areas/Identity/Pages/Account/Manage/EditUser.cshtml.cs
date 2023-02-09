#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using NuJournalPro.Data;
using NuJournalPro.Models.Database;
using NuJournalPro.Models.Identity;
using NuJournalPro.Models.Settings;
using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using NuJournalPro.Enums;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class EditUserModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly SignInManager<NuJournalUser> _signInManager;
        private readonly ILogger<EditUserModel> _logger;
        private readonly DefaultUserSettings _defaultUserSettings;
        private readonly DefaultGraphics _defaultGraphics;
        private readonly IImageService _imageService;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public EditUserModel(UserManager<NuJournalUser> userManager,
                                     SignInManager<NuJournalUser> signInManager,
                                     ILogger<EditUserModel> logger,
                                     IOptions<DefaultUserSettings> defaultUserSettings,
                                     IOptions<DefaultGraphics> defaultGraphics,
                                     IImageService imageService,
                                     IUserService userService,
                                     ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _defaultUserSettings = defaultUserSettings.Value;
            _defaultGraphics = defaultGraphics.Value;
            _imageService = imageService;
            _userService = userService;
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public string AccessDeniedImage { get; set; }

        public bool UserFormVisibility { get; set; } = true;

        [Display(Name = "Select User Role")]
        public NuJournalUserRole SelectedUserRole { get; set; }

        public bool DeleteProfilePictureCheckbox { get; set; }

        public List<NuJournalUser> AppUserList { get; set; } = new List<NuJournalUser>();

        [Display(Name = "Select a user to edit")]
        public string SelectedUserEmail { get; set; }

        [BindProperty]
        public UserInputModel SelectedUserInput { get; set; } = new UserInputModel();

        public async Task<IActionResult> OnGetAsync()
        {           
            UserFormVisibility = false;

            var administrativeUser = await _userManager.GetUserAsync(User);
            if (administrativeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (_userService.IsAdministration(administrativeUser))
            {
                AppUserList = _userService.GetAppUserList(administrativeUser, true);
                if (AppUserList != null)
                {
                    ViewData["SelectUserList"] = new SelectList(AppUserList, "Email", "UserNameWithDetails", AppUserList.FirstOrDefault().Email);
                    SelectedUserEmail = AppUserList.FirstOrDefault().Email;
                    SelectedUserInput.ProfilePicture = await _userService.GetDefaultProfilePictureAsync();
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

        public async Task<IActionResult> OnPostSelectUserAsync(string selectedUserEmail)
        {            
            var administrativeUser = await _userManager.GetUserAsync(User);
            if (administrativeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (_userService.IsAdministration(administrativeUser))
            {
                var selectedUser = await _userManager.FindByEmailAsync(selectedUserEmail);

                AppUserList = _userService.GetAppUserList(administrativeUser, true);

                if (AppUserList != null)
                {
                    ViewData["SelectUserList"] = new SelectList(AppUserList, "Email", "UserNameWithDetails");
                    SelectedUserInput = await _userService.GetUserInputAsync(selectedUser);
                    var selectedUserRole = new NuJournalUserRole();
                    selectedUserRole = await _userService.GetDefaultUserRoleAsync(selectedUser);
                    if (selectedUserRole.Equals(NuJournalUserRole.Deleted))
                    {
                        ViewData["UserRolesList"] = _userService.CreateUserRolesList(administrativeUser, true);
                    }
                    else
                    {
                        ViewData["UserRolesList"] = _userService.CreateUserRolesList(administrativeUser);
                    }                    
                    SelectedUserRole = await _userService.GetDefaultUserRoleAsync(selectedUser);
                    UserFormVisibility = true;
                }
                else
                {                    
                    UserFormVisibility = false;
                }
            }
            else
            {
                AccessDeniedImage = await _userService.GetAccessDeniedImageAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostModifyUserAsync(UserInputModel selectedUserInput, NuJournalUserRole selectedUserRole, bool deleteProfilePictureCheckbox)
        {
            StatusMessage = string.Empty;

            var administrativeUser = await _userManager.GetUserAsync(User);
            if (administrativeUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (_userService.IsAdministration(administrativeUser))
            {
                var selectedUser = await _userManager.FindByEmailAsync(selectedUserInput.Email);
                
                ViewData["UserRolesList"] = _userService.CreateUserRolesList(administrativeUser);

                AppUserList = _userService.GetAppUserList(administrativeUser, true);

                if (AppUserList != null)
                {
                    ViewData["SelectUserList"] = new SelectList(AppUserList, "Email", "UserNameWithDetails", selectedUserInput.Email);
                }

                if (!ModelState.IsValid)
                {
                    StatusMessage = "Error: The user profile was not updated.";
                }
                else
                {
                    var currentSelectedUserRole = await _userService.GetDefaultUserRoleAsync(selectedUser);

                    if (selectedUserInput.Email == selectedUser.Email &&
                        selectedUserInput.FirstName == selectedUser.FirstName &&
                        selectedUserInput.MiddleName == selectedUser.MiddleName &&
                        selectedUserInput.LastName == selectedUser.LastName &&
                        selectedUserInput.DisplayName == selectedUser.DisplayName &&
                        selectedUserInput.PhoneNumber == selectedUser.PhoneNumber &&
                        selectedUserInput.Password == null &&
                        selectedUserRole == currentSelectedUserRole &&
                        !deleteProfilePictureCheckbox)
                    {
                        StatusMessage = "Error: No changes were made to the user profile.";
                    }
                    else
                    {
                        if (selectedUser.Email != selectedUserInput.Email)
                        {
                            if (await _userManager.FindByEmailAsync(selectedUserInput.Email) == null)
                            {
                                selectedUser.Email = selectedUserInput.Email;
                            }
                            else
                            {
                                ModelState.AddModelError(SelectedUserInput.Email, $"Error: The email address {selectedUserInput.Email} is already in use.");
                                SelectedUserInput = await _userService.GetUserInputAsync(selectedUser);
                                SelectedUserRole = await _userService.GetDefaultUserRoleAsync(selectedUser);
                                return Page();
                            }
                        }

                        if (selectedUser.FirstName != selectedUserInput.FirstName)
                        {
                            selectedUser.FirstName = selectedUserInput.FirstName;
                        }

                        if (selectedUser.MiddleName != selectedUserInput.MiddleName)
                        {
                            selectedUser.MiddleName = selectedUserInput.MiddleName;
                        }

                        if (selectedUser.LastName != selectedUserInput.LastName)
                        {
                            selectedUser.LastName = selectedUserInput.LastName;
                        }

                        if (selectedUser.DisplayName != selectedUserInput.DisplayName)
                        {
                            var verifyDisplayNameResult = _userService.VerifyDisplayName(selectedUserInput.DisplayName);

                            if (verifyDisplayNameResult != string.Empty)
                            {
                                ModelState.AddModelError("NewUserInput.DisplayName", verifyDisplayNameResult);
                                SelectedUserInput = await _userService.GetUserInputAsync(selectedUser);
                                SelectedUserRole = await _userService.GetDefaultUserRoleAsync(selectedUser);
                                return Page();
                            }
                            else
                            {
                                selectedUser.DisplayName = selectedUserInput.DisplayName;
                            }
                        }

                        var formatedInputPhoneNumber = _userService.FormatPhoneNumber(selectedUserInput.PhoneNumber);

                        if (selectedUser.PhoneNumber != formatedInputPhoneNumber)
                        {
                            selectedUser.PhoneNumber = formatedInputPhoneNumber;
                        }

                        if (currentSelectedUserRole != selectedUserRole)
                        {
                            var roleRemovalResult = await _userManager.RemoveFromRoleAsync(selectedUser, currentSelectedUserRole.ToString());
                            if (!roleRemovalResult.Succeeded)
                            {
                                ModelState.AddModelError("SelectedUserRole", $"Error: Unable to remove user from the {currentSelectedUserRole.ToString()} role.");
                                SelectedUserInput = await _userService.GetUserInputAsync(selectedUser);
                                SelectedUserRole = await _userService.GetDefaultUserRoleAsync(selectedUser);
                                return Page();
                            }

                            var roleAdditionResult = await _userManager.AddToRoleAsync(selectedUser, selectedUserRole.ToString());
                            if (!roleAdditionResult.Succeeded)
                            {
                                ModelState.AddModelError("SelectedUserRole", $"Error: Unable to add user from the {selectedUserRole.ToString()} role.");
                                SelectedUserInput = await _userService.GetUserInputAsync(selectedUser);
                                SelectedUserRole = await _userService.GetDefaultUserRoleAsync(selectedUser);
                                return Page();
                            }

                            var newUserRoles = new List<string>();
                            newUserRoles.Add(selectedUserRole.ToString());
                            selectedUser.UserRoles = newUserRoles;
                        }

                        if (deleteProfilePictureCheckbox)
                        {
                            if (_userService.DeleteProfilePicture(selectedUser) == false)
                            {
                                ModelState.AddModelError("SelectedUserInput.ImageFile", "Error: Unable to delete the user's profile picture.");
                                SelectedUserInput = await _userService.GetUserInputAsync(selectedUser);
                                SelectedUserRole = await _userService.GetDefaultUserRoleAsync(selectedUser);
                                return Page();
                            }
                            SelectedUserInput.ProfilePicture = await _userService.GetDefaultProfilePictureAsync();
                        }
                        else if (selectedUserInput.ImageFile != null)
                        {
                            if (!await _userService.ChangeProfilePictureAsync(selectedUser, SelectedUserInput.ImageFile))
                            {
                                ModelState.AddModelError("SelectedUserInput.ImageFile", "Error: Unable to change the user's profile picture.");
                                SelectedUserInput = await _userService.GetUserInputAsync(selectedUser);
                                SelectedUserRole = await _userService.GetDefaultUserRoleAsync(selectedUser);
                                return Page();
                            }
                            SelectedUserInput.ProfilePicture = await _userService.CreateProfilePictureAsync(SelectedUserInput.ImageFile);
                        }

                        selectedUser.ModifiedByUser = administrativeUser.UserName;
                        selectedUser.ModifiedByRoles = administrativeUser.UserRoles;
                        selectedUser.Modified = DateTime.UtcNow;
                        var selectedUserUpdateResult = await _userManager.UpdateAsync(selectedUser);

                        if (!selectedUserUpdateResult.Succeeded)
                        {
                            StatusMessage = $"Error: Due to one or more errors, the user's profile was not updated.";

                            foreach (var error in selectedUserUpdateResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                                _logger.LogError(error.Description);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("The user's profile was updated successfully.");
                        }

                        StatusMessage = "The user's profile was updated successfully.";
                    }
                }

                SelectedUserInput = await _userService.GetUserInputAsync(selectedUser);
                SelectedUserRole = await _userService.GetDefaultUserRoleAsync(selectedUser);
            }
            else
            {
                AccessDeniedImage = await _userService.GetAccessDeniedImageAsync();
            }

            return Page();
        }
    }
}
