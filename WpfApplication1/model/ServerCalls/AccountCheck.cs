using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPoeChallengeTracker.model
{
    class AccountCheck
    {
        private static readonly string profileUrl = "https://www.pathofexile.com/account/view-profile/%ACCOUNT%";
        private static readonly string challengeUrl = "https://www.pathofexile.com/account/view-profile/%ACCOUNT%/challenges/";

        public static LoginStatus checkAccountName(string accountname)
        {
            var url = profileUrl.Replace("%ACCOUNT%", accountname.ToLower());

            //Get the entire profile html page
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument document = htmlWeb.Load(url);

            if (checkForInvalidName(document))
            {
                return LoginStatus.InvalidName;
            }

            if (checkForPrivateProfile(document))
            {
                return LoginStatus.ValidNamePrivateProfile;
            }

            url = challengeUrl.Replace("%ACCOUNT%", accountname);
            htmlWeb = new HtmlWeb();
            document = htmlWeb.Load(url);

            if (checkForPrivateProfile(document))
            {
                //same function, different url
                return LoginStatus.ValidNamePrivateChallenges;
            }

            if (checkForValidProfile(document))
            {
                return LoginStatus.ValidName;
            }

            //something weird happening, account status not detected
            return LoginStatus.InvalidName;
        }

        private static bool checkForInvalidName(HtmlDocument document)
        {
            var node = document.DocumentNode.SelectSingleNode("/html/body/div[1]/div[2]/div[2]/div[1]/h1");
            if (node != null)
            {
                var inner = node.InnerHtml.ToLower();
                if (inner == "login")
                {
                    return true;
                }
            }
            return false;
        }

        private static bool checkForPrivateProfile(HtmlDocument document)
        {

            var node = document.DocumentNode.SelectSingleNode("/html/body/div[1]/div[2]/div[2]/div[1]/div/div/div[2]/div[3]/div/p/em");
            if (node != null)
            {
                var inner = node.InnerHtml.ToLower();
                if (inner.Contains("private"))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool checkForValidProfile(HtmlDocument document)
        {
            var node = document.DocumentNode.SelectSingleNode("/html/body/div[1]/div[2]/div[2]/div[1]/div/div/div[2]/div[3]/div/div[2]/h2");
            if (node != null)
            {
                var inner = node.InnerHtml.ToLower();
                if (inner.Contains("challenges"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
