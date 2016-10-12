using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPoeChallengeTracker.model
{
    public class SubChallengeData
    {
        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private List<SubChallengeInfo> infos;
        public List<SubChallengeInfo> Infos
        {
            get { return infos; }
        }   


        public SubChallengeData(string description)
        {
            Description = description;
            infos = new List<SubChallengeInfo>();
        }

        public SubChallengeData() : this("")
        {

        }

        private bool isProgressable;

        public bool IsProgressable
        {
            get { return isProgressable; }
            set { isProgressable = value; }
        }

        private int neededToComplete;

        public int NeededToComplete
        {
            get { return neededToComplete; }
            set { neededToComplete = value; }
        }
    }
}
