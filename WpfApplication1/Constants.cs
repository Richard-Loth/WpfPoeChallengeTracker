using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace WpfPoeChallengeTracker
{
    class ColorConstants
    {
        public static readonly SolidColorBrush SubChallengeIsDone = new SolidColorBrush(Color.FromArgb(255, 10, 180, 9));
        public static readonly SolidColorBrush SubChallengeIsNotDone = new SolidColorBrush(Color.FromArgb(255, 212, 165, 109));
        public static readonly SolidColorBrush ChallengeIsDoneCheckColor = new SolidColorBrush(Color.FromArgb(255, 12, 195, 16));
        public static readonly SolidColorBrush ChallengeIsNotDoneCheckColor = new SolidColorBrush(Color.FromArgb(255, 200, 7, 7));

        private ColorConstants()
        {

        }

    }

    class ErrorMessages
    {
        public const string PROGRESS_DIFFERENT_SIZE = "Challenge data and progress have different sizes";
        public const string SUBPROGRESS_DIFFERENT_SIZE = "Challenge has different subchallengesize then subprogressessize";

        private ErrorMessages()
        {

        }
    }
}
