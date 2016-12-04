using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
            var numberOfLeague = 0;
            foreach (var league in leagues)
            {
                numberOfLeague++;
                Debug.WriteLine("-------------------\nDownloading and parsing data for league " + league);
                var list = parseUrlForChallengeData(baseUrl + league);
                Debug.WriteLine("Creating xml object");
                var doc = generateXmlDocumentByChallengeList(list, league);
                var filename = "challengeData" + numberOfLeague.ToString("00") + "." + league.Replace("%2B", "-").Replace("%2F", "-") + ".xml";
                Debug.WriteLine("Writing xml object to file " + filename);
                writeXmlDocToFile(doc, filename);
                if (numberOfLeague < leagues.Count)
                {
                    Debug.WriteLine("Doing a five second break to not make servers angry");
                    Task.Delay(5000).Wait();
                }

            }
            var end = DateTime.UtcNow;
            var span = end.Subtract(start);
            Debug.WriteLine("\n\nParsing, retrieving and converting all challenge data to xml took: " + (span.Minutes > 0 ? span.Minutes + "m, " : "") + span.Seconds + "s, " + span.Milliseconds + "ms");
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
            var leagueEle = doc.CreateElement(nsD + "league", ns);

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
                typeEle.AppendChild(doc.CreateTextNode(data.Type.ToString()));
                challengeEle.AppendChild(typeEle);

                if (data.Type == ChallengeType.Progressable || data.Type == ChallengeType.ProgressableNoSubs)
                {
                    //needed
                    var neededEle = doc.CreateElement(nsD + "neededToComplete", ns);
                    neededEle.AppendChild(doc.CreateTextNode(Convert.ToString(data.NeedForCompletion)));
                    challengeEle.AppendChild(neededEle);
                    bool firstSub = true;
                    foreach (var subData in data.SubChallenges)
                    {

                        //subchallenge
                        var subChallengeEle = doc.CreateElement(nsD + "subchallenge", ns);
                        challengeEle.AppendChild(subChallengeEle);

                        //subname
                        var subNameEle = doc.CreateElement(nsD + "name", ns);
                        subNameEle.AppendChild(doc.CreateTextNode(subData.Description));
                        subChallengeEle.AppendChild(subNameEle);

                        //progressable subs
                        if (subData.IsProgressable)
                        {
                            var subProgressEle = doc.CreateElement(nsD + "neededToCompleteSub", ns);
                            subProgressEle.AppendChild(doc.CreateTextNode(Convert.ToString(subData.NeededToComplete)));
                            subChallengeEle.AppendChild(subProgressEle);
                        }

                        if (firstSub)
                        {
                            //checkIfWikiLinksExist
                            Debug.WriteLine("Testing if wiki page exists: " + subData.Description);
                            var baseWikiUrl = "http://pathofexile.gamepedia.com";
                            HtmlWeb htmlWeb = new HtmlWeb();
                            HtmlDocument document = htmlWeb.Load(baseWikiUrl + "/" + subData.Description.Replace(" ", "_"));
                            var node = document.DocumentNode.SelectSingleNode("//html/body/div[2]/div[3]/div[1]/div[4]/div[4]/div/p");
                            if (node == null)
                            {
                                Debug.WriteLine("Yes");
                                var att = doc.CreateAttribute("createWikiLinks");
                                att.Value = "true";
                                challengeEle.Attributes.Append(att);
                            }
                            else
                            {
                                Debug.WriteLine("No");
                            }
                            Task.Delay(1000).Wait();
                        }
                        firstSub = false;
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

                    string completiontext = completionNode.InnerHtml.Substring(completionNode.InnerHtml.LastIndexOf('/') + 1);
                    data.NeedForCompletion = Convert.ToInt32(completiontext);
                    var subNodes = node.SelectNodes("div[@class='detail']/span[@class='items']/ul/li");
                    if (subNodes != null)
                    {
                        data.Type = ChallengeType.Progressable;
                        foreach (var subNode in subNodes)
                        {
                            var isProgressableSub = false;
                            var neededToComplete = 0;
                            var desc = subNode.InnerHtml.Trim();
                            var indexOfBracketOpen = checkIfProgressableSub(desc);
                            if (indexOfBracketOpen > 0)
                            {
                                isProgressableSub = true;
                                var indexOfSlash = desc.LastIndexOf("/");
                                var needed = desc.Substring(indexOfSlash + 1, desc.Length - indexOfSlash - 2);
                                if (needed.Contains(","))
                                {
                                    if (needed.Length == 5)
                                    {
                                        needed = needed.Remove(1, 1);
                                    }
                                }
                                neededToComplete = Convert.ToInt32(needed);
                                desc = desc.Substring(0, indexOfBracketOpen - 1).Trim();
                            }

                            var subdata = new SubChallengeData(desc);
                            subdata.IsProgressable = isProgressableSub;
                            subdata.NeededToComplete = neededToComplete;
                            data.SubChallenges.Add(subdata);
                        }
                    }
                    else
                    {
                        data.Type = ChallengeType.ProgressableNoSubs;
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

        private static List<string> makeLeaguesList()
        {
            var leagues = new List<string>();
            //leagues.Add("Anarchy/Onslaught".Replace("/", "%2F"));
            //leagues.Add("Domination/Nemesis".Replace("/", "%2F"));
            //leagues.Add("Ambush/Invasion".Replace("/", "%2F"));
            //leagues.Add("Rampage/Beyond".Replace("/", "%2F"));
            //leagues.Add("Torment/Bloodlines".Replace("/", "%2F"));
            //leagues.Add("Torment/Bloodlines+1-Month".Replace("/", "%2B"));
            //leagues.Add("Torment/Bloodlines+1-Month+HC".Replace("/", "%2B"));
            //leagues.Add("Warbands/Tempest".Replace("/", "%2F"));
            //leagues.Add("Flashback".Replace("/", "%2F"));
            //leagues.Add("Talisman".Replace("/", "%2F"));
            //leagues.Add("Perandus".Replace("/", "%2F"));
            //leagues.Add("Prophecy".Replace("/", "%2F"));
            //leagues.Add("Essence".Replace("/", "%2F"));
            leagues.Add("Breach".Replace("/", "%2F"));
            return leagues;
        }
    }
}
