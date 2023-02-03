using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NuJournalPro.Data;
using NuJournalPro.Enums;
using NuJournalPro.Models;
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


        public bool IsAdmin(NuJournalUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            else
            {
                if (user.UserRolesString.Contains(NuJournalUserRole.Administrator.ToString()) && !user.UserRolesString.Contains(NuJournalUserRole.Owner.ToString()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsOwner(NuJournalUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            else
            {
                if (user.UserRolesString.Contains(NuJournalUserRole.Owner.ToString()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsAdministration(NuJournalUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            else
            {
                if (!user.UserRolesString.Contains(NuJournalUserRole.Owner.ToString()) && !user.UserRolesString.Contains(NuJournalUserRole.Administrator.ToString()))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool IsDisplayNameUnique(string displayName)
        {
            foreach (var appUser in _userManager.Users.ToList())
            {
                if (displayName.ToUpper() == appUser.DisplayName.ToUpper())
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsDisplayNameSimilar(string displayName)
        {
            foreach (var appUser in _userManager.Users.ToList())
            {
                if (Regex.Replace(displayName.ToUpper(), @"[^0-9a-zA-Z]+", "") == Regex.Replace(appUser.DisplayName.ToUpper(), @"[^0-9a-zA-Z]+", ""))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsDisplayNameForbidden(string displayName)
        {
            foreach (var notAllowed in Enum.GetValues(typeof(ForbidenDisplayName)).Cast<ForbidenDisplayName>().ToList())
            {
                if (displayName.ToUpper().Contains(notAllowed.ToString().ToUpper()))
                {
                    return true;
                }
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

            if (pwdOptions.RequireUppercase)
            {
                chars.Insert(rand.Next(0, chars.Count), characterPool[0][rand.Next(0, characterPool[0].Length)]);
            }
            if (pwdOptions.RequireLowercase)
            {
                chars.Insert(rand.Next(0, chars.Count), characterPool[1][rand.Next(0, characterPool[1].Length)]);
            }
            if (pwdOptions.RequireDigit)
            {
                chars.Insert(rand.Next(0, chars.Count), characterPool[2][rand.Next(0, characterPool[2].Length)]);
            }
            if (pwdOptions.RequireNonAlphanumeric)
            {
                chars.Insert(rand.Next(0, chars.Count), characterPool[3][rand.Next(0, characterPool[3].Length)]);
            }

            for (int i = chars.Count; i < pwdOptions.RequiredLength || chars.Distinct().Count() < pwdOptions.RequiredUniqueChars; i++)
            {
                string rcs = characterPool[rand.Next(0, characterPool.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public async Task<string> GetAccessDeniedImageAsync()
        {
            string compressedAccessDeniedDecodedImageB64 = "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";
            var imagePath = _defaultGraphics.ImagePath ?? Environment.GetEnvironmentVariable("ImagePath");
            var secureAccess = _defaultGraphics.SecureAccess ?? Environment.GetEnvironmentVariable("SecureAccess");

            if (imagePath != null && secureAccess != null && File.Exists($"{Directory.GetCurrentDirectory()}{imagePath}{secureAccess}"))
            {
                var defaultEncodedImageData = await _imageService.EncodeImageDataAsync(secureAccess, imagePath);
                var defaultImageMimeType = _imageService.GetImageMimeType(secureAccess, imagePath);
                if (defaultEncodedImageData != null && defaultImageMimeType != null)
                {
                    var defaultDecodedImage = _imageService.DecodeImage(defaultEncodedImageData, defaultImageMimeType);
                    if (defaultDecodedImage != null)
                    {
                        return defaultDecodedImage;
                    }
                }
            }

            return _imageService.DecompressDecodedImageB64(compressedAccessDeniedDecodedImageB64);
        }

        public async Task<ProfilePicture> GetDefaultProfilePictureAsync()
        {
            string compressedDefaultProfilePictureImageData = "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";
            string profilePictureMimeType = "image/svg+xml";
            var imagePath = _defaultUserSettings.ImagePath ?? Environment.GetEnvironmentVariable("ImagePath");
            var profilePicture = _defaultUserSettings.ProfilePicture ?? Environment.GetEnvironmentVariable("ProfilePicture");

            if (imagePath != null && profilePicture != null && File.Exists($"{Directory.GetCurrentDirectory()}{imagePath}{profilePicture}"))
            {
                var createdProfilePicture = await CreateProfilePictureAsync(profilePicture, imagePath);
                if (createdProfilePicture != null)
                {
                    return createdProfilePicture;
                }
            }

            return new ProfilePicture()
            {
                CompressedImageData = Convert.FromBase64String(compressedDefaultProfilePictureImageData),
                ImageMimeType = profilePictureMimeType,
                ImageSize = 4140
            };
        }

        public async Task<ProfilePicture> GetProfilePictureAsync(NuJournalUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            else
            {
                var profilePicture = _dbContext.ProfilePicture?.FirstOrDefault(u => u.NuJournalUserId == user.Id);

                if (profilePicture != null)
                {
                    return profilePicture;
                }
                else
                {
                    return await GetDefaultProfilePictureAsync();
                }
            }
        }

        public async Task<bool> ChangeProfilePictureAsync(NuJournalUser user, IFormFile newProfilePictureFile)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            else if (newProfilePictureFile == null)
            {
                return false;
            }
            else
            {
                var newUserProfilePicture = await CreateProfilePictureAsync(newProfilePictureFile);
                var oldUserProfilePicture = _dbContext.ProfilePicture?.FirstOrDefault(u => u.NuJournalUserId == user.Id);
                if (newUserProfilePicture == null)
                {
                    return false;
                }
                else
                {
                    if (oldUserProfilePicture == null)
                    {
                        newUserProfilePicture.NuJournalUserId = user.Id.ToString();
                        _dbContext.ProfilePicture?.Add(newUserProfilePicture);
                    }
                    else
                    {
                        oldUserProfilePicture.CompressedImageData = newUserProfilePicture.CompressedImageData;
                        oldUserProfilePicture.ImageMimeType = newUserProfilePicture.ImageMimeType;
                        oldUserProfilePicture.ImageSize = newUserProfilePicture.ImageSize;
                    }

                    if (_dbContext.SaveChanges() > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public async Task<ProfilePicture?> CreateProfilePictureAsync(IFormFile file)
        {
            if (file == null)
            {
                return null;
            }
            else
            {
                var compressedImage = await _imageService.CreateCompressedImageAsync(file);

                if (compressedImage != null)
                {
                    return new ProfilePicture()
                    {
                        CompressedImageData = compressedImage.CompressedImageData,
                        ImageMimeType = compressedImage.ImageMimeType,
                        ImageSize = compressedImage.ImageSize
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<ProfilePicture?> CreateProfilePictureAsync(string fileName, string filePath)
        {
            if (fileName == null)
            {
                return null;
            }
            else
            {
                var compressedImage = await _imageService.CreateCompressedImageAsync(fileName, filePath);

                if (compressedImage != null)
                {
                    return new ProfilePicture()
                    {
                        CompressedImageData = compressedImage.CompressedImageData,
                        ImageMimeType = compressedImage.ImageMimeType,
                        ImageSize = compressedImage.ImageSize
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        public bool DeleteProfilePicture(NuJournalUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            else
            {
                var userProfilePicture = _dbContext.ProfilePicture?.FirstOrDefault(u => u.NuJournalUserId == user.Id);
                if (userProfilePicture == null)
                {
                    return false;
                }
                else
                {
                    _dbContext.ProfilePicture?.Remove(userProfilePicture);
                    if (_dbContext.SaveChanges() > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public async Task<UserInfo> GetUserInfoAsync(NuJournalUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            else
            {
                var userInfo = new UserInfo()
                {
                    UserName = await _userManager.GetUserNameAsync(user),
                    Email = await _userManager.GetEmailAsync(user),
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    FullNameMiddleInitial = user.FullNameMiddleInitial,
                    DisplayName = user.DisplayName,
                    PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                    ProfilePicture = await GetProfilePictureAsync(user),
                    UserRoles = await _userManager.GetRolesAsync(user) as List<string>
                };
                return userInfo;
            }
        }

        public async Task<UserInputModel> GetUserInputAsync(NuJournalUser existingUser)
        {
            if (existingUser == null)
            {
                throw new ArgumentNullException(nameof(existingUser));
            }
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
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
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
                else
                {
                    return null;
                }

                if (userList.Count > 0)
                {
                    return userList;
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<NuJournalUser> CreateUserAsync(UserInputModel userModel, UserInfo? parentUserInfo = null, IFormFile? profilePictureFile = null)
        {
            if (userModel.Email == null)
            {
                throw new ArgumentNullException(nameof(userModel.Email));
            }
            else
            {
                NuJournalUser newUser = Activator.CreateInstance<NuJournalUser>();
                newUser.UserName = userModel.Email;
                newUser.Email = userModel.Email;

                if (userModel.FirstName != null)
                {
                    newUser.FirstName = userModel.FirstName;
                }
                if (userModel.MiddleName != null)
                {
                    newUser.MiddleName = userModel.MiddleName;
                }
                if (userModel.LastName != null)
                {
                    newUser.LastName = userModel.LastName;
                }
                if (userModel.DisplayName != null)
                {
                    newUser.DisplayName = userModel.DisplayName;
                }
                if (userModel.PhoneNumber != null)
                {
                    newUser.PhoneNumber = userModel.PhoneNumber;
                }

                if (parentUserInfo != null)
                {
                    if (parentUserInfo.UserName != null) newUser.CreatedByUser = parentUserInfo.UserName;
                    if (parentUserInfo.UserRoles != null) newUser.CreatedByRoles = parentUserInfo.UserRoles;
                }
                else
                {
                    newUser.CreatedByUser = "User Service"; // In this case the User Role(s) will be added automatically by the NuJournalUser data model.
                }

                if (profilePictureFile != null)
                {
                    newUser.ProfilePicture = await CreateProfilePictureAsync(profilePictureFile);
                }

                return newUser;
            }
        }

        public bool DeleteUserAccount(NuJournalUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
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
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
