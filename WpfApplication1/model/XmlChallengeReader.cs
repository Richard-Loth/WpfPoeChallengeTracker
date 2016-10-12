using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace WpfPoeChallengeTracker.model
{
    class XmlChallengeReader
    {

        const string LEAGUE = "league";
        const string NAME = "name";
        const string LEAGUENAME = "leaguename";
        const string ENDS_ON = "endsOn";
        const string NUMBER_OF_CHALLENGES = "numberOfChallenges";
        const string STARTED_ON = "startedOn";
        const string CHALLENGES = "challenges";
        const string CHALLENGE = "challenge";
        const string DESCRIPTION = "description";
        const string TYPE = "type";
        const string BINARY = "binary";
        const string PROGRESSABLE = "progressable";
        const string PROGRESSABLE_NO_SUBS = "progressableNoSubs";
        const string NEEDED_TO_COMPLETE = "neededToComplete";
        const string NEEDED_TO_COMPLETE_SUB = "neededToCompleteSub";
        const string SUBCHALLENGE = "subchallenge";
        const string INFO = "info";
        const string TEXT = "text";
        const string URL = "url";

        const string WIKI_BASE_URL = "http://pathofexile.gamepedia.com/";





        public LeagueInfo readXml(string xmlPath)
        {
            var uri = new Uri(xmlPath);
            var stream = new StreamReader(xmlPath).BaseStream;
            var leagueInfo = new LeagueInfo();
            var data = new List<ChallengeData>();
            leagueInfo.ChallengeDatas = data;
            XmlReader xml = XmlReader.Create(stream);
            bool insideLeaguename = false;
            bool insideStartedOn = false;
            bool insideEndsOn = false;
            bool insideSingleChallenge = false;
            bool insideSubChallenge = false;
            bool insideInfo = false;
            bool insideName = false;
            bool insideDescription = false;
            bool insideType = false;
            bool insideNeedForCompletion = false;
            bool insideNeedForCompletionSub = false;
            bool insideInfoText = false;
            bool insideInfoUrl = false;
            ChallengeData challengeData = null;
            SubChallengeData subChallengeData = null;
            SubChallengeInfo info = null;
            while (xml.Read())

            {
                switch (xml.NodeType)
                {
                    case XmlNodeType.Element:
                        string element = xml.Name;
                        if (element.Contains(":"))
                        {
                            element = element.Substring(element.IndexOf(":") + 1);
                        }
                        if (element == CHALLENGES)
                        {
                            continue;
                        }
                        if (!insideSingleChallenge && element == LEAGUENAME)
                        {
                            insideLeaguename = true;
                            continue;
                        }
                        if (!insideSingleChallenge && element == STARTED_ON)
                        {
                            insideStartedOn = true;
                            continue;
                        }
                        if (!insideEndsOn && element == ENDS_ON)
                        {
                            insideEndsOn = true;
                            continue;
                        }
                        if (element == CHALLENGE)
                        {
                            challengeData = new ChallengeData();
                            var attribute = xml.GetAttribute("createWikiLinks");
                            if (attribute != null)
                            {
                                challengeData.WikiLinksExist = attribute.ToLower() == "true";
                            }
                            insideSingleChallenge = true;
                            continue;
                        }
                        if (element == SUBCHALLENGE)
                        {
                            insideSubChallenge = true;
                            subChallengeData = new SubChallengeData();
                            continue;
                        }
                        if (insideSingleChallenge && !insideSubChallenge)
                        {
                            insideName = element == NAME;
                            insideDescription = element == DESCRIPTION;
                            insideType = element == TYPE;
                            insideNeedForCompletion = element == NEEDED_TO_COMPLETE;
                            continue;
                        }
                        if (insideSingleChallenge && insideSubChallenge && !insideInfo)
                        {
                            insideName = element == NAME;
                            insideNeedForCompletionSub = element == NEEDED_TO_COMPLETE_SUB;
                            if (element == INFO)
                            {
                                insideInfo = true;
                                info = new SubChallengeInfo();
                            }
                            continue;
                        }
                        if (insideSingleChallenge && insideSubChallenge && insideInfo)
                        {
                            insideInfoText = element == TEXT;
                            insideInfoUrl = element == URL;
                        }


                        break;
                    case XmlNodeType.Text:
                        if (insideLeaguename)
                        {
                            leagueInfo.Leaguename = xml.Value.Trim();
                            continue;
                        }
                        if (insideStartedOn)
                        {
                            string datetime = xml.Value.Trim();
                            if (datetime != null && datetime.Length > 0)
                            {

                                DateTime dt = DateTime.ParseExact(datetime, "yyyy-MM-dd HH:mmZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                                leagueInfo.LeagueStartedOn = dt;
                            }
                            continue;
                        }
                        if (insideEndsOn)
                        {
                            string datetime = xml.Value.Trim();
                            if (datetime != null && datetime.Length > 0)
                            {
                                DateTime dt = DateTime.ParseExact(datetime, "yyyy-MM-dd HH:mmZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                                leagueInfo.LeagueEndsOn = dt;
                            }
                            continue;
                        }
                        if (insideSingleChallenge && !insideSubChallenge)
                        {
                            if (insideName)
                            {
                                challengeData.Name = xml.Value.Trim();
                            }
                            if (insideDescription)
                            {
                                challengeData.Description = xml.Value.Trim();
                            }
                            if (insideType)
                            {
                                String type = xml.Value.Trim();
                                if (type == BINARY)
                                {
                                    challengeData.Type = ChallengeType.Binary;
                                    continue;
                                }
                                if (type == PROGRESSABLE)
                                {
                                    challengeData.Type = ChallengeType.Progressable;
                                    continue;
                                }
                                if (type == PROGRESSABLE_NO_SUBS)
                                {
                                    challengeData.Type = ChallengeType.ProgressableNoSubs;
                                    continue;
                                }
                                throw new InvalidOperationException("Encountered unknown challenge type during processing " + xmlPath);
                            }
                            if (insideNeedForCompletion)
                            {
                                String number = xml.Value.Trim();
                                challengeData.NeedForCompletion = Convert.ToInt32(number);
                            }
                        }
                        if (insideSingleChallenge && insideSubChallenge)
                        {
                            if (insideName)
                            {
                                var subName = xml.Value.Trim();
                                subChallengeData.Description = subName;
                                if (challengeData.WikiLinksExist)
                                {
                                    var generatedInfo = new SubChallengeInfo();
                                    generatedInfo.Text = "Wiki";
                                    generatedInfo.UrlAsString = WIKI_BASE_URL + subName.Replace(" ", "_");
                                    subChallengeData.Infos.Add(generatedInfo);
                                }
                            }
                            if (insideNeedForCompletionSub)
                            {
                                subChallengeData.IsProgressable = true;
                                subChallengeData.NeededToComplete = Convert.ToInt32(xml.Value.Trim());
                            }
                            if (insideInfo)
                            {
                                if (insideInfoText)
                                {
                                    info.Text = xml.Value.Trim();
                                }
                                if (insideInfoUrl)
                                {
                                    info.UrlAsString = xml.Value.Trim();
                                }
                            }
                        }
                        break;



                    case XmlNodeType.EndElement:
                        string endElement = xml.Name;
                        if (endElement.Contains(":"))
                        {
                            endElement = endElement.Substring(endElement.IndexOf(":") + 1);
                        }
                        if (!insideSingleChallenge && endElement == LEAGUENAME)
                        {
                            insideLeaguename = false;
                        }
                        if (!insideSingleChallenge && endElement == STARTED_ON)
                        {
                            insideStartedOn = false;
                        }
                        if (!insideSingleChallenge && endElement == ENDS_ON)
                        {
                            insideEndsOn = false;
                        }

                        if (endElement == CHALLENGE)
                        {
                            insideSingleChallenge = false;
                            data.Add(challengeData);
                        }

                        if (insideSingleChallenge && !insideSubChallenge)
                        {
                            insideName = insideDescription = insideType = insideNeedForCompletion = false;
                        }
                        if (insideSingleChallenge && insideSubChallenge && !insideInfo)
                        {
                            if (endElement == SUBCHALLENGE)
                            {
                                insideSubChallenge = false;
                                challengeData.SubChallenges.Add(subChallengeData);
                            }
                            if (endElement == NAME)
                            {
                                insideName = false;
                            }
                            if (endElement == NEEDED_TO_COMPLETE_SUB)
                            {
                                insideNeedForCompletionSub = false;
                            }
                        }
                        if (insideSingleChallenge && insideSubChallenge && insideInfo)
                        {
                            if (endElement == TEXT)
                            {
                                insideInfoText = false;
                            }
                            if (endElement == URL)
                            {
                                insideInfoUrl = false;
                            }
                            if (endElement == INFO)
                            {
                                insideInfo = false;
                                subChallengeData.Infos.Add(info);
                            }
                        }
                        break;
                }
            }
            stream.Dispose();
            xml.Dispose();
            return leagueInfo;
        }
    }
}
