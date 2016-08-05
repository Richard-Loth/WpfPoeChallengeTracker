using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPoeChallengeTracker.model.persistence
{
    public class League
    {
        private string inAppName;

        public string InAppName
        {
            get { return inAppName; }
            set { inAppName = value; }
        }

        private string urlName;

        public string UrlName
        {
            get { return urlName; }
            set { urlName = value; }
        }

        private string xmlFilePath;

        public string XmlFilePath
        {
            get { return xmlFilePath; }
            set { xmlFilePath = value; }
        }

        private string progressFileName;

        public string ProgressFileName
        {
            get { return progressFileName; }
            set { progressFileName = value; }
        }
    }
}
