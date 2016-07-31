using WpfPoeChallengeTracker.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPoeChallengeTracker.viewmodel
{
   public class SubChallengeInfoView
    {
        public bool HasUrl
        {
            get
            {
                return data.UrlAsString.Length > 0;
            }

        }

        private SubChallengeInfo data;

        public SubChallengeInfo Data
        {
            set { data = value; }
        }

        public string Text
        {
            get
            {
                return data.Text;
            }

        }

        Uri uri;
        public Uri Url
        {
            get
            {
                return uri;
            }
        }

        public SubChallengeInfoView(SubChallengeInfo subSubInfo)
        {
            data = subSubInfo;
            string url = data.UrlAsString;
            if (url.Length >0)
            {
                uri = new Uri(url);
            }
        }
    }
}
