using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using NuJournalPro.Data;
using NuJournalPro.Models.Database;
using NuJournalPro.Models.Settings;
using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Services.Helpers
{
    public class SetupDataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AddNewUserModel> _logger;
        private readonly IUserStore<NuJournalUser> _userStore;
        private readonly IUserEmailStore<NuJournalUser> _emailStore;
        private readonly OwnerSettings _ownerSettings;
        private readonly IEmailSender _emailSender;
        private readonly IUserService _userService;

        public SetupDataService(ApplicationDbContext dbContext,
                                UserManager<NuJournalUser> userManager,
                                RoleManager<IdentityRole> roleManager,
                                ILogger<AddNewUserModel> logger,
                                IUserStore<NuJournalUser> userStore,                                
                                OwnerSettings ownerSettings,
                                IEmailSender emailSender,
                                IUserService userService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _ownerSettings = ownerSettings;
            _emailSender = emailSender;
            _userService = userService;
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
