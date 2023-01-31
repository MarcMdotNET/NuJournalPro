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

        public async Task<NuJournalUser> CreateNewUserAsync(UserInputModel userModel, UserInfo? parentUserInfo = null, IFormFile? profilePictureFile = null)
        {
            NuJournalUser newUser = Activator.CreateInstance<NuJournalUser>();
            if (userModel.Email != null)
            {
                newUser.UserName = userModel.Email;
                newUser.Email = userModel.Email;
            }
            else throw new ArgumentNullException(nameof(newUser.UserName));
            if (userModel.FirstName != null) newUser.FirstName = userModel.FirstName;
            if (userModel.MiddleName != null) newUser.MiddleName = userModel.MiddleName;
            if (userModel.LastName != null) newUser.LastName = userModel.LastName;
            if (userModel.DisplayName != null) newUser.DisplayName = userModel.DisplayName;
            if (userModel.PhoneNumber != null) newUser.PhoneNumber = userModel.PhoneNumber;

            if (parentUserInfo != null)
            {
                if (parentUserInfo.UserName != null) newUser.CreatedByUser = parentUserInfo.UserName;
                if (parentUserInfo.UserRoles != null) newUser.CreatedByRoles = parentUserInfo.UserRoles;
            }
            else newUser.CreatedByUser = "User Service"; // In this case the User Role(s) will be added automatically by the NuJournalUser data model.

            if (profilePictureFile != null) newUser.ProfilePicture = (ProfilePicture?)await _imageService.CreateCompressedImageAsync(profilePictureFile);

            return newUser;
        }

        public async Task<string?> GetAccessDeniedImageAsync()
        {
            if (_defaultGraphics.SecureAccess == null)
            {
                string defaultPath = "/appresources/default/img/";
                string defaultImage = "AccessDenied.svg";
                var defaultEncodedImageData = await _imageService.EncodeImageDataAsync(defaultImage, false, defaultPath);
                var defaultImageMimeType = _imageService.GetImageMimeType(defaultImage);
                if (defaultEncodedImageData != null && defaultImageMimeType != null)
                {
                    var defaultDecodedImage = _imageService.DecodeImage(defaultEncodedImageData, defaultImageMimeType);
                    if (defaultDecodedImage != null) return defaultDecodedImage;
                    else return null;
                }
                else return null;
            }
            else
            {
                var encodedImageData = await _imageService.EncodeImageDataAsync(_defaultGraphics.SecureAccess);
                var imageMimeType = _imageService.GetImageMimeType(_defaultGraphics.SecureAccess);
                if (encodedImageData != null && imageMimeType != null)
                {
                    var decodedImage = _imageService.DecodeImage(encodedImageData, imageMimeType);
                    if (decodedImage != null) return decodedImage;
                    else return null;
                }
                else return null;
            }
        }

        public async Task<ProfilePicture?> GetDefaultProfilePictureAsync()
        {
            if (_defaultUserSettings.ProfilePicture == null)
            {
                string defaultPath = "/appresources/default/img/";
                string defaultImage = "ProfilePicture.svg";
                return (ProfilePicture?)await _imageService.CreateCompressedImageAsync(defaultImage, defaultPath);
            }
            else return (ProfilePicture?)await _imageService.CreateCompressedImageAsync(_defaultUserSettings.ProfilePicture);
        }

        public async Task<UserInfo?> GetUserInfoAsync(NuJournalUser user)
        {
            if (user != null)
            {
                return new UserInfo()
                {
                    UserName = await _userManager.GetUserNameAsync(user),
                    UserRoles = await _userManager.GetRolesAsync(user) as List<string>
                };
            }
            else return null;
        }

        public bool IsAdmin(NuJournalUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            else
            {
                if (user.UserRolesString.Contains(NuJournalUserRole.Administrator.ToString()) && !user.UserRolesString.Contains(NuJournalUserRole.Owner.ToString())) return true;
                else return false;
            }
        }

        public bool IsOwner(NuJournalUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            else
            {
                if (user.UserRolesString.Contains(NuJournalUserRole.Owner.ToString())) return true;
                else return false;
            }
        }

        public bool IsAdministration(NuJournalUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            else
            {
                if (!user.UserRolesString.Contains(NuJournalUserRole.Owner.ToString()) && !user.UserRolesString.Contains(NuJournalUserRole.Administrator.ToString())) return false;
                else return true;
            }
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

        public string GenerateRandomPassword(PasswordOptions? pwdOptions = null)
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

        public async Task<UserInputModel?> GetExistingUserInputAsync(NuJournalUser existingUser)
        {
            if (existingUser == null) return null;
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

        public List<NuJournalUser>? GetAppUserList(NuJournalUser user)
        {
            if (user == null) return null;
            else
            {
                var userList = new List<NuJournalUser>();
                if (IsOwner(user))
                {
                    userList = _userManager.Users.Cast<NuJournalUser>()
                         .Where(u => !u.UserName.Equals(user.UserName))
                         .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Owner.ToString()))
                         .OrderBy(r => r.UserRoles)
                         .OrderBy(n => n.LastName)
                         .OrderBy(n => n.FirstName)
                         .OrderBy(n => n.DisplayName)
                         .OrderBy(e => e.Email)
                         .ToList();
                }
                else if (!IsOwner(user) && IsAdmin(user))
                {
                    userList = _userManager.Users.Cast<NuJournalUser>()
                         .Where(u => !u.UserName.Equals(user.UserName))
                         .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Owner.ToString()))
                         .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Administrator.ToString()))
                         .Where(u => !u.UserRoles.Contains(NuJournalUserRole.Deleted.ToString()))
                         .OrderBy(r => r.UserRoles)
                         .OrderBy(n => n.LastName)
                         .OrderBy(n => n.FirstName)
                         .OrderBy(n => n.DisplayName)
                         .OrderBy(e => e.Email)
                         .ToList();
                }
                else return null;

                if (userList.Count > 0) return userList;
                else return null;
            }
        }

        public async Task<ProfilePicture?> GetProfilePictureAsync(NuJournalUser user)
        {
            if (user == null) return null;
            else
            {
                var profilePicture = _dbContext.ProfilePicture?.FirstOrDefault(u => u.NuJournalUserId == user.Id);
                if (profilePicture != null) return profilePicture;
                else return await GetDefaultProfilePictureAsync();
            }
        }

        public async Task<bool> ChangeProfilePictureAsync(NuJournalUser user, IFormFile newProfilePictureFile)
        {
            if (user == null || newProfilePictureFile == null) return false;
            else
            {
                var newUserProfilePicture = await _imageService.CreateCompressedImageAsync(newProfilePictureFile);
                var oldUserProfilePicture = _dbContext.ProfilePicture?.FirstOrDefault(u => u.NuJournalUserId == user.Id);
                if (newUserProfilePicture == null || oldUserProfilePicture == null) return false;
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

        public bool DeleteProfilePicture(NuJournalUser user)
        {
            if (user is null) return false;
            else
            {
                var userProfilePicture = _dbContext.ProfilePicture?.FirstOrDefault(u => u.NuJournalUserId == user.Id);
                if (userProfilePicture == null) return false;
                else
                {
                    _dbContext.ProfilePicture?.Remove(userProfilePicture);
                    if (_dbContext.SaveChanges() > 0) return true;
                    else return false;
                }
            }
        }

        public bool DeleteUserAccount(NuJournalUser user)
        {
            if (user == null) return false;
            else
            {
                // Remove any user roles from user.
                foreach (var userRole in user.UserRoles.ToList())
                {
                    user.UserRoles.Remove(userRole);
                }
                user.UserRoles.Add(NuJournalUserRole.Deleted.ToString());
                return true;
            }
        }
        
        public async Task<bool> RemoveUserAccountAsync(NuJournalUser user)
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
