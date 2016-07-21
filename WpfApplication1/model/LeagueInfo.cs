using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poe_Challenge_Tracker.model
{
   public  class LeagueInfo
    {

        private string leaguename;

        public string Leaguename
        {
            get { return leaguename; }
            set { leaguename = value; }
        }

        private List<ChallengeData> challengeDatas;

        public List<ChallengeData> ChallengeDatas
        {
            get { return challengeDatas; }
            set { challengeDatas = value; }
        }

        private DateTime leagueEndsOn;

        public DateTime LeagueEndsOn
        {
            get { return leagueEndsOn; }
            set { leagueEndsOn = value; }
        }

        private DateTime leagueStartedOn;

        public DateTime LeagueStartedOn
        {
            get { return leagueStartedOn; }
            set { leagueStartedOn = value; }
        }




    }
}
