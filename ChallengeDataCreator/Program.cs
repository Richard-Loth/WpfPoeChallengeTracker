using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WpfPoeChallengeTracker.model;

namespace ChallengeDataCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            var leagues = makeLeaguesList();
            var baseUrl = "https://www.pathofexile.com/account/view-profile/xGeronimo87x/challenges/";
            var start = DateTime.UtcNow;
            foreach (var league in leagues)
            {
                var list = parseUrlForChallengeData(baseUrl + league);
        
            }
            var end = DateTime.UtcNow;
            var span = end.Subtract(start);
            Debug.WriteLine("Parsing and retrieving all challenge data took: " + span.Seconds + "s, " + span.Milliseconds + "ms");
        }


        private static List<ChallengeData> parseUrlForChallengeData(String url)
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument document = htmlWeb.Load(url);
            var challengesList = new List<ChallengeData>();
            var rootNode = document.DocumentNode.SelectSingleNode("//div[@class='achievement-list']");
            var challengeNodes = rootNode.SelectNodes("div[@class='achievement clearfix'] | div[@class='achievement clearfix incomplete']");
            foreach (var node in challengeNodes)
            {
                var data = new ChallengeData();
                data.Name= node.SelectSingleNode("h2[1]").InnerHtml;

                var completionNode = node.SelectSingleNode("h2[2]");
                if (completionNode != null)
                {
                    data.Type = ChallengeType.Progressable;
                    string completiontext = completionNode.InnerHtml.Substring(completionNode.InnerHtml.LastIndexOf('/')+1);
                    data.NeedForCompletion = Convert.ToInt32(completiontext);

                }
                else
                {
                    data.Type = ChallengeType.Binary;
                }

                data.Description = node.SelectSingleNode("div[@class='detail']/span[@class='text']").InnerHtml;

                challengesList.Add(data);
            }
            return challengesList;
        }

        private static Stream getResponse(string url)
        {
            WebRequest request = WebRequest.Create(url);
            return request.GetResponse().GetResponseStream();
        }



        private static List<string> makeLeaguesList()
        {
            var leagues = new List<string>();
            leagues.Add("Anarchy/Onslaught".Replace("/", "%2F"));
            leagues.Add("Domination/Nemesis".Replace("/", "%2F"));
            leagues.Add("Ambush/Invasion".Replace("/", "%2F"));
            leagues.Add("Rampage/Beyond".Replace("/", "%2F"));
            leagues.Add("Torment/Bloodlines".Replace("/", "%2F"));
            leagues.Add("Torment/Bloodlines+1-Month".Replace("/", "%2B"));
            leagues.Add("Torment/Bloodlines+1-Month+HC".Replace("/", "%2B"));
            leagues.Add("Warbands/Tempest".Replace("/", "%2F"));
            leagues.Add("Flashback".Replace("/", "%2F"));
            leagues.Add("Talisman".Replace("/", "%2F"));
            leagues.Add("Perandus".Replace("/", "%2F"));
            leagues.Add("Prophecy".Replace("/", "%2F"));
            return leagues;
        }
    }
}
