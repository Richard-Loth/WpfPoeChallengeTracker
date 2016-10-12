using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                try
                {
                    progressIndex++;
                    var challengeProgress = progressList.ElementAt(progressIndex);
                    var cssClass = node.Attributes["class"];
                    var subNodes = node.SelectNodes("div/span[2]/ul/li");
                    switch (challengeProgress.Type)
                    {

                        case ChallengeType.Binary:
                            if (subNodes != null)
                            {
                                Debug.WriteLine("Fehler: Gibt Unterknoten in binary challenge:" + node.SelectSingleNode("h2").InnerHtml);
                            }
                            challengeProgress.IsDone = !cssClass.Value.Contains("incomplete");
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
                                    var subChallengeProgress = challengeProgress.SubChallengesProgress.ElementAt(subIndex);
                                    subChallengeProgress.CurrentCompletion = completion;
                                    var subDesc = subNode.InnerText;
                                    if (subDesc.Contains("/"))
                                    {
                                        var index = checkIfProgressableSub(subDesc);
                                        if (index > 0)
                                        {
                                            var slashIndex = subDesc.LastIndexOf("/");
                                            if (slashIndex > index)
                                            {
                                                string completed = subDesc.Substring(index, slashIndex - index);
                                                subChallengeProgress.CurrentProgress = Convert.ToInt32(completed);
                                            }
                                        } 
                                    }
                                }
                            }
                            break;
                        case ChallengeType.ProgressableNoSubs:
                            if (subNodes != null)
                            {
                                Debug.WriteLine("Fehler: Gibt  Unterknoten in progressableNoSubs challenge:" + node.SelectSingleNode("h2").InnerHtml);
                            }
                            var incompleteNode = node.SelectSingleNode("h2[2]/span");
                            if (incompleteNode != null)
                            {
                                var incompleteString = incompleteNode.InnerHtml;
                                challengeProgress.Progress = Convert.ToInt32(incompleteString);
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        private static Regex progressableSubRegex = new Regex(@"(\d+/\d+)");

        private static int checkIfProgressableSub(string subChallengeDescription)
        {
            var match = progressableSubRegex.Match(subChallengeDescription);
            if (match.Success)
            {
                return match.Index;
            }
            return -1;
        }
    }
}