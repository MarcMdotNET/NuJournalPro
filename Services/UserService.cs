using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NuJournalPro.Data;
using NuJournalPro.Enums;
using NuJournalPro.Models.Database;
using NuJournalPro.Models.Identity;
using NuJournalPro.Models.Settings;
using NuJournalPro.Services.Interfaces;
using System.Text.RegularExpressions;

namespace NuJournalPro.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly IImageService _imageService;
        private readonly DefaultUserSettings _defaultUserSettings;
        private readonly DefaultGraphics _defaultGraphics;
        private readonly ApplicationDbContext _dbContext;

        public UserService(UserManager<NuJournalUser> userManager,
                                       IImageService imageService,
                                       IOptions<DefaultUserSettings> defaultUserSettings,
                                       IOptions<DefaultGraphics> defaultGraphics,
                                       ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _imageService = imageService;
            _defaultUserSettings = defaultUserSettings.Value;
            _defaultGraphics = defaultGraphics.Value;
            _dbContext = dbContext;
        }

        public async Task<NuJournalUser> CreateNewUserAsync(UserInputModel userModel, UserInfo? parentUserInfo = null, IFormFile? userProfilePictureFile = null)
        {
            NuJournalUser newUser = Activator.CreateInstance<NuJournalUser>();
            newUser.UserName = userModel.Email;
            newUser.Email = userModel.Email;
            newUser.FirstName = userModel.FirstName;
            newUser.MiddleName = userModel.MiddleName;
            newUser.LastName = userModel.LastName;
            newUser.DisplayName = userModel.DisplayName;
            newUser.PhoneNumber = userModel.PhoneNumber;

            if (parentUserInfo != null)
            {
                newUser.CreatedByUser = parentUserInfo.UserName;
                newUser.CreatedByRoles = parentUserInfo.UserRoles;
            }
            else newUser.CreatedByUser = "User Service"; // In this case the User Role(s) will be added automatically by the NuJournalUser data model.

            if (userProfilePictureFile != null) newUser.ProfilePicture = (ProfilePicture)await _imageService.CreateCompressedImageAsync(userProfilePictureFile);

            return newUser;
        }

        public async Task<string> GetAccessDeniedImageAsync()
        {
            return _imageService.DecodeImage(await _imageService.EncodeImageDataAsync(_defaultGraphics.SecureAccess), _imageService.GetImageMimeType(_defaultGraphics.SecureAccess));
        }

        public async Task<ProfilePicture> GetDefaultProfilePictureAsync()
        {
            return (ProfilePicture)await _imageService.CreateCompressedImageAsync(_defaultUserSettings.ProfilePicture);
        }

        public async Task<UserInfo> GetUserInfoAsync(NuJournalUser user)
        {
            return new UserInfo()
            {
                UserName = await _userManager.GetUserNameAsync(user),
                UserRoles = await _userManager.GetRolesAsync(user) as List<string>
            };
        }

        public bool IsAdmin(NuJournalUser user)
        {
            if (user.UserRolesString.Contains(NuJournalUserRole.Administrator.ToString()) && !user.UserRolesString.Contains(NuJournalUserRole.Owner.ToString())) return true;
            else return false;
        }

        public bool IsOwner(NuJournalUser user)
        {
            if (user.UserRolesString.Contains(NuJournalUserRole.Owner.ToString())) return true;
            else return false;
        }

        public bool IsAdministration(NuJournalUser user)
        {
            if (!user.UserRolesString.Contains(NuJournalUserRole.Owner.ToString()) && !user.UserRolesString.Contains(NuJournalUserRole.Administrator.ToString())) return false;
            else return true;
        }

        public bool IsDisplayNameUnique(string displayName)
        {
            foreach (var appUser in _userManager.Users.ToList())
            {
                if (displayName.ToUpper() == appUser.DisplayName.ToUpper()) return false;
            }
            return true;
        }

        public bool IsDisplayNameSimilar(string displayName)
        {
            foreach (var appUser in _userManager.Users.ToList())
            {
                if (Regex.Replace(displayName.ToUpper(), @"[^0-9a-zA-Z]+", "") == Regex.Replace(appUser.DisplayName.ToUpper(), @"[^0-9a-zA-Z]+", "")) return true;
            }
            return false;
        }

        public bool IsDisplayNameForbidden(string displayName)
        {
            foreach (var notAllowed in Enum.GetValues(typeof(ForbidenDisplayName)).Cast<ForbidenDisplayName>().ToList())
            {
                if (displayName.ToUpper().Contains(notAllowed.ToString().ToUpper())) return true;
            }
            return false;
        }

        public string GenerateRandomPassword(PasswordOptions pwdOptions = null)
        {
            if (pwdOptions == null)
            {
                pwdOptions = new PasswordOptions()
                {
                    RequiredLength = 16,
                    RequiredUniqueChars = 4,
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireNonAlphanumeric = true,
                    RequireUppercase = true
                };
            }

            string[] characterPool = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",
                "abcdefghijkmnopqrstuvwxyz",
                "0123456789",
                "!@$?_-"
            };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (pwdOptions.RequireUppercase) chars.Insert(rand.Next(0, chars.Count), characterPool[0][rand.Next(0, characterPool[0].Length)]);
            if (pwdOptions.RequireLowercase) chars.Insert(rand.Next(0, chars.Count), characterPool[1][rand.Next(0, characterPool[1].Length)]);
            if (pwdOptions.RequireDigit) chars.Insert(rand.Next(0, chars.Count), characterPool[2][rand.Next(0, characterPool[2].Length)]);
            if (pwdOptions.RequireNonAlphanumeric) chars.Insert(rand.Next(0, chars.Count), characterPool[3][rand.Next(0, characterPool[3].Length)]);

            for (int i = chars.Count; i < pwdOptions.RequiredLength || chars.Distinct().Count() < pwdOptions.RequiredUniqueChars; i++)
            {
                string rcs = characterPool[rand.Next(0, characterPool.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public async Task<UserInputModel> GetExistingUserInputAsync(NuJournalUser existingUser)
        {
            if (existingUser is null) return null;
            else
            {
                return new UserInputModel()
                {
                    FirstName = existingUser.FirstName,
                    MiddleName = existingUser.MiddleName,
                    LastName = existingUser.LastName,
                    DisplayName = existingUser.DisplayName,
                    Email = existingUser.Email,
                    PhoneNumber = existingUser.PhoneNumber,
                    ProfilePicture = await GetProfilePictureAsync(existingUser),
                    Joined = existingUser.Joined.ToLocalTime(),
                    CreatedByUser = existingUser.UserName.Equals(existingUser.CreatedByUser) ? "User Registration" : existingUser.CreatedByUser,
                    CreatedByRoles = existingUser.CreatedByRoles,
                    Modified = existingUser.Modified.ToLocalTime(),
                    ModifiedByUser = existingUser.ModifiedByUser,
                    ModifiedByRoles = existingUser.ModifiedByRoles
                };
            }
        }

        public List<NuJournalUser> GetAppUserList(NuJournalUser user) // Needs fixing.
        {
            if (IsOwner(user))
            {
                var userList = new List<NuJournalUser>();
                userList = _userManager.Users.Cast<NuJournalUser>()
                                         .Where(u => !u.UserName.Equals(user.UserName))
                                         .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Owner.ToString()))
                                         .OrderBy(r => r.UserRoles)
                                         .OrderBy(n => n.LastName)
                                         .OrderBy(n => n.FirstName)
                                         .OrderBy(n => n.DisplayName)
                                         .OrderBy(e => e.Email)
                                         .ToList();
                return userList;
            }
            else if (!IsOwner(user) && IsAdmin(user))
            {
                var userList = new List<NuJournalUser>();
                userList = _userManager.Users.Cast<NuJournalUser>()
                                         .Where(u => !u.UserName.Equals(user.UserName))
                                         .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Owner.ToString()))
                                         .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Administrator.ToString()))
                                         .OrderBy(r => r.UserRoles)
                                         .OrderBy(n => n.LastName)
                                         .OrderBy(n => n.FirstName)
                                         .OrderBy(n => n.DisplayName)
                                         .OrderBy(e => e.Email)
                                         .ToList();
                return userList;
            }
            else return null;
        }

        public async Task<ProfilePicture> GetProfilePictureAsync(NuJournalUser user)
        {
            if (user is null) return null;
            else
            {
                var profilePicture = _dbContext.ProfilePicture.FirstOrDefault(u => u.NuJournalUserId == user.Id);
                if (profilePicture != null) return profilePicture;
                else return await GetDefaultProfilePictureAsync();
            }
        }

        public async Task<bool> ChangeProfilePictureAsync(NuJournalUser user, IFormFile newUserProfilePictureFile)
        {
            if (user is null || newUserProfilePictureFile is null) return false;
            else
            {
                var newUserProfilePicture = await _imageService.CreateCompressedImageAsync(newUserProfilePictureFile);
                var oldUserProfilePicture = _dbContext.ProfilePicture.FirstOrDefault(u => u.NuJournalUserId == user.Id);
                if (oldUserProfilePicture is null) return false;
                else
                {
                    oldUserProfilePicture.CompressedImageData = newUserProfilePicture.CompressedImageData;
                    oldUserProfilePicture.ImageMimeType = newUserProfilePicture.ImageMimeType;
                    oldUserProfilePicture.ImageSize = newUserProfilePicture.ImageSize;
                    if (_dbContext.SaveChanges() > 0) return true;
                    else return false;
                }
            }
        }

        public async Task<bool> DeleteProfilePictureAsync(NuJournalUser user)
        {
            if (user is null) return false;
            else
            {
                var defaultUserProfilePicture = await GetDefaultProfilePictureAsync();
                var oldUserProfilePicture = _dbContext.ProfilePicture.FirstOrDefault(u => u.NuJournalUserId == user.Id);
                if (oldUserProfilePicture is null) return false;
                else
                {
                    oldUserProfilePicture.CompressedImageData = defaultUserProfilePicture.CompressedImageData;
                    oldUserProfilePicture.ImageMimeType = defaultUserProfilePicture.ImageMimeType;
                    oldUserProfilePicture.ImageSize = defaultUserProfilePicture.ImageSize;
                    if (_dbContext.SaveChanges() > 0) return true;
                    else return false;
                }
            }
        }

        public async Task<bool> RemoveUserAccountAsync(NuJournalUser user) // Needs fixing.
        {
            if (user == null) return false;
            else
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded) return true;
                else return false;
            }
        }
    }
}
