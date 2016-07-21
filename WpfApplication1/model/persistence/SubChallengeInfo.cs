using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poe_Challenge_Tracker.model
{
  public  class SubChallengeInfo
    {
        

        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        private string urlAsString;

        public string UrlAsString
        {
            get { return urlAsString; }
            set { urlAsString = value; }
        }

        public SubChallengeInfo(string text, string urlString)
        {
            Text = text;
            UrlAsString = urlString;
        }

        public SubChallengeInfo() : this("","")
        {

        }
    }
}
