using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPoeChallengeTracker.model
{
    class DataCreator
    {
        public  LeagueInfo createChallengeDataListFromXml(string xmlPath)
        {
            try
            {
                return  new XmlChallengeReader().readXml(xmlPath);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error during xml challenge data parsing: " + e.Message);
                throw new Exception("Error during xml parsing", e);
            }
        }

        public List<ChallengeProgress> createChallengeProgressListFromChallengeList(List<ChallengeData> dataList)
        {
            var progressList = new List<ChallengeProgress>();
            foreach (var data in dataList)
            {
                var progress = new ChallengeProgress(data.Type);
                if (data.Type == ChallengeType.Progressable)
                {
                    foreach (var subData in data.SubChallenges)
                    {
                        progress.AddSubChallengeProgress(new SubChallengeProgress());
                    }
                }
                progressList.Add(progress);
            }
            return progressList;
        }
    }
}
