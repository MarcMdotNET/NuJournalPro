using NuJournalPro.Data;
using NuJournalPro.Services.Interfaces;
using System.Text;

namespace NuJournalPro.Services
{
    public class SlugService : ISlugService
    {
        private readonly ApplicationDbContext _dbContext;

        public SlugService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool IsUnique(string slug)
        {
            bool isUnique = false;

            if (_dbContext.Posts != null)
            {
                isUnique = !_dbContext.Posts.Any(cp => cp.Slug == slug);
            }

            if (_dbContext.Pages != null)
            {
                isUnique = !_dbContext.Pages.Any(cp => cp.Slug == slug);
            }

            return isUnique;
        }

        public string UrlFriendly(string title)
        {
            if (title == null)
            {
                return string.Empty;
            }

            const int maxLength = 80;
            var titleLength = title.Length;
            var previousDash = false;

            var stringBuilder = new StringBuilder(titleLength);

            char localCharacter;

            for (int i = 0; i < titleLength; i++)
            {
                localCharacter = title[i];
                if ((localCharacter >= 'a' && localCharacter <= 'z') || (localCharacter >= '0' && localCharacter <= '9'))
                {
                    stringBuilder.Append(localCharacter);
                    previousDash = false;
                }
                else if (localCharacter >= 'A' && localCharacter <= 'Z')
                {
                    // Convert to lowercase.
                    stringBuilder.Append((char)(localCharacter | 32));
                    previousDash = false;
                }
                else if (localCharacter == ' ' || localCharacter == ',' || localCharacter == '.' || localCharacter == '/' ||
                        localCharacter == '\\' || localCharacter == '-' || localCharacter == '_' || localCharacter == '=')
                {
                    if (!previousDash && stringBuilder.Length > 0)
                    {
                        stringBuilder.Append('-');
                        previousDash = true;
                    }
                }
                else if (localCharacter == '#')
                {
                    if (i > 0)
                    {
                        if (title[i - 1] == 'C' || title[i - 1] == 'F')
                        {
                            stringBuilder.Append("-sharp");
                        }
                    }
                }
                else if (localCharacter == '+')
                {
                    stringBuilder.Append("-plus");
                }
                else if ((int)localCharacter >= 128)
                {
                    int prevlen = stringBuilder.Length;
                    stringBuilder.Append(RemapInt2ASCII(localCharacter));
                    if (prevlen != stringBuilder.Length)
                    {
                        previousDash = false;
                    }
                }
                if (stringBuilder.Length == maxLength)
                {
                    break;
                }
            }
            if (previousDash)
            {
                return stringBuilder.ToString().Substring(0, stringBuilder.Length - 1);
            }
            else
            {
                return stringBuilder.ToString();
            }
        }

        private string RemapInt2ASCII(char intChar)
        {
            string charString = intChar.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(charString))
            {
                return "a";
            }
            else if ("èéêëę".Contains(charString))
            {
                return "e";
            }
            else if ("ìíîïı".Contains(charString))
            {
                return "i";
            }
            else if ("òóôõöøőð".Contains(charString))
            {
                return "o";
            }
            else if ("ùúûüŭů".Contains(charString))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(charString))
            {
                return "intChar";
            }
            else if ("żźž".Contains(charString))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(charString))
            {
                return "charString";
            }
            else if ("ñń".Contains(charString))
            {
                return "n";
            }
            else if ("ýÿ".Contains(charString))
            {
                return "y";
            }
            else if ("ğĝ".Contains(charString))
            {
                return "g";
            }
            else if (intChar == 'ř')
            {
                return "r";
            }
            else if (intChar == 'ł')
            {
                return "l";
            }
            else if (intChar == 'đ')
            {
                return "d";
            }
            else if (intChar == 'ß')
            {
                return "ss";
            }
            else if (intChar == 'Þ')
            {
                return "th";
            }
            else if (intChar == 'ĥ')
            {
                return "h";
            }
            else if (intChar == 'ĵ')
            {
                return "j";
            }
            else
            {
                return "";
            }
        }
    }
}
