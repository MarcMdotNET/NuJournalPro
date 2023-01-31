using Microsoft.AspNetCore.Identity;
using NuJournalPro.Models.Database;
using NuJournalPro.Models.Identity;

namespace NuJournalPro.Services.Interfaces
{
    public interface IUserService
    {
        Task<NuJournalUser> CreateNewUserAsync(UserInputModel userModel, UserInfo? parentUserInfo = null, IFormFile? userProfilePictureFile = null);
        Task<string> GetAccessDeniedImageAsync();
        Task<UserInfo> GetUserInfoAsync(NuJournalUser user);
        Task<ProfilePicture> GetDefaultProfilePictureAsync();
        bool IsAdmin(NuJournalUser user);
        bool IsOwner(NuJournalUser user);
        bool IsAdministration(NuJournalUser user);
        bool IsDisplayNameUnique(string displayName);
        bool IsDisplayNameSimilar(string displayName);
        bool IsDisplayNameForbidden(string displayName);
        string GenerateRandomPassword(PasswordOptions? pwdOptions = null);
        List<NuJournalUser> GetAppUserList(NuJournalUser user);
        Task<UserInputModel> GetExistingUserInputAsync(NuJournalUser existingUser);
        Task<ProfilePicture> GetProfilePictureAsync(NuJournalUser user);
        Task<bool> ChangeProfilePictureAsync(NuJournalUser user, IFormFile newProfilePictureFile);
        Task<bool> DeleteProfilePictureAsync(NuJournalUser user);
        Task<bool> RemoveUserAccountAsync(NuJournalUser user);
    }
}
