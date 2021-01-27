using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKBugClient
{
    public class HtmlHelper
    {
        public static string GenerateBoldTag(string text)
        {
            return $@"<b>{text}</b>";
        }

        public static string GenerateItalicTag(string text)
        {
            return $@"<i>{text}</i>";
        }

        public static string GenerateLink(string inlineText, string url)
        {
            return $@"<a href=""{url}"">{inlineText}</a>";
        }

        public static string GenerateMentionLink(int telegramUserId, string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                if (telegramUserId == -1)
                    userName = "demouser";
                else
                    userName = "Someone";
            }

            return $@"<a href=""tg://user?id={telegramUserId}"">{userName}</a>";
        }
    }
}
