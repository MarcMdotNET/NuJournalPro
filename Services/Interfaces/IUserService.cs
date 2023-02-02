using Microsoft.AspNetCore.Identity;
using NuJournalPro.Models;
using NuJournalPro.Models.Database;
using NuJournalPro.Models.Identity;

namespace NuJournalPro.Services.Interfaces
{
    public interface IUserService
    {
        bool IsAdmin(NuJournalUser user);
        bool IsOwner(NuJournalUser user);
        bool IsAdministration(NuJournalUser user);
        bool IsDisplayNameUnique(string displayName);
        bool IsDisplayNameSimilar(string displayName);
        bool IsDisplayNameForbidden(string displayName);
        string GenerateRandomPassword(PasswordOptions? pwdOptions = null);
        Task<string> GetAccessDeniedImageAsync();
        Task<ProfilePicture> GetDefaultProfilePictureAsync();
        Task<ProfilePicture> GetProfilePictureAsync(NuJournalUser user);
        Task<bool> ChangeProfilePictureAsync(NuJournalUser user, IFormFile newProfilePictureFile);
        Task<ProfilePicture?> CreateProfilePictureAsync(IFormFile file);
        Task<ProfilePicture?> CreateProfilePictureAsync(string fileName, string filePath);
        bool DeleteProfilePicture(NuJournalUser user);
        Task<UserInfo> GetUserInfoAsync(NuJournalUser user);
        Task<UserInputModel> GetUserInputAsync(NuJournalUser existingUser);
        List<NuJournalUser>? GetAppUserList(NuJournalUser user);
        Task<NuJournalUser> CreateUserAsync(UserInputModel userModel, UserInfo? parentUserInfo = null, IFormFile? profilePictureFile = null);
        bool DeleteUserAccount(NuJournalUser user);
        Task<bool> RemoveUserAccountAsync(NuJournalUser user);        
    }
}
