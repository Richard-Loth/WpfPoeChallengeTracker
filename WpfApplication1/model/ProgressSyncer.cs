using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPoeChallengeTracker.model
{
    class ProgressSyncer
    {
        private static readonly string challengeUrl = "https://www.pathofexile.com/account/view-profile/%ACCOUNT%/challenges/%LEAGUE%";

        public static void syncProgress(List<ChallengeProgress> progressList, string accountName, string currentLeague)
        {
            var url = challengeUrl.Replace("%ACCOUNT%", accountName.ToLower()).Replace("%LEAGUE%", currentLeague);

            //Get the entire challenge html page
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument document = htmlWeb.Load(url);

            //Get the challenge list node
            var rootNode = document.DocumentNode.SelectSingleNode("//div[@class='achievement-list']");
            //Get the challenges nodes
            var challengeNodes = rootNode.SelectNodes("div[@class='achievement clearfix'] | div[@class='achievement clearfix incomplete']");

            int progressIndex = -1;
            foreach (var node in challengeNodes)
            {
                progressIndex++;
                var subChallengeProgress = progressList.ElementAt(progressIndex);
                var cssClass = node.Attributes["class"];
                var subNodes = node.SelectNodes("div/span[2]/ul/li");
                switch (subChallengeProgress.Type)
                {

                    case ChallengeType.Binary:
                        if (subNodes != null)
                        {
                            Debug.WriteLine("Fehler: Gibt Unterknoten in binary challenge:" + node.SelectSingleNode("h2").InnerHtml);
                        }
                        subChallengeProgress.IsDone = !cssClass.Value.Contains("incomplete");
                        break;
                    case ChallengeType.Progressable:
                        if (subNodes == null)
                        {
                            Debug.WriteLine("Fehler: Gibt keine Unterknoten in progressable challenge:" + node.SelectSingleNode("h2").InnerHtml);
                        }
                        else
                        {
                            int subIndex = -1;
                            foreach (var subNode in subNodes)
                            {
                                subIndex++;
                                var subCssClass = subNode.Attributes["class"];
                                var completion = subCssClass.Value.Contains("finished") ? SubChallengeCompletionType.Auto : SubChallengeCompletionType.Not;
                                subChallengeProgress.SubChallengesProgress.ElementAt(subIndex).CompletionType = completion;
                            }
                        }
                        break;
                }
            }

        }
    }
}