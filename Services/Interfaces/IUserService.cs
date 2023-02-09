using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuJournalPro.Enums;
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
        string VerifyDisplayName(string displayName, NuJournalUser? requestingUser = null);
        string FormatPhoneNumber(string unformatedPhoneNumber);
        string GenerateRandomPassword(PasswordOptions? pwdOptions = null);
        Task<string> GetAccessDeniedImageAsync();
        Task<ProfilePicture> GetDefaultProfilePictureAsync();
        Task<ProfilePicture> GetProfilePictureAsync(NuJournalUser user);        
        Task<bool> ChangeProfilePictureAsync(NuJournalUser user, IFormFile newProfilePictureFile);
        Task<ProfilePicture?> CreateProfilePictureAsync(IFormFile file);
        Task<ProfilePicture?> CreateProfilePictureAsync(string fileName, string filePath);
        bool DeleteProfilePicture(NuJournalUser user);
        SelectList? CreateUserRolesList(NuJournalUser requestingUser, bool? showDeletedRole = null);        
        Task<NuJournalUserRole> GetDefaultUserRoleAsync(NuJournalUser user);
        Task<UserInfo> GetUserInfoAsync(NuJournalUser user);
        Task<UserInputModel> GetUserInputAsync(NuJournalUser existingUser);
        List<NuJournalUser>? GetAppUserList(NuJournalUser user, bool? showDeletedUsers = null);
        List<NuJournalUser>? GetDeletedUserList(NuJournalUser user);
        Task<NuJournalUser> CreateUserAsync(UserInputModel userModel, NuJournalUser? parentUser = null, IFormFile? profilePictureFile = null, bool? emailConfirmed = null);
        bool DeleteUserAccount(NuJournalUser user);
        Task<bool> RemoveUserAccountAsync(NuJournalUser user);        
    }
}
