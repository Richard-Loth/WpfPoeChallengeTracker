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
using System.Xml.Schema;
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
                var doc = generateXmlDocumentByChallengeList(list, league);
                writeXmlDocToFile(doc, "challengeData." + league.Replace("%2B", "-").Replace("%2F", "-") + ".xml");
            }
            var end = DateTime.UtcNow;
            var span = end.Subtract(start);
            Debug.WriteLine("Parsing, retrieving and converting all challenge data to xml took: " + span.Seconds + "s, " + span.Milliseconds + "ms");
        }

        private static void writeXmlDocToFile(XmlDocument doc, string filename)
        {
            var stream = new StreamWriter("C: \\Users\\Richard Loth\\Desktop\\Neuer Ordner\\" + filename);
            doc.Save(stream);

        }

        private static XmlDocument generateXmlDocumentByChallengeList(List<ChallengeData> datas, string league)
        {
            league = league.Replace("%2B", "-").Replace("%2F", "-");
            //Generate the doc and the rootelement
            var doc = new XmlDocument();
            string ns = "de/loth/richard/projects/PoeChallengeTracker/xmlns";
            XmlSchema schema = new XmlSchema();
            schema.Namespaces.Add("pct", ns);
            doc.Schemas.Add(schema);
            
            var rootElement = doc.CreateElement("leagues");
            rootElement.SetAttribute("xmlns:pct", ns);
            string nsD = rootElement.GetPrefixOfNamespace(ns) + ":";
            doc.AppendChild(rootElement);

            //generate league element
            var leagueEle = doc.CreateElement(nsD + "league",ns);
            
            rootElement.AppendChild(leagueEle);

            //leaguename
            var leagueNameEle = doc.CreateElement(nsD + "leaguename", ns);
            leagueNameEle.AppendChild(doc.CreateTextNode(league));
            leagueEle.AppendChild(leagueNameEle);

            //challenges
            var challengesEle = doc.CreateElement(nsD + "challenges", ns);
            leagueEle.AppendChild(challengesEle);

            //single challenge
            foreach (var data in datas)
            {
                var challengeEle = doc.CreateElement(nsD + "challenge", ns);
                challengesEle.AppendChild(challengeEle);
                //name
                var nameEle = doc.CreateElement(nsD + "name", ns);
                nameEle.AppendChild(doc.CreateTextNode(data.Name));
                challengeEle.AppendChild(nameEle);

                //description
                var descEle = doc.CreateElement(nsD + "description", ns);
                descEle.AppendChild(doc.CreateTextNode(data.Description));
                challengeEle.AppendChild(descEle);

                //type
                var typeEle = doc.CreateElement(nsD + "type", ns);
                typeEle.AppendChild(doc.CreateTextNode(data.Type.ToString().ToLower()));
                challengeEle.AppendChild(typeEle);

                if (data.Type == ChallengeType.Progressable)
                {
                    //needed
                    var neededEle = doc.CreateElement(nsD + "neededToComplete", ns);
                    neededEle.AppendChild(doc.CreateTextNode(Convert.ToString(data.NeedForCompletion)));
                    challengeEle.AppendChild(neededEle);

                    foreach (var subData in data.SubChallenges)
                    {
                        //subchallenge
                        var subChallengeEle = doc.CreateElement(nsD + "subchallenge", ns);
                        challengeEle.AppendChild(subChallengeEle);

                        //subname
                        var subNameEle = doc.CreateElement(nsD + "name", ns);
                        subNameEle.AppendChild(doc.CreateTextNode(subData.Description));
                        subChallengeEle.AppendChild(subNameEle);
                    }
                }
            }
            return doc;
        }

        private static List<ChallengeData> parseUrlForChallengeData(String url)
        {
            //Get the entire challenge html page
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument document = htmlWeb.Load(url);

            //Get the challenge list node
            var rootNode = document.DocumentNode.SelectSingleNode("//div[@class='achievement-list']");
            //Get the challenges nodes
            var challengesList = new List<ChallengeData>();
            var challengeNodes = rootNode.SelectNodes("div[@class='achievement clearfix'] | div[@class='achievement clearfix incomplete']");
            foreach (var node in challengeNodes)
            {
                //Grab the data
                var data = new ChallengeData();
                data.Name = node.SelectSingleNode("h2[1]").InnerHtml;
                data.Description = node.SelectSingleNode("div[@class='detail']/span[@class='text']").InnerHtml;

                //decide whether it has subchallenges
                var completionNode = node.SelectSingleNode("h2[2]");
                if (completionNode != null)
                {
                    //grab the subchallenge data
                    data.Type = ChallengeType.Progressable;
                    string completiontext = completionNode.InnerHtml.Substring(completionNode.InnerHtml.LastIndexOf('/') + 1);
                    data.NeedForCompletion = Convert.ToInt32(completiontext);
                    var subNodes = node.SelectNodes("div[@class='detail']/span[@class='items']/ul/li");
                    foreach (var subNode in subNodes)
                    {
                        data.SubChallenges.Add(new SubChallengeData(subNode.InnerHtml));
                    }
                }
                else
                {
                    data.Type = ChallengeType.Binary;
                }
                challengesList.Add(data);
            }
            return challengesList;
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
            //leagues.Add("Prophecy".Replace("/", "%2F"));
            return leagues;
        }
    }
}
