using System.Collections.Generic;

namespace WpfPoeChallengeTracker.model
{
    public class ChallengeData
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private bool wikiLinksExist;

        public bool WikiLinksExist
        {
            get { return wikiLinksExist; }
            set { wikiLinksExist = value; }
        }


        private ChallengeType type;
        public ChallengeType Type
        {
            get { return type; }
            set
            {
                type = value;
                if (type == ChallengeType.Progressable && subChallenges == null)
                {
                    subChallenges = new List<SubChallengeData>();
                }
            }
        }

        private int needForCompletion;

        public int NeedForCompletion
        {
            get { return needForCompletion; }
            set { needForCompletion = value; }
        }



        public ChallengeData() : this("", "", ChallengeType.Binary)
        {
            wikiLinksExist = false;
        }

        public ChallengeData(string name, string description, ChallengeType type)
        {
            Name = name;
            Description = description;
            Type = type;
            if (type == ChallengeType.Progressable)
            {
                subChallenges = new List<SubChallengeData>();
            }
        }

        private List<SubChallengeData> subChallenges;
        public List<SubChallengeData> SubChallenges
        {
            get
            {
                if (subChallenges == null)
                {
                    subChallenges = new List<SubChallengeData>();
                }
                return subChallenges;
            }
        }
    }
}
