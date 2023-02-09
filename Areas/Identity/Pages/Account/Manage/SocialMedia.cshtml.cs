#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuJournalPro.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NuJournalPro.Areas.Identity.Pages.Account.Manage
{
    public class SocialMediaModel : PageModel
    {
        private readonly UserManager<NuJournalUser> _userManager;
        private readonly ILogger<SocialMediaModel> _logger;

        public SocialMediaModel(UserManager<NuJournalUser> userManager,
                                ILogger<SocialMediaModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public InputModel LinksInput { get; set; } = new InputModel();

        public class InputModel
        {
            // Social Media
            [Display(Name = "GitHub Repository")]
            [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            [Url]
            public string GitHubUrl { get; set; }

            [Display(Name = "Twitter Profile")]
            [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            [Url]
            public string TwitterUrl { get; set; }

            [Display(Name = "LinkedIn Profile")]
            [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            [Url]
            public string LinkedInUrl { get; set; }

            [Display(Name = "YouTube Channel")]
            [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            [Url]
            public string YouTubeUrl { get; set; }

            [Display(Name = "Facebook Profile")]
            [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            [Url]
            public string FacebookUrl { get; set; }

            [Display(Name = "Instagram Profile")]
            [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
            [Url]
            public string InstagramUrl { get; set; }
        }

        private async Task LoadSocialMediaAsync(NuJournalUser user)
        {            
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;

            LinksInput.GitHubUrl = user.GitHubUrl;
            LinksInput.TwitterUrl = user.TwitterUrl;
            LinksInput.LinkedInUrl = user.LinkedInUrl;
            LinksInput.YouTubeUrl = user.YouTubeUrl;
            LinksInput.FacebookUrl = user.FacebookUrl;
            LinksInput.InstagramUrl = user.InstagramUrl;
        }

        public async Task<IActionResult> OnGetAsync()
        {            
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadSocialMediaAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(InputModel linksInput)
        {            
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSocialMediaAsync(user);
            }
            else
            {
                if (linksInput.GitHubUrl == user.GitHubUrl &&
                    linksInput.TwitterUrl == user.TwitterUrl &&
                    linksInput.LinkedInUrl == user.LinkedInUrl &&
                    linksInput.YouTubeUrl == user.YouTubeUrl &&
                    linksInput.FacebookUrl == user.FacebookUrl &&
                    linksInput.InstagramUrl == user.InstagramUrl)
                {
                    StatusMessage = "Error: No changes were made to your social media links.";
                    await LoadSocialMediaAsync(user);                    
                }
                else
                {
                    if (linksInput.GitHubUrl != user.GitHubUrl)
                    {
                        user.GitHubUrl = linksInput.GitHubUrl;
                    }
                    if (linksInput.TwitterUrl != user.TwitterUrl)
                    {
                        user.TwitterUrl = linksInput.TwitterUrl;
                    }
                    if (linksInput.LinkedInUrl != user.LinkedInUrl)
                    {
                        user.LinkedInUrl = linksInput.LinkedInUrl;
                    }
                    if (linksInput.YouTubeUrl != user.YouTubeUrl)
                    {
                        user.YouTubeUrl = linksInput.YouTubeUrl;
                    }
                    if (linksInput.FacebookUrl != user.FacebookUrl)
                    {
                        user.FacebookUrl = linksInput.FacebookUrl;
                    }
                    if (linksInput.InstagramUrl != user.InstagramUrl)
                    {
                        user.InstagramUrl = linksInput.InstagramUrl;
                    }

                    var userUpdateStatus = await _userManager.UpdateAsync(user);

                    if (userUpdateStatus.Succeeded)
                    {
                        StatusMessage = "Your social media links have been succesfully updated.";
                        _logger.LogInformation($"The social media links for user {user} have been succesfully updated.");
                    }
                    else
                    {
                        StatusMessage = "Error: An error occured while updating your social media links.";
                        _logger.LogError($"An error occured while updating the social media links for user {user}.");
                    }
                    
                    await LoadSocialMediaAsync(user);
                }
            }

            return Page();
        }
    }
}
